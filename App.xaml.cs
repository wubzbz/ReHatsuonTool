using System.Text;
using System.Windows;
using Python.Runtime;

namespace HatsuonTool;

public partial class App : Application
{
    static App()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try { PythonEngine.Shutdown(); }
        catch { }
        base.OnExit(e);
    }
}
