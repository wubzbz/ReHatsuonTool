using ReHatsuonTool.ViewModels;
using System.Windows;

namespace ReHatsuonTool;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        _viewModel = new MainViewModel();
        DataContext = _viewModel;
        InitializeComponent();

        // Manually trigger initial page after all controls are loaded
        NavSetup.IsChecked = true;
    }

    private void NavSetup_Checked(object sender, RoutedEventArgs e)
    {
        _viewModel.Navigate(PageType.Setup);
        SetupPage.Visibility = Visibility.Visible;
        AddLinesPage.Visibility = Visibility.Collapsed;
        SavePage.Visibility = Visibility.Collapsed;
    }

    private void NavAdd_Checked(object sender, RoutedEventArgs e)
    {
        _viewModel.Navigate(PageType.AddLines);
        SetupPage.Visibility = Visibility.Collapsed;
        AddLinesPage.Visibility = Visibility.Visible;
        SavePage.Visibility = Visibility.Collapsed;
    }

    private void NavSave_Checked(object sender, RoutedEventArgs e)
    {
        _viewModel.Navigate(PageType.Save);
        SetupPage.Visibility = Visibility.Collapsed;
        AddLinesPage.Visibility = Visibility.Collapsed;
        SavePage.Visibility = Visibility.Visible;
    }
}
