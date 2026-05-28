using System.IO;
using System.Text.Json;
using ReHatsuonTool.Models;

namespace ReHatsuonTool.Services;

public static class CharacterLoader
{
    public static List<CharacterInfo> Load(string filePath)
    {
        var result = new List<CharacterInfo>();

        try
        {
            var json = File.ReadAllText(filePath);
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("Characters", out var chars))
                return result;

            foreach (var c in chars.EnumerateArray())
            {
                var name = "?";
                if (c.TryGetProperty("Name", out var n))
                    name = n.GetString() ?? "?";

                var color = "#FFFFFF";
                if (c.TryGetProperty("Color", out var clr))
                    color = clr.GetString() ?? "#FFFFFF";

                string? api = null, arg = null;
                if (c.TryGetProperty("Voice", out var voice) && voice.ValueKind == JsonValueKind.Object)
                {
                    api = voice.TryGetProperty("API", out var a) ? a.GetString() : null;
                    arg = voice.TryGetProperty("Arg", out var r) ? r.GetString() : null;
                }

                if (api?.Contains("AquesTalk", StringComparison.OrdinalIgnoreCase) != true)
                    continue;

                result.Add(new CharacterInfo(name, color, api, arg));
            }
        }
        catch { }

        return result;
    }
}
