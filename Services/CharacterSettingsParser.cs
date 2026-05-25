using System.IO;
using System.Text.Json;
using ReHatsuonTool.Localization;

namespace ReHatsuonTool.Services;

public static class CharacterSettingsParser
{
    public static string? Parse(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("Characters", out var chars) || chars.ValueKind != JsonValueKind.Array)
                return null;

            int total = chars.GetArrayLength();
            if (total == 0)
                return Texts.CharsEmpty;

            const int maxShow = 3;
            var sb = new List<string>();
            int i = 0;
            foreach (var c in chars.EnumerateArray())
            {
                if (i >= maxShow) break;

                var name = Texts.CharsUnknownName;
                if (c.TryGetProperty("Name", out var nameEl))
                    name = nameEl.GetString() ?? Texts.CharsUnknownName;

                var voiceInfo = "";
                if (c.TryGetProperty("Voice", out var voice) && voice.ValueKind == JsonValueKind.Object)
                {
                    var api = voice.TryGetProperty("API", out var a) ? a.GetString() : null;
                    var arg = voice.TryGetProperty("Arg", out var r) ? r.GetString() : null;
                    if (!string.IsNullOrEmpty(api))
                        voiceInfo = $"  [{api}{(arg != null ? "/" + arg : "")}]";
                }

                sb.Add($"    {name}{voiceInfo}");
                i++;
            }

            var lines = string.Join("\n", sb);
            if (total > maxShow)
                lines += "\n" + string.Format(Texts.CharsMoreFormat, total - maxShow);
            return string.Format(Texts.CharsSummaryFormat, total) + "\n" + lines;
        }
        catch
        {
            return null;
        }
    }
}
