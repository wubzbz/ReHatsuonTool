using ReHatsuonTool.Models;
using ReHatsuonTool.Localization;
using System.Reflection;

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

    public AppConfig Config { get; }
    public SetupViewModel SetupViewModel { get; }
    public AddLinesViewModel AddLinesViewModel { get; }
    public SaveViewModel SaveViewModel { get; }

    public string VersionText { get; }
    public string LicenseText { get; }

    public MainViewModel()
    {
        Config = AppConfig.Load();
        SetupViewModel = new SetupViewModel(Config);
        AddLinesViewModel = new AddLinesViewModel(Config);
        SaveViewModel = new SaveViewModel(Config, AddLinesViewModel);

        var ver = Assembly.GetExecutingAssembly().GetName().Version;
        VersionText = $"v{ver?.ToString(3) ?? Texts.UnknownVer}";
        LicenseText = Texts.LicenseInfo;

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
