using Microsoft.Win32;
using ReHatsuonTool.Models;
using ReHatsuonTool.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Input;

namespace ReHatsuonTool.ViewModels;

public class AddLinesViewModel : BaseViewModel
{
    private readonly AppConfig _config;
    private List<CharacterInfo> _allCharacters = [];
    private CharacterInfo? _lastCharacter;

    private string? _selectedTimelineName;
    private bool _isLoaded;
    private bool _hasValidLines;

    public AddLinesViewModel(AppConfig config)
    {
        _config = config;
        ImportCsvCommand = new RelayCommand(ImportCsv);
        DeleteLineCommand = new RelayCommand<LineViewModel>(DeleteLine);
    }

    public ObservableCollection<LineViewModel> Lines { get; } = [];
    public ObservableCollection<string> TimelineNames { get; } = [];
    private readonly List<string> _rawTimelineNames = [];
    public ICommand ImportCsvCommand { get; }
    public ICommand DeleteLineCommand { get; }

    public string? SelectedTimelineName
    {
        get => _selectedTimelineName;
        set => SetProperty(ref _selectedTimelineName, value);
    }

    public string? RawTimelineName
    {
        get
        {
            var idx = TimelineNames.IndexOf(SelectedTimelineName ?? "");
            return idx >= 0 && idx < _rawTimelineNames.Count
                ? _rawTimelineNames[idx] : SelectedTimelineName;
        }
    }

    public bool IsLoaded
    {
        get => _isLoaded;
        set => SetProperty(ref _isLoaded, value);
    }

    public bool HasValidLines
    {
        get => _hasValidLines;
        set => SetProperty(ref _hasValidLines, value);
    }

    public IEnumerable<LineViewModel> ValidLines =>
        Lines.Where(l => !string.IsNullOrWhiteSpace(l.Text) && l.Character != null);

    public List<CharacterInfo> AllCharacters => _allCharacters;

    public void Load()
    {
        if (!_isLoaded)
        {
            var path = _config.CharacterSettingsPath;
            if (!string.IsNullOrEmpty(path))
                _allCharacters = CharacterLoader.Load(path);
            _isLoaded = true;
        }

        var currentSelection = SelectedTimelineName;
        TimelineNames.Clear();
        _rawTimelineNames.Clear();

        var ymmpPath = _config.YmmpFilePath;
        if (!string.IsNullOrEmpty(ymmpPath))
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(File.ReadAllText(ymmpPath));
                var timelines = doc.RootElement.GetProperty("Timelines");
                foreach (var tl in timelines.EnumerateArray())
                {
                    var name = tl.GetProperty("Name").GetString() ?? "?";
                    _rawTimelineNames.Add(name);

                    // Count VoiceItems for display
                    int count = 0;
                    if (tl.TryGetProperty("Items", out var items))
                    {
                        foreach (var item in items.EnumerateArray())
                        {
                            if (item.TryGetProperty("$type", out var t) &&
                                t.GetString()?.Contains("VoiceItem") == true)
                                count++;
                        }
                    }
                    TimelineNames.Add(name);
                }
            }
            catch { }
        }

        if (currentSelection != null && TimelineNames.Contains(currentSelection))
            SelectedTimelineName = currentSelection;
        else
            SelectedTimelineName = TimelineNames.FirstOrDefault();

        if (Lines.Count == 0)
            AddNewLineInternal(null);
    }

    public void AddNewLine() => AddNewLineInternal(_lastCharacter);

    private void AddNewLineInternal(CharacterInfo? defaultChar)
    {
        // Previous last line becomes deletable
        if (Lines.Count > 0) Lines[^1].CanDelete = true;

        // Default to first character if none specified
        defaultChar ??= _allCharacters.FirstOrDefault();

        var line = new LineViewModel(defaultChar) { CanDelete = false };
        line.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(LineViewModel.IsTextValid) && line.IsTextValid)
            {
                _lastCharacter = line.Character;
                if (line == Lines[^1] && !string.IsNullOrWhiteSpace(line.Text))
                    AddNewLineInternal(_lastCharacter);
                RefreshHasValidLines();
            }
        };
        Lines.Add(line);
        RefreshHasValidLines();
    }

    private static (string[] lines, Encoding encoding) ReadCsvLines(
        byte[] bytes, Dictionary<string, CharacterInfo> charMap)
    {
        Encoding[] encodings =
        [
            Encoding.UTF8,
            Encoding.GetEncoding("shift_jis"),
            Encoding.GetEncoding("gbk"),
        ];

        foreach (var enc in encodings)
        {
            try
            {
                var text = enc.GetString(bytes);
                // Strip BOM if present (UTF-8 BOM = U+FEFF)
                if (text.Length > 0 && text[0] == '\uFEFF')
                    text = text[1..];
                var lines = text.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);
                // Check if at least one line matches a role name
                int matches = lines.Count(l =>
                {
                    var parts = l.Split(',', 2);
                    return parts.Length >= 2 && charMap.ContainsKey(parts[0].Trim());
                });
                if (matches > 0)
                    return (lines, enc);
            }
            catch { }
        }

        // Fallback
        var fallback = Encoding.UTF8.GetString(bytes)
            .Split(["\r\n", "\n", "\r"], StringSplitOptions.None);
        return (fallback, Encoding.UTF8);
    }

    public void DeleteLine(LineViewModel? line)
    {
        if (line == null || Lines.Count <= 1) return;
        Lines.Remove(line);
        RefreshHasValidLines();
    }

    private void RefreshHasValidLines()
    {
        HasValidLines = Lines.Any(l => l.IsTextValid && l.Character != null);
    }

    private void ImportCsv()
    {
        var dialog = new OpenFileDialog
        {
            Title = "import CSV",
            Filter = "CSV file (*.csv)|*.csv|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            var charMap = _allCharacters.ToDictionary(
                c => c.Name, StringComparer.OrdinalIgnoreCase);

            // Auto-detect encoding: try all encodings, pick one matching role names
            var bytes = File.ReadAllBytes(dialog.FileName);
            var (csvLines, _) = ReadCsvLines(bytes, charMap);

            foreach (var csvLine in csvLines)
            {
                var parts = csvLine.Split(',', 2);
                if (parts.Length < 2) continue;

                var charName = parts[0].Trim();
                var text = parts[1].Trim();
                if (string.IsNullOrEmpty(text)) continue;

                if (charMap.TryGetValue(charName, out var character))
                {
                    _lastCharacter = character;

                    // If last line is empty, fill it; otherwise add via internal method
                    var last = Lines[^1];
                    if (string.IsNullOrWhiteSpace(last.Text))
                    {
                        last.Character = character;
                        last.Text = text;
                    }
                    else
                    {
                        AddNewLineInternal(character);
                        Lines[^1].Text = text;
                    }
                }
            }

            // Ensure trailing empty line
            if (Lines.Count == 0 || !string.IsNullOrWhiteSpace(Lines[^1].Text))
                AddNewLineInternal(_lastCharacter);
            RefreshHasValidLines();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"CSV import failed: {ex.Message}", "Error");
        }
    }
}
