using System.Windows.Media;
using ReHatsuonTool.Models;

namespace ReHatsuonTool.ViewModels;

public class LineViewModel : BaseViewModel
{
    private CharacterInfo? _character;
    private string _text = string.Empty;
    private bool _isTextValid;
    private bool _canDelete = true;
    private Brush _colorBrush = new SolidColorBrush(Colors.Gray);
    private Brush _glowBrush = new SolidColorBrush(Colors.Gray);

    public LineViewModel(CharacterInfo? defaultCharacter = null)
    {
        _character = defaultCharacter;
        UpdateBrushes();
    }

    public CharacterInfo? Character
    {
        get => _character;
        set
        {
            if (SetProperty(ref _character, value))
            {
                OnPropertyChanged(nameof(CharacterColor));
                OnPropertyChanged(nameof(CharacterName));
                UpdateBrushes();
            }
        }
    }

    public string Text
    {
        get => _text;
        set
        {
            if (SetProperty(ref _text, value))
                IsTextValid = !string.IsNullOrWhiteSpace(value);
        }
    }

    public bool IsTextValid
    {
        get => _isTextValid;
        set => SetProperty(ref _isTextValid, value);
    }

    public bool CanDelete
    {
        get => _canDelete;
        set => SetProperty(ref _canDelete, value);
    }

    public string CharacterColor => _character?.Color ?? "#CCCCCC";
    public string CharacterName => _character?.Name ?? "?";

    /// <summary>Original color for the swatch.</summary>
    public Brush ColorBrush
    {
        get => _colorBrush;
        set => SetProperty(ref _colorBrush, value);
    }

    /// <summary>Normalized color for the textbox border glow.</summary>
    public Brush GlowBrush
    {
        get => _glowBrush;
        set => SetProperty(ref _glowBrush, value);
    }

    private void UpdateBrushes()
    {
        try
        {
            var original = (Color)ColorConverter.ConvertFromString(CharacterColor);
            ColorBrush = new SolidColorBrush(original);

            // Normalize lightness: shift extreme colors toward mid-range for border
            var hsl = RgbToHsl(original);
            hsl.Lightness = Math.Clamp(hsl.Lightness, 0.50f, 0.65f);
            hsl.Saturation = Math.Clamp(hsl.Saturation, 0.45f, 0.75f);
            var balanced = HslToRgb(hsl);
            GlowBrush = new SolidColorBrush(balanced);
        }
        catch
        {
            ColorBrush = new SolidColorBrush(Colors.Gray);
            GlowBrush = new SolidColorBrush(Colors.Gray);
        }
    }

    private static (float Hue, float Saturation, float Lightness) RgbToHsl(Color c)
    {
        float r = c.R / 255f, g = c.G / 255f, b = c.B / 255f;
        float max = Math.Max(r, Math.Max(g, b));
        float min = Math.Min(r, Math.Min(g, b));
        float l = (max + min) / 2f;

        if (max == min) return (0, 0, l);

        float d = max - min;
        float s = l > 0.5f ? d / (2f - max - min) : d / (max + min);

        float h = 0;
        if (max == r) h = (g - b) / d + (g < b ? 6 : 0);
        else if (max == g) h = (b - r) / d + 2;
        else h = (r - g) / d + 4;

        return (h / 6f, s, l);
    }

    private static Color HslToRgb((float H, float S, float L) hsl)
    {
        float r, g, b;
        if (hsl.S == 0)
        {
            r = g = b = hsl.L;
        }
        else
        {
            float q = hsl.L < 0.5f ? hsl.L * (1 + hsl.S) : hsl.L + hsl.S - hsl.L * hsl.S;
            float p = 2 * hsl.L - q;
            r = HueToRgb(p, q, hsl.H + 1f / 3f);
            g = HueToRgb(p, q, hsl.H);
            b = HueToRgb(p, q, hsl.H - 1f / 3f);
        }
        return Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
    }

    private static float HueToRgb(float p, float q, float t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1f / 6f) return p + (q - p) * 6 * t;
        if (t < 1f / 2f) return q;
        if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6;
        return p;
    }
}
