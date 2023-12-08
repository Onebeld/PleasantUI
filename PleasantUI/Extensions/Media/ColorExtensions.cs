using Avalonia.Media;
using Avalonia.Utilities;

namespace PleasantUI.Extensions.Media;

public static class ColorExtensions
{
    private const float Epsilon = 0.001f;

    /// <summary>
    /// Returns a new color that is lightened by the given percent.
    /// </summary>
    /// <param name="color">The color to be lightened.</param>
    /// <param name="percent">The percentage by which to lighten the color. Positive values lighten the color, while negative values darken the color.</param>
    /// <returns>The resulting lightened color.</returns>
    public static Color GetLightenPercent(this Color color, float percent)
    {
        HslColor hslColor = color.ToHsl();

        double lightness = hslColor.L < Epsilon ? percent : hslColor.L + (hslColor.L * percent);
        MathUtilities.Clamp(lightness, 0.6, 0.7);

        return HslColor.ToRgb(hslColor.H, hslColor.S, lightness, hslColor.A);
    }
}