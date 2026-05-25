using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using ReHatsuonTool.Localization;
using ReHatsuonTool.Models;
using ReHatsuonTool.Services;

namespace ReHatsuonTool.ViewModels;

public class SetupViewModel : BaseViewModel
{
    private readonly AppConfig _config;
    private string _ymm4Directory = string.Empty;
    private string _ymmpFilePath = string.Empty;
    private string _ymm4Status = string.Empty;
    private string _ymmpStatus = string.Empty;
    private string _versionInfo = string.Empty;
    private string _charactersSummary = string.Empty;
    private bool _isYmm4Valid;
    private bool _isYmmpValid;
    private bool _isSetupComplete;

    public SetupViewModel(AppConfig config)
    {
        _config = config;

        _ymm4Directory = config.Ymm4Directory ?? string.Empty;
        _ymmpFilePath = config.YmmpFilePath ?? string.Empty;

        BrowseYmm4Command = new RelayCommand(BrowseYmm4);
        BrowseYmmpCommand = new RelayCommand(BrowseYmmp);

        if (!string.IsNullOrEmpty(_ymm4Directory))
            ValidateYmm4();
        if (!string.IsNullOrEmpty(_ymmpFilePath))
            ValidateYmmp();
    }

    public string Ymm4Directory
    {
        get => _ymm4Directory;
        set
        {
            if (SetProperty(ref _ymm4Directory, value))
            {
                _config.Ymm4Directory = value;
                _config.Save();
                ValidateYmm4();
            }
        }
    }

    public string YmmpFilePath
    {
        get => _ymmpFilePath;
        set
        {
            if (SetProperty(ref _ymmpFilePath, value))
            {
                _config.YmmpFilePath = value;
                _config.Save();
                ValidateYmmp();
            }
        }
    }

    public string Ymm4Status
    {
        get => _ymm4Status;
        set => SetProperty(ref _ymm4Status, value);
    }

    public string YmmpStatus
    {
        get => _ymmpStatus;
        set => SetProperty(ref _ymmpStatus, value);
    }

    public string VersionInfo
    {
        get => _versionInfo;
        set => SetProperty(ref _versionInfo, value);
    }

    public string CharactersSummary
    {
        get => _charactersSummary;
        set => SetProperty(ref _charactersSummary, value);
    }

    public bool IsYmm4Valid
    {
        get => _isYmm4Valid;
        set => SetProperty(ref _isYmm4Valid, value);
    }

    public bool IsYmmpValid
    {
        get => _isYmmpValid;
        set => SetProperty(ref _isYmmpValid, value);
    }

    public bool IsSetupComplete
    {
        get => _isSetupComplete;
        set => SetProperty(ref _isSetupComplete, value);
    }

    public ICommand BrowseYmm4Command { get; }
    public ICommand BrowseYmmpCommand { get; }

    private void ValidateYmm4()
    {
        var result = Ymm4Detector.Detect(_ymm4Directory);
        IsYmm4Valid = result.ExeFound && result.Version != null;

        if (result.ExeFound)
        {
            Ymm4Status = result.ExePath!;
            if (result.Version != null)
            {
                VersionInfo = string.Format(Texts.DetectVersionFormat, result.Version);
                CharactersSummary = result.CharactersSummary ?? "";
                _config.DetectedVersion = result.Version;
                _config.CharacterSettingsPath = result.CharacterSettingsPath;
                _config.Save();
            }
            else
            {
                VersionInfo = result.Error ?? Texts.DetectVersionFailed;
                CharactersSummary = "";
            }
        }
        else
        {
            Ymm4Status = Texts.YmmpErrorPrefix + (result.Error ?? "");
            VersionInfo = "";
        }

        UpdateSetupComplete();
    }

    private void ValidateYmmp()
    {
        var result = YmmpValidator.Validate(_ymmpFilePath);
        IsYmmpValid = result.IsValid;

        if (result.IsValid)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(string.Format(Texts.YmmpSummaryFormat, result.TimelineNames?.Count ?? 0, result.VoiceItemCount));
            if (result.TimelineNames != null)
            {
                foreach (var name in result.TimelineNames)
                    sb.AppendLine("    " + name);
            }
            YmmpStatus = sb.ToString().TrimEnd();
        }
        else
        {
            YmmpStatus = Texts.YmmpErrorPrefix + (result.ErrorMessage ?? "");
        }

        UpdateSetupComplete();
    }

    private void UpdateSetupComplete()
    {
        IsSetupComplete = IsYmm4Valid && IsYmmpValid;
    }

    private void BrowseYmm4()
    {
        var dialog = new OpenFolderDialog
        {
            Title = Texts.DlgYmm4Title,
            InitialDirectory = _ymm4Directory
        };

        if (dialog.ShowDialog() == true)
        {
            Ymm4Directory = dialog.FolderName;
        }
    }

    private void BrowseYmmp()
    {
        var dialog = new OpenFileDialog
        {
            Title = Texts.DlgYmmpTitle,
            Filter = Texts.DlgYmmpFilter,
            InitialDirectory = Path.GetDirectoryName(_ymmpFilePath),
            FileName = Path.GetFileName(_ymmpFilePath)
        };

        if (dialog.ShowDialog() == true)
        {
            YmmpFilePath = dialog.FileName;
        }
    }
}
