using ReHatsuonTool.Models;
using ReHatsuonTool.Localization;
using ReHatsuonTool.Services;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace ReHatsuonTool.ViewModels;

public enum PageType
{
    Setup,
    AddLines,
    Save
}

public class MainViewModel : BaseViewModel
{
    private PageType _currentPage = PageType.Setup;
    private BaseViewModel? _currentPageViewModel;
    private string _versionText;
    private bool _hasUpdate;
    private string? _updateHintText;
    private readonly CancellationTokenSource _cts = new();

    public AppConfig Config { get; }
    public SetupViewModel SetupViewModel { get; }
    public AddLinesViewModel AddLinesViewModel { get; }
    public SaveViewModel SaveViewModel { get; }

    public string VersionText
    {
        get => _versionText;
        set => SetProperty(ref _versionText, value);
    }

    public bool HasUpdate
    {
        get => _hasUpdate;
        set
        {
            if (SetProperty(ref _hasUpdate, value))
                OnPropertyChanged(nameof(UpdateHintText));
        }
    }

    public string? UpdateHintText
    {
        get => _hasUpdate ? _updateHintText : null;
        set => SetProperty(ref _updateHintText, value);
    }

    public string LicenseText { get; }

    public ICommand OpenReleasePageCommand { get; }

    /// <summary>CancellationToken that signals when the app is exiting.</summary>
    public CancellationToken AppShutdownToken => _cts.Token;

    /// <summary>Called by App.OnExit to cancel ongoing async work.</summary>
    public void CancelAsyncTasks() => _cts.Cancel();

    public MainViewModel()
    {
        Config = AppConfig.Load();
        SetupViewModel = new SetupViewModel(Config);
        AddLinesViewModel = new AddLinesViewModel(Config);
        SaveViewModel = new SaveViewModel(Config, AddLinesViewModel);

        var ver = Assembly.GetExecutingAssembly().GetName().Version!;
        _versionText = $"v{ver.ToString(3)}";
        LicenseText = Texts.LicenseInfo;

        // Seed version info so first-time users don't get an update prompt
        Config.InitializeVersionInfo(ver);

        OpenReleasePageCommand = new RelayCommand(() =>
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/wubzbz/ReHatsuonTool/releases",
                    UseShellExecute = true
                });
            }
            catch { }
        });

        SetupViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SetupViewModel.IsSetupComplete))
            {
                OnPropertyChanged(nameof(CanNavigateToAddLines));
                OnPropertyChanged(nameof(CanNavigateToSave));
            }
        };

        AddLinesViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(AddLinesViewModel.HasValidLines))
                OnPropertyChanged(nameof(CanNavigateToSave));
        };

        CurrentPageViewModel = SetupViewModel;

        // Fire-and-forget update check
        _ = CheckForUpdateAsync();
    }

    private async Task CheckForUpdateAsync()
    {
        var ct = _cts.Token;

        // Wait a moment so the window has time to render
        try { await Task.Delay(2000, ct); }
        catch (OperationCanceledException) { return; }

        if (!Config.ShouldCheckForUpdate())
        {
            Debug.WriteLine("[Update] Skipped — checked within 24h");
        }
        else
        {
            var latest = await UpdateChecker.GetLatestVersionAsync(ct: ct);
            if (latest != null)
            {
                Config.LatestVersion = latest;
                Config.LastUpdateCheck = DateTime.UtcNow;
                Config.Save();
            }
        }

        if (Version.TryParse(Config.LatestVersion, out var latestVer))
        {
            var currentVer = Assembly.GetExecutingAssembly().GetName().Version!;
            if (latestVer > currentVer)
            {
                var appDispatcher = Application.Current?.Dispatcher;
                appDispatcher?.Invoke(() =>
                    {
                        UpdateHintText = string.Format(Texts.UpdateAvailable, VersionText);
                        HasUpdate = true;
                    });
            }
        }
    }

    public PageType CurrentPage
    {
        get => _currentPage;
        set
        {
            if (SetProperty(ref _currentPage, value))
            {
                CurrentPageViewModel = value switch
                {
                    PageType.Setup => SetupViewModel,
                    PageType.AddLines => AddLinesViewModel,
                    PageType.Save => SaveViewModel,
                    _ => null!
                };

                if (value == PageType.AddLines)
                    AddLinesViewModel.Load();
                if (value == PageType.Save)
                    SaveViewModel.Load();
            }
        }
    }

    public BaseViewModel? CurrentPageViewModel
    {
        get => _currentPageViewModel;
        set => SetProperty(ref _currentPageViewModel, value);
    }

    public bool CanNavigateToAddLines => SetupViewModel.IsSetupComplete;
    public bool CanNavigateToSave => SetupViewModel.IsSetupComplete && AddLinesViewModel.HasValidLines;

    public void Navigate(PageType page)
    {
        CurrentPage = page;
    }
}
