using ReHatsuonTool.Localization;
using System.IO;
using System.Text.Json;

namespace ReHatsuonTool.Services;

public static class YmmpValidator
{
    public record ValidationResult(bool IsValid, string? ErrorMessage, int? VoiceItemCount, List<string>? TimelineNames);

    public static ValidationResult Validate(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return new ValidationResult(false, Texts.YmmpFileNotFound, null, null);

            var json = File.ReadAllText(filePath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("Timelines", out var timelines))
                return new ValidationResult(false, Texts.YmmpNoTimelines, null, null);

            if (timelines.ValueKind != JsonValueKind.Array || timelines.GetArrayLength() == 0)
                return new ValidationResult(false, Texts.YmmpTimelinesEmpty, null, null);

            var names = new List<string>();
            int totalVoiceItems = 0;

            foreach (var timeline in timelines.EnumerateArray())
            {
                var name = "?";
                if (timeline.TryGetProperty("Name", out var nameEl))
                    name = nameEl.GetString() ?? "?";

                if (!timeline.TryGetProperty("Items", out var items))
                    return new ValidationResult(false, string.Format(Texts.YmmpTimelineMissingItems, name), null, null);

                if (items.ValueKind != JsonValueKind.Array)
                    return new ValidationResult(false, string.Format(Texts.YmmpItemsNotArray, name), null, null);

                int voiceCount = 0;
                foreach (var item in items.EnumerateArray())
                {
                    if (item.TryGetProperty("$type", out var typeEl))
                    {
                        var type = typeEl.GetString();
                        if (type?.Contains("VoiceItem") == true)
                            voiceCount++;
                    }
                }
                totalVoiceItems += voiceCount;
                names.Add(string.Format(Texts.YmmpTimelineItemFormat, name, voiceCount));
            }

            return new ValidationResult(true, null, totalVoiceItems, names);
        }
        catch (JsonException ex)
        {
            return new ValidationResult(false, string.Format(Texts.YmmpJsonInvalid, ex.Message), null, null);
        }
        catch (Exception ex)
        {
            return new ValidationResult(false, string.Format(Texts.YmmpReadFailed, ex.Message), null, null);
        }
    }
}
