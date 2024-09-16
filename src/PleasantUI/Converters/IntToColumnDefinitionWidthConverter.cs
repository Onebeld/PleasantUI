using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using PleasantUI.Extensions;

namespace PleasantUI.Converters;

/// <summary>
/// Converts an integer value to a <see cref="GridLength"/> with <see cref="GridUnitType.Pixel"/>.
/// </summary>
public class IntToColumnDefinitionWidthConverter : IValueConverter
{
    /// <summary>
    /// Gets a static instance of the converter.
    /// </summary>
    public static readonly IntToColumnDefinitionWidthConverter Instance = new();

    /// <summary>
    /// Converts an integer value to a <see cref="GridLength"/>.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    /// <param name="targetType">The target type (should be <see cref="GridLength"/>).</param>
    /// <param name="parameter">Not used.</param>
    /// <param name="culture">Not used.</param>
    /// <returns>A <see cref="GridLength"/> representing the integer value in pixels.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        double d = value!.ToString()!.GetDouble();
        return new GridLength(d, GridUnitType.Pixel);
    }

    /// <summary>
    /// Convert back is not supported.
    /// </summary>
    /// <param name="value">The value to convert back.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">A parameter.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>Not supported.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}