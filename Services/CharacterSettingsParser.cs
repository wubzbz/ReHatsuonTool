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

            const int maxShow = 3;
            var sb = new List<string>();
            int aqTotal = 0;
            int shown = 0;

            foreach (var c in chars.EnumerateArray())
            {
                // Filter: AquesTalk only
                string? api = null;
                if (c.TryGetProperty("Voice", out var voice) && voice.ValueKind == JsonValueKind.Object)
                    api = voice.TryGetProperty("API", out var a) ? a.GetString() : null;

                if (api?.Contains("AquesTalk", StringComparison.OrdinalIgnoreCase) != true)
                    continue;

                aqTotal++;

                if (shown >= maxShow)
                    continue;

                var name = Texts.CharsUnknownName;
                if (c.TryGetProperty("Name", out var nameEl))
                    name = nameEl.GetString() ?? Texts.CharsUnknownName;

                var voiceInfo = "";
                if (c.TryGetProperty("Voice", out var voice2) && voice2.ValueKind == JsonValueKind.Object)
                {
                    var arg = voice2.TryGetProperty("Arg", out var r) ? r.GetString() : null;
                    if (!string.IsNullOrEmpty(api))
                        voiceInfo = $"  [{api}{(arg != null ? "/" + arg : "")}]";
                }

                sb.Add($"    {name}{voiceInfo}");
                shown++;
            }

            if (aqTotal == 0)
                return Texts.CharsEmpty;

            var lines = string.Join("\n", sb);
            if (aqTotal > maxShow)
                lines += "\n" + string.Format(Texts.CharsMoreFormat, aqTotal - maxShow);
            return string.Format(Texts.CharsSummaryFormat, aqTotal) + "\n" + lines;
        }
        catch
        {
            return null;
        }
    }
}
