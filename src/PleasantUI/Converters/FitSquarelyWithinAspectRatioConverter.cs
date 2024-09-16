using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace PleasantUI.Converters;

/// <summary>
/// A value converter that calculates the size of a square that fits squarely within a given rectangle.
/// </summary>
public class FitSquarelyWithinAspectRatioConverter : IValueConverter
{
    /// <summary>
    /// A static instance of the <see cref="FitSquarelyWithinAspectRatioConverter"/> class.
    /// </summary>
    public static readonly FitSquarelyWithinAspectRatioConverter Instance = new();

    /// <summary>
    /// Converts a <see cref="Rect"/> value to the size of a square that fits squarely within it.
    /// </summary>
    /// <param name="value">The <see cref="Rect"/> value to convert.</param>
    /// <param name="targetType">The target type of the conversion.</param>
    /// <param name="parameter">An optional parameter to be used in the conversion.</param>
    /// <param name="culture">The culture to use in the conversion.</param>
    /// <returns>The size of a square that fits squarely within the given <see cref="Rect"/>, 
    /// or <see cref="AvaloniaProperty.UnsetValue"/> if the input value is null.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return AvaloniaProperty.UnsetValue;

        Rect bounds = (Rect)value;
        return Math.Min(bounds.Width, bounds.Height);
    }

    /// <summary>
    /// Convert back is not supported by this converter.
    /// </summary>
    /// <param name="value">The value to convert back.</param>
    /// <param name="targetType">The target type of the conversion.</param>
    /// <param name="parameter">An optional parameter to be used in the conversion.</param>
    /// <param name="culture">The culture to use in the conversion.</param>
    /// <returns>Throws a <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">This method is not supported.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}