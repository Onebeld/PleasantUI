using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PleasantUI.Converters;

/// <summary>
/// Converts a <see cref="Color" /> to a <see cref="uint" /> (UInt32) representation and vice versa.
/// </summary>
public class ColorToUIntConverter : IValueConverter
{
    /// <summary>
    /// Gets a static instance of the <see cref="ColorToUIntConverter" />.
    /// </summary>
    public static readonly ColorToUIntConverter Instance = new();

    /// <summary>
    /// Converts a <see cref="Color" /> value to its <see cref="uint" /> (UInt32) representation.
    /// </summary>
    /// <param name="value">The value produced by the binding source.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>
    /// The <see cref="uint" /> representation of the color if the input is a <see cref="Color" />; otherwise,
    /// <see cref="AvaloniaProperty.UnsetValue" />.
    /// </returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Color color)
            return color.ToUInt32();

        return AvaloniaProperty.UnsetValue;
    }

    /// <summary>
    /// Converts a <see cref="uint" /> (UInt32) value back to a <see cref="Color" />.
    /// </summary>
    /// <param name="value">The value produced by the binding target.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>
    /// The <see cref="Color" /> representation of the UInt32 value if the input is a <see cref="uint" />; otherwise,
    /// <see cref="AvaloniaProperty.UnsetValue" />.
    /// </returns>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is uint color)
            return Color.FromUInt32(color);

        return AvaloniaProperty.UnsetValue;
    }
}