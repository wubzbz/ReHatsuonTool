using ReHatsuonTool.ViewModels;
using System.Text;
using System.Windows;
using Python.Runtime;

namespace ReHatsuonTool;

public partial class App : Application
{
    static App()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (MainWindow?.DataContext is MainViewModel vm)
            vm.CancelAsyncTasks();

        try { PythonEngine.Shutdown(); }
        catch { }
        base.OnExit(e);
    }
}
