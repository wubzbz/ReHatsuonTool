using Python.Runtime;
using System.Diagnostics;
using System.IO;

namespace ReHatsuonTool.Services;

public static class PythonHatsuonService
{
    private static string? _pythonDir;
    private static bool _initialized;

    private static string PythonDir => _pythonDir ??= FindPythonDir();
    private static string PythonDll => Path.Combine(PythonDir, "python313.dll");
    private static string LibSitePackages => Path.Combine(PythonDir, "Lib", "site-packages");

    private static string FindPythonDir()
    {
        var dir = AppDomain.CurrentDomain.BaseDirectory;
        while (dir != null)
        {
            var candidate = Path.Combine(dir, "python");
            if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "python313.dll")))
                return candidate;
            dir = Path.GetDirectoryName(dir);
        }
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "python");
    }

    public static bool IsAvailable => File.Exists(PythonDll);

    public static string Diagnose()
    {
        if (!IsAvailable)
            return $"Python DLL not found. Searched: {PythonDir}";
        try
        {
            EnsureInitialized();
            using (Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                string ver = sys.version.ToString();
                return $"Python.NET OK. {ver.Split('\n')[0]}";
            }
        }
        catch (Exception ex)
        {
            return $"Python.NET error: {ex.Message}";
        }
    }

    private static void EnsureInitialized()
    {
        if (_initialized) return;
        Runtime.PythonDLL = PythonDll;
        PythonEngine.Initialize();
        
        // Add site-packages to path
        using (Py.GIL())
        {
            dynamic sys = Py.Import("sys");
            sys.path.insert(0, LibSitePackages);
        }
        
        _initialized = true;
    }

    public static Dictionary<int, string> Convert(IEnumerable<(int Index, string Serif)> items)
    {
        var result = new Dictionary<int, string>();
        if (!IsAvailable) return result;

        var inputList = items.ToList();
        if (inputList.Count == 0) return result;

        try
        {
            EnsureInitialized();
            using (Py.GIL())
            {
                // Load yukkuri_mandarin (or placeholder)
                dynamic convertFunc;
                try
                {
                    dynamic ym = Py.Import("yukkurimandarin");
                    convertFunc = ym.text_convert;
                }
                catch
                {
                    // Placeholder: return serif as-is
                    foreach (var item in inputList)
                        result[item.Index] = item.Serif;
                    return result;
                }

                foreach (var item in inputList)
                {
                    try
                    {
                        var hatsuon = convertFunc(item.Serif).ToString();
                        result[item.Index] = hatsuon ?? item.Serif;
                    }
                    catch
                    {
                        result[item.Index] = item.Serif;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Python.NET] Error: {ex.Message}");
            // Fallback
            foreach (var item in inputList)
            {
                if (!result.ContainsKey(item.Index))
                    result[item.Index] = item.Serif;
            }
        }

        return result;
    }
}
