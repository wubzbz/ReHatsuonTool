using System.IO;
using System.Text.RegularExpressions;
using ReHatsuonTool.Localization;

namespace ReHatsuonTool.Services;

public static partial class Ymm4Detector
{
    public record DetectionResult(
        bool ExeFound,
        string? ExePath,
        string? Version,
        string? CharacterSettingsPath,
        string? CharactersSummary,
        string? Error);

    public static DetectionResult Detect(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
            return Fail(Texts.DetectDirEmpty);

        if (!Directory.Exists(directory))
            return Fail(Texts.DetectDirNotFound);

        var exePath = FindExeShallow(directory);
        if (exePath == null)
            return Fail(Texts.DetectExeNotFound);

        var exeDir = Path.GetDirectoryName(exePath)!;

        var userSettingDir = Path.Combine(exeDir, "user", "setting");
        if (!Directory.Exists(userSettingDir))
            return new DetectionResult(true, exePath, null, null, null, Texts.DetectNoUserSetting);

        var versionPattern = VersionPattern();
        string[] versionDirs;
        try { versionDirs = Directory.GetDirectories(userSettingDir); }
        catch { versionDirs = []; }

        var versions = versionDirs
            .Select(Path.GetFileName)
            .Where(d => d != null && versionPattern.IsMatch(d))
            .Select(d => d!)
            .OrderByDescending(d => new Version(d))
            .ToList();

        if (versions.Count == 0)
            return new DetectionResult(true, exePath, null, null, null, Texts.DetectNoVersionFolder);

        var latestVersion = versions[0];
        var characterSettingsPath = Path.Combine(userSettingDir, latestVersion,
            "YukkuriMovieMaker.Settings.CharacterSettings.json");

        if (!File.Exists(characterSettingsPath))
            return new DetectionResult(true, exePath, latestVersion, null, null,
                string.Format(Texts.DetectNoCharSettings, latestVersion));

        var charsSummary = CharacterSettingsParser.Parse(characterSettingsPath);
        return new DetectionResult(true, exePath, latestVersion, characterSettingsPath, charsSummary, null);
    }

    private static DetectionResult Fail(string msg) => new(false, null, null, null, null, msg);

    private static string? FindExeShallow(string dir)
    {
        try { foreach (var f in Directory.GetFiles(dir, "YukkuriMovieMaker.exe")) return f; }
        catch { }

        try
        {
            foreach (var sub in Directory.GetDirectories(dir))
            {
                try { foreach (var f in Directory.GetFiles(sub, "YukkuriMovieMaker.exe")) return f; }
                catch { }
            }
        }
        catch { }

        return null;
    }

    [GeneratedRegex(@"^\d+\.\d+\.\d+\.\d+$")]
    private static partial Regex VersionPattern();
}
