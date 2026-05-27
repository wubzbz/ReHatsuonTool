using System.IO;
using System.Text.Json;

namespace ReHatsuonTool.Models;

public class AppConfig
{
    public string? Ymm4Directory { get; set; }
    public string? YmmpFilePath { get; set; }
    public string? DetectedVersion { get; set; }
    public string? CharacterSettingsPath { get; set; }

    // Update checking
    public string? LatestVersion { get; set; }
    public DateTime? LastUpdateCheck { get; set; }

    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Re-HatsuonTool",
        "config.json");

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static AppConfig Load()
    {
        try
        {
            var dir = Path.GetDirectoryName(ConfigPath);
            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
            }
        }
        catch { }
        return new AppConfig();
    }

    public void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(ConfigPath);
            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(this, JsonOptions);
            File.WriteAllText(ConfigPath, json);
        }
        catch { }
    }

    /// <summary>
    /// Returns true if at least 24 hours have passed since the last update check.
    /// </summary>
    public bool ShouldCheckForUpdate()
    {
        if (LastUpdateCheck == null) return true;
        return (DateTime.UtcNow - LastUpdateCheck.Value).TotalHours >= 24;
    }

    /// <summary>
    /// On first run there is no cached LatestVersion — seed it with the current version
    /// so the user isn't prompted to update immediately.
    /// </summary>
    public void InitializeVersionInfo(Version currentVersion)
    {
        if (string.IsNullOrEmpty(LatestVersion) || LastUpdateCheck == null)
        {
            LatestVersion = currentVersion.ToString(3);
            LastUpdateCheck = DateTime.UtcNow;
            Save();
        }
    }
}
