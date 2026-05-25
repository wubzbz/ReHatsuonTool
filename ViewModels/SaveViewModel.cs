using Microsoft.Win32;
using ReHatsuonTool.Localization;
using ReHatsuonTool.Models;
using ReHatsuonTool.Services;
using System.IO;
using System.Text;
using System.Windows.Input;

namespace ReHatsuonTool.ViewModels;

public class SaveViewModel : BaseViewModel
{
    private readonly AppConfig _config;
    private readonly AddLinesViewModel _addLinesVm;
    private string _exportSavedPath = string.Empty;
    private string _ymmpPath = string.Empty;
    private string _ymmpTimeline = string.Empty;
    private string _ymmpExistingCount = string.Empty;
    private string _ymmpStartTime = string.Empty;
    private string _appendStatus = string.Empty;
    private bool _hasExportPath;
    private bool _keepLinesAfterAppend;
    private int _validLineCount;

    public SaveViewModel(AppConfig config, AddLinesViewModel addLinesVm)
    {
        _config = config;
        _addLinesVm = addLinesVm;
        ExportCsvCommand = new RelayCommand(ExportCsv);
        AppendToYmmpCommand = new RelayCommand(AppendToYmmp);
    }

    public ICommand ExportCsvCommand { get; }
    public ICommand AppendToYmmpCommand { get; }

    public string ExportSavedPath
    {
        get => _exportSavedPath;
        set { SetProperty(ref _exportSavedPath, value); HasExportPath = !string.IsNullOrEmpty(value); }
    }

    public bool HasExportPath { get => _hasExportPath; set => SetProperty(ref _hasExportPath, value); }
    public bool KeepLinesAfterAppend
    {
        get => _keepLinesAfterAppend;
        set => SetProperty(ref _keepLinesAfterAppend, value);
    }
    public string YmmpPath { get => _ymmpPath; set => SetProperty(ref _ymmpPath, value); }
    public string YmmpTimeline { get => _ymmpTimeline; set => SetProperty(ref _ymmpTimeline, value); }
    public string YmmpExistingCount { get => _ymmpExistingCount; set => SetProperty(ref _ymmpExistingCount, value); }
    public string YmmpStartTime { get => _ymmpStartTime; set => SetProperty(ref _ymmpStartTime, value); }

    public string AppendStatus
    {
        get => _appendStatus;
        set => SetProperty(ref _appendStatus, value);
    }

    public bool CanSave => _validLineCount > 0;
    public int ValidLineCount { get => _validLineCount; set { SetProperty(ref _validLineCount, value); OnPropertyChanged(nameof(CanSave)); } }

    public void Load()
    {
        // Show Python diagnostic (only first time, don't overwrite append status)
        if (string.IsNullOrEmpty(_appendStatus) || _appendStatus.StartsWith("Python"))
            AppendStatus = PythonHatsuonService.Diagnose();

        ValidLineCount = _addLinesVm.ValidLines.Count();

        var path = _config.YmmpFilePath;
        if (!string.IsNullOrEmpty(path))
        {
            YmmpPath = path;
            YmmpTimeline = _addLinesVm.SelectedTimelineName ?? "?";
            var (Count, StartTime) = GetTimelineInfo(path, YmmpTimeline);
            YmmpExistingCount = Count;
            YmmpStartTime = StartTime;
        }
        else
        {
            YmmpPath = "(not set)";
            YmmpTimeline = "?";
            YmmpExistingCount = "?";
            YmmpStartTime = "?";
        }
    }

    private void ExportCsv()
    {
        var dialog = new SaveFileDialog
        {
            Title = "导出 CSV 台本",
            Filter = "CSV file (*.csv)|*.csv",
            DefaultExt = "csv",
            FileName = "script.csv"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            var sb = new StringBuilder();
            foreach (var line in _addLinesVm.ValidLines)
            {
                sb.Append(line.Character?.Name ?? "?");
                sb.Append(',');
                sb.Append(line.Text.Replace("\r", "").Replace("\n", "\\n"));
                sb.AppendLine();
            }

            File.WriteAllText(dialog.FileName, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            ExportSavedPath = dialog.FileName;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"CSV export failed: {ex.Message}", "Error");
        }
    }

    private void AppendToYmmp()
    {
        var path = _config.YmmpFilePath;
        var timeline = _addLinesVm.SelectedTimelineName ?? "";

        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            AppendStatus = Texts.SaveYmmpNotFound;
            return;
        }

        if (!PythonHatsuonService.IsAvailable)
        {
            AppendStatus = Texts.SavePythonUnavailable;
            return;
        }

        var validList = _addLinesVm.ValidLines.ToList();
        var serifItems = validList
            .Select((l, i) => (Index: i, Serif: l.Text)).ToList();
        var hatsuonMap = PythonHatsuonService.Convert(serifItems);

        bool anyConverted = false;
        for (int i = 0; i < validList.Count; i++)
        {
            if (hatsuonMap.TryGetValue(i, out var h) && h != validList[i].Text)
                anyConverted = true;
        }
        if (!anyConverted && validList.Count > 0)
            AppendStatus = Texts.SaveConversionFallback;

        var lines = validList.Select((l, i) => (
            CharacterName: l.Character?.Name ?? "?",
            Serif: l.Text,
            Hatsuon: hatsuonMap.TryGetValue(i, out var h) ? h : l.Text
        ));

        var result = YmmpAppender.Append(path, timeline,
            _config.CharacterSettingsPath, lines);
        if (result.Success)
        {
            var now = DateTime.Now.ToString("HH:mm:ss");
            AppendStatus = string.Format(Texts.SaveAppendSuccess, now, result.AppendedCount, timeline);

            if (!_keepLinesAfterAppend)
            {
                _addLinesVm.Lines.Clear();
                _addLinesVm.AddNewLine();
            }

            ValidLineCount = _addLinesVm.ValidLines.Count();
            var (Count, StartTime) = GetTimelineInfo(path, timeline);
            YmmpExistingCount = Count;
            YmmpStartTime = StartTime;
        }
        else
        {
            AppendStatus = string.Format(Texts.SaveYmmpAppendFailed, result.Error);
        }
    }

    private static (string Count, string StartTime) GetTimelineInfo(string ymmpPath, string timelineName)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(File.ReadAllText(ymmpPath));
            var timelines = doc.RootElement.GetProperty("Timelines");
            int fps = 60;
            foreach (var tl in timelines.EnumerateArray())
            {
                var name = tl.GetProperty("Name").GetString() ?? "";
                if (name != timelineName) continue;

                if (tl.TryGetProperty("VideoInfo", out var vi) && vi.TryGetProperty("FPS", out var f))
                    fps = f.GetInt32();

                int count = 0;
                int lastEnd = 0;
                if (tl.TryGetProperty("Items", out var items))
                {
                    foreach (var item in items.EnumerateArray())
                    {
                        if (item.TryGetProperty("$type", out var t) &&
                            t.GetString()?.Contains("VoiceItem") == true)
                            count++;

                        var frame = item.TryGetProperty("Frame", out var fr) ? fr.GetInt32() : 0;
                        var len = item.TryGetProperty("Length", out var ln) ? ln.GetInt32() : 0;
                        if (frame + len > lastEnd) lastEnd = frame + len;
                    }
                }

                var startTime = FrameToTimeString(lastEnd, fps);
                return (count.ToString(), startTime);
            }
            return ("0", "00:00:00:00");
        }
        catch { return ("?", "?"); }
    }

    private static string FrameToTimeString(int totalFrames, int fps)
    {
        if (fps <= 0) fps = 60;
        int totalSeconds = totalFrames / fps;
        int frames = totalFrames % fps;
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;
        return $"{hours:D2}:{minutes:D2}:{seconds:D2}+{frames:D2}";
    }
}
