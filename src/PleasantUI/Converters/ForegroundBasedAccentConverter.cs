using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using PleasantUI.Core.Helpers;

namespace PleasantUI.Converters;

/// <summary>
/// Converts a foreground color to an accent color based on its luminance.
/// </summary>
public class ForegroundBasedAccentConverter : IMultiValueConverter
{
    /// <summary>
    /// Determines an appropriate accent color (black or white) based on the luminance of the input color.
    /// </summary>
    /// <param name="values">An array where the first element is expected to be a <see cref="Color"/>.</param>
    /// <param name="targetType">The target type of the binding (not used).</param>
    /// <param name="parameter">An optional parameter (not used).</param>
    /// <param name="culture">The culture to use in the converter (not used).</param>
    /// <returns>A <see cref="SolidColorBrush"/> with either black or white, depending on the luminance of the input color, or <see cref="AvaloniaProperty.UnsetValue"/> if conversion fails.</returns>
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is not Color color) return AvaloniaProperty.UnsetValue;

        double lum = ColorHelper.GetRelativeLuminance(color);
        return new SolidColorBrush(lum <= 0.2 ? Colors.White : Colors.Black);
    }
}