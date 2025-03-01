using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PleasantUI.Converters;

/// <summary>
/// Converts a color to a transparent color by setting its alpha channel to zero.
/// </summary>
public class ColorToTransparentConverter : IMultiValueConverter
{
    /// <summary>
    /// Converts a color to a transparent brush.
    /// </summary>
    /// <param name="values">An array where the first element is expected to be a <see cref="Color"/>.</param>
    /// <param name="targetType">The target type of the binding (not used).</param>
    /// <param name="parameter">An optional parameter (not used).</param>
    /// <param name="culture">The culture to use in the converter (not used).</param>
    /// <returns>A <see cref="SolidColorBrush"/> with a transparent version of the input color, or <see cref="AvaloniaProperty.UnsetValue"/> if the conversion fails.</returns>
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is Color color)
            return new SolidColorBrush(Color.FromArgb(0, color.R, color.G, color.B));

        return AvaloniaProperty.UnsetValue;
    }
}
