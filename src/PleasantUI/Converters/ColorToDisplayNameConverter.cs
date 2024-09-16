using System.Globalization;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PleasantUI.Converters;

/// <summary>
/// Gets the approximated display name for the color.
/// </summary>
public class ColorToDisplayNameConverter : IValueConverter
{
    /// <summary>
    /// Gets an instance of the <see cref="ColorToDisplayNameConverter" /> class.
    /// </summary>
    public static readonly ColorToDisplayNameConverter Instance = new();

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Color color;

        switch (value)
        {
            case Color valueColor:
                color = valueColor;
                break;
            case HslColor valueHslColor:
                color = valueHslColor.ToRgb();
                break;
            case HsvColor valueHsvColor:
                color = valueHsvColor.ToRgb();
                break;
            case SolidColorBrush valueBrush:
                color = valueBrush.Color;
                break;
            case uint valueUInt:
                color = Color.FromUInt32(valueUInt);
                break;
            default:
                // Invalid color value provided
                return AvaloniaProperty.UnsetValue;
        }

        // ColorHelper.ToDisplayName ignores the alpha component
        // This means fully transparent colors will be named as a real color
        // That undesirable behavior is specially overridden here
        if (color.A == 0x00)
            return AvaloniaProperty.UnsetValue;

        return ColorHelper.ToDisplayName(color);
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return AvaloniaProperty.UnsetValue;
    }
}