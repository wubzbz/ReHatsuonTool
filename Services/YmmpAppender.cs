using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ReHatsuonTool.Services;

public static class YmmpAppender
{
    public record AppendResult(bool Success, string? Error, int AppendedCount);

    public static AppendResult Append(
        string ymmpPath,
        string targetTimelineName,
        string? globalCharacterSettingsPath,
        IEnumerable<(string CharacterName, string Serif, string Hatsuon)> lines)
    {
        if (!File.Exists(ymmpPath))
            return new AppendResult(false, "ymmp file not found", 0);

        var validLines = lines.Where(l => !string.IsNullOrWhiteSpace(l.Serif)).ToList();
        if (validLines.Count == 0)
            return new AppendResult(false, "no valid lines", 0);

        try
        {
            var json = File.ReadAllText(ymmpPath);
            var root = JsonNode.Parse(json);
            if (root == null) return new AppendResult(false, "JSON parse failed", 0);

            var timelines = root["Timelines"]?.AsArray();
            if (timelines == null || timelines.Count == 0)
                return new AppendResult(false, "no Timelines found", 0);

            // Find target timeline by name
            JsonNode? targetTimeline = null;
            foreach (var tl in timelines)
            {
                var name = tl?["Name"]?.GetValue<string>() ?? "";
                if (name == targetTimelineName)
                {
                    targetTimeline = tl;
                    break;
                }
            }
            targetTimeline ??= timelines[0];
            if (targetTimeline == null)
                return new AppendResult(false, "timeline not found", 0);

            var items = targetTimeline["Items"]?.AsArray();
            if (items == null)
            {
                items = [];
                targetTimeline["Items"] = items;
            }

            // Build character config map, adding missing characters from global settings
            var requiredChars = validLines.Select(l => l.CharacterName).Distinct().ToList();
            var charMap = EnsureCharacters(root, globalCharacterSettingsPath, requiredChars);

            // Read FPS
            int fps = 60;
            var fpsNode = targetTimeline["VideoInfo"]?["FPS"];
            if (fpsNode != null) fps = fpsNode.GetValue<int>();

            // Calculate start frame
            int startFrame = 0;
            if (items.Count > 0)
            {
                var lastItem = items[^1];
                var lastFrame = lastItem?["Frame"]?.GetValue<int>() ?? 0;
                var lastLength = lastItem?["Length"]?.GetValue<int>() ?? 300;
                startFrame = lastFrame + lastLength;
            }

            int frameDuration = fps * 1; // 1 second default per item
            int currentFrame = startFrame;

            foreach (var line in validLines)
            {
                var charConfig = charMap.GetValueOrDefault(line.CharacterName);
                JsonObject newItem = CreateVoiceItem(line, charConfig, currentFrame, frameDuration);
                items.Add(newItem);
                currentFrame += frameDuration;
            }

            // Update timeline Length and MaxLayer
            UpdateTimelineMeta(targetTimeline, fps);

            // Write back with Japanese text preserved
            var options = new JsonSerializerOptions
            {
                TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver(),
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(
                    System.Text.Unicode.UnicodeRanges.All)
            };
            File.WriteAllText(ymmpPath, root.ToJsonString(options));

            return new AppendResult(true, null, validLines.Count);
        }
        catch (Exception ex)
        {
            return new AppendResult(false, ex.Message, 0);
        }
    }

    private static Dictionary<string, JsonNode> EnsureCharacters(
        JsonNode root, string? globalSettingsPath, List<string> requiredNames)
    {
        var map = new Dictionary<string, JsonNode>();
        var charsArray = root["Characters"]?.AsArray();
        if (charsArray == null)
        {
            charsArray = new JsonArray();
            root["Characters"] = charsArray;
        }

        // Index existing characters
        foreach (var c in charsArray)
        {
            var name = c?["Name"]?.GetValue<string>();
            if (name != null) map[name] = c!;
        }

        // Find missing characters from global settings
        var missing = requiredNames.Where(n => !map.ContainsKey(n)).ToList();
        if (missing.Count > 0 && !string.IsNullOrEmpty(globalSettingsPath) && File.Exists(globalSettingsPath))
        {
            try
            {
                var globalJson = File.ReadAllText(globalSettingsPath);
                var globalRoot = JsonNode.Parse(globalJson);
                var globalChars = globalRoot?["Characters"]?.AsArray();
                if (globalChars != null)
                {
                    foreach (var name in missing)
                    {
                        JsonNode? match = null;
                        foreach (var gc in globalChars)
                        {
                            if (gc?["Name"]?.GetValue<string>() == name)
                            {
                                match = gc!.DeepClone();
                                break;
                            }
                        }
                        if (match != null)
                        {
                            charsArray.Add(match);
                            map[name] = match;
                        }
                    }
                }
            }
            catch { }
        }

        return map;
    }

    private static JsonObject CreateVoiceItem(
        (string CharacterName, string Serif, string Hatsuon) line,
        JsonNode? charConfig,
        int frame,
        int length)
    {
        var item = new JsonObject
        {
            ["$type"] = "YukkuriMovieMaker.Project.Items.VoiceItem, YukkuriMovieMaker",
            ["IsWaveformEnabled"] = false,
            ["CharacterName"] = line.CharacterName,
            ["Serif"] = line.Serif,
            ["Hatsuon"] = line.Hatsuon,
            ["Decorations"] = JsonValue.Create(Array.Empty<object>()),
            ["Pronounce"] = new JsonObject
            {
                ["$type"] = "YukkuriMovieMaker.Voice.AquesTalkVoicePronounce, YukkuriMovieMaker"
            },
            ["Frame"] = frame,
            ["Layer"] = charConfig?["Layer"]?.DeepClone() ?? 0,
            ["Length"] = length,
            ["KeyFrames"] = new JsonObject { ["Frames"] = JsonValue.Create(Array.Empty<object>()), ["Count"] = 0 },
            ["Group"] = 0,
            ["Remark"] = "",
            ["IsLocked"] = false,
            ["IsHidden"] = false,
            ["ContentOffset"] = "00:00:00",
            ["VoiceFadeIn"] = 0.0,
            ["VoiceFadeOut"] = 0.0,
            ["EchoIsEnabled"] = false,
            ["EchoInterval"] = 0.1,
            ["EchoAttenuation"] = 40.0,
            ["AudioEffects"] = JsonValue.Create(Array.Empty<object>()),
            ["JimakuVisibility"] = "UseCharacterSetting",
            ["Blend"] = "Normal",
            ["IsInverted"] = false,
            ["IsClippingWithObjectAbove"] = false,
            ["IsAlwaysOnTop"] = false,
            ["IsZOrderEnabled"] = false,
            ["Font"] = "",
            ["BasePoint"] = "CenterBottom",
            ["FontColor"] = "#FFFFFFFF",
            ["Style"] = "Border",
            ["StyleColor"] = "#FF000000",
            ["Bold"] = false,
            ["Italic"] = false,
            ["Underline"] = false,
            ["Strikethrough"] = false,
            ["IsTrimEndSpace"] = true,
            ["IsDevidedPerCharacter"] = false,
            ["DisplayInterval"] = 0.0,
            ["DisplayDirection"] = "FromFirst",
            ["HideInterval"] = 0.0,
            ["HideDirection"] = "FromFirst",
            ["JimakuVideoEffects"] = JsonValue.Create(Array.Empty<object>()),
            ["TachieFaceParameter"] = null,
            ["TachieFaceEffects"] = JsonValue.Create(Array.Empty<object>()),
            ["WordWrap"] = "NoWrap",
        };

        // Copy voice-related from character config
        static JsonNode? DeepCloneOrNull(JsonNode? node) => node?.DeepClone();

        item["PlaybackRate"] = charConfig?["PlaybackRate"]?.DeepClone() ?? 100.0;
        item["VoiceParameter"] = DeepCloneOrNull(charConfig?["VoiceParameter"]);
        item["Volume"] = DeepCloneOrNull(charConfig?["Volume"]);
        item["Pan"] = DeepCloneOrNull(charConfig?["Pan"]);
        item["FontSize"] = DeepCloneOrNull(charConfig?["FontSize"]);
        item["LineHeight2"] = DeepCloneOrNull(charConfig?["LineHeight2"]);
        item["LetterSpacing2"] = DeepCloneOrNull(charConfig?["LetterSpacing2"]);
        item["MaxWidth"] = DeepCloneOrNull(charConfig?["MaxWidth"]);
        item["Font"] = charConfig?["Font"]?.DeepClone() ?? "メイリオ";
        item["FontColor"] = charConfig?["FontColor"]?.DeepClone() ?? "#FFFFFFFF";
        item["Style"] = charConfig?["Style"]?.DeepClone() ?? "Border";
        item["StyleColor"] = charConfig?["StyleColor"]?.DeepClone() ?? "#FF000000";
        item["Bold"] = charConfig?["Bold"]?.DeepClone() ?? false;
        item["Italic"] = charConfig?["Italic"]?.DeepClone() ?? false;
        item["Underline"] = charConfig?["Underline"]?.DeepClone() ?? false;
        item["Strikethrough"] = charConfig?["Strikethrough"]?.DeepClone() ?? false;
        item["IsTrimEndSpace"] = charConfig?["IsTrimEndSpace"]?.DeepClone() ?? true;

        // Ensure animation properties use default values if missing
        EnsureAnimationDefault(item, "X", 0.0);
        EnsureAnimationDefault(item, "Y", 0.0);
        EnsureAnimationDefault(item, "Z", 0.0);
        EnsureAnimationDefault(item, "Opacity", 100.0);
        EnsureAnimationDefault(item, "Zoom", 100.0);
        EnsureAnimationDefault(item, "Rotation", 0.0);

        return item;
    }

    private static void EnsureAnimationDefault(JsonObject item, string key, double defaultValue)
    {
        if (item[key] != null) return;
        item[key] = new JsonObject
        {
            ["Values"] = new JsonArray { new JsonObject { ["Value"] = defaultValue } },
            ["Span"] = 0.0,
            ["AnimationType"] = "なし",
            ["Bezier"] = DefaultBezier()
        };
    }

    private static JsonObject DefaultBezier() => new()
    {
        ["Points"] = new JsonArray
        {
            new JsonObject
            {
                ["Point"] = new JsonObject { ["X"] = 0.0, ["Y"] = 0.0 },
                ["ControlPoint1"] = new JsonObject { ["X"] = -0.3, ["Y"] = -0.3 },
                ["ControlPoint2"] = new JsonObject { ["X"] = 0.3, ["Y"] = 0.3 }
            },
            new JsonObject
            {
                ["Point"] = new JsonObject { ["X"] = 1.0, ["Y"] = 1.0 },
                ["ControlPoint1"] = new JsonObject { ["X"] = -0.3, ["Y"] = -0.3 },
                ["ControlPoint2"] = new JsonObject { ["X"] = 0.3, ["Y"] = 0.3 }
            }
        },
        ["IsQuadratic"] = false
    };

    private static void UpdateTimelineMeta(JsonNode timeline, int fps)
    {
        var items = timeline["Items"]?.AsArray();
        if (items == null || items.Count == 0) return;

        int maxFrame = 0;
        int maxLayer = 0;
        foreach (var item in items)
        {
            var f = item?["Frame"]?.GetValue<int>() ?? 0;
            var l = item?["Length"]?.GetValue<int>() ?? 0;
            var layer = item?["Layer"]?.GetValue<int>() ?? 0;
            if (f + l > maxFrame) maxFrame = f + l;
            if (layer > maxLayer) maxLayer = layer;
        }
        timeline["Length"] = Math.Max(maxFrame, 1);
        timeline["MaxLayer"] = maxLayer;
    }
}
