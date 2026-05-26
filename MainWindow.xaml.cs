using ReHatsuonTool.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ReHatsuonTool;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private FrameworkElement? _currentPageGrid;

    public MainWindow()
    {
        _viewModel = new MainViewModel();
        DataContext = _viewModel;
        InitializeComponent();

        _currentPageGrid = SetupPage;
        SetupPage.Opacity = 1.0;

        // Manually trigger initial page after all controls are loaded
        NavSetup.IsChecked = true;
    }

    private void SwitchPage(FrameworkElement showPage)
    {
        if (_currentPageGrid == showPage)
            return;

        var hidePage = _currentPageGrid;

        if (hidePage != null)
        {
            var fadeOut = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(150));
            fadeOut.Completed += (_, _) =>
            {
                hidePage.Visibility = Visibility.Collapsed;

                showPage.Visibility = Visibility.Visible;
                showPage.Opacity = 0.0;
                var fadeIn = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(150));
                showPage.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };
            hidePage.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
        else
        {
            showPage.Visibility = Visibility.Visible;
            showPage.Opacity = 1.0;
        }

        _currentPageGrid = showPage;
    }

    private void NavSetup_Checked(object sender, RoutedEventArgs e)
    {
        _viewModel.Navigate(PageType.Setup);
        SwitchPage(SetupPage);
    }

    private void NavAdd_Checked(object sender, RoutedEventArgs e)
    {
        _viewModel.Navigate(PageType.AddLines);
        SwitchPage(AddLinesPage);
    }

    private void NavSave_Checked(object sender, RoutedEventArgs e)
    {
        _viewModel.Navigate(PageType.Save);
        SwitchPage(SavePage);
    }
}
