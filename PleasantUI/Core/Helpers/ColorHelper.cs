using Avalonia.Media;

namespace PleasantUI.Core.Helpers;

public static class ColorHelper
{
    /// <summary>
    /// Gets the relative (perceptual) luminance/brightness of the given color.
    /// 1 is closer to white while 0 is closer to black.
    /// </summary>
    /// <param name="color">The color to calculate relative luminance for.</param>
    /// <returns>The relative (perceptual) luminance/brightness of the given color.</returns>
    public static double GetRelativeLuminance(Color color)
    {
        // The equation for relative luminance is given by
        //
        // L = 0.2126 * Rg + 0.7152 * Gg + 0.0722 * Bg
        //
        // where Xg = { X/3294 if X <= 10, (R/269 + 0.0513)^2.4 otherwise }
        //
        // If L is closer to 1, then the color is closer to white; if it is closer to 0,
        // then the color is closer to black.  This is based on the fact that the human
        // eye perceives green to be much brighter than red, which in turn is perceived to be
        // brighter than blue.

        const double div1 = 1 / 3294.0;
        const double div2 = 1 / 269.0;

        double rg = color.R <= 10 ? color.R * div1 : Math.Pow(color.R * div2 + 0.0513, 2.4);
        double gg = color.G <= 10 ? color.G * div1 : Math.Pow(color.G * div2 + 0.0513, 2.4);
        double bg = color.B <= 10 ? color.B * div1 : Math.Pow(color.B * div2 + 0.0513, 2.4);

        return 0.2126 * rg + 0.7152 * gg + 0.0722 * bg;
    }
}