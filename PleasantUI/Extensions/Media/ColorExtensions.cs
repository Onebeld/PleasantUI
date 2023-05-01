using Avalonia.Media;
using Avalonia.Utilities;

namespace PleasantUI.Extensions.Media;

public static class ColorExtensions
{
    private const float Epsilon = 0.001f;
    
    public static Color LightenPercent(this Color color, float percent)
    {
        HslColor hslColor = color.ToHsl();

        double lightness = hslColor.L < Epsilon ? percent : hslColor.L + (hslColor.L * percent);
        MathUtilities.Clamp(lightness, 0, 1);

        return HslColor.ToRgb(hslColor.H, hslColor.S, lightness, hslColor.A);
    }

    public static Color InvertColor(this Color color)
    {
        Color invertColor = new(
            color.A,
            (byte)(255 - color.R),
            (byte)(255 - color.G),
            (byte)(255 - color.B)
        );

        return invertColor;
    }
}