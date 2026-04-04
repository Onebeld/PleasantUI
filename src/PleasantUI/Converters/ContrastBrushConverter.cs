using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using PleasantUI.Core.Helpers;

namespace PleasantUI.Converters;

/// <summary>
/// Converts a <see cref="ISolidColorBrush"/> into a high-contrast brush.
/// </summary>
public class ContrastBrushConverter : IValueConverter
{
    /// <summary>
    /// Gets a static instance of the <see cref="ContrastBrushConverter" />.
    /// </summary>
    public static readonly ContrastBrushConverter Instance = new();
    
    /// <summary>
    /// Evaluates the brightness of the input brush and returns a contrasting color.
    /// </summary>
    /// <param name="value">The source <see cref="ISolidColorBrush"/></param>
    /// <param name="targetType">The type of the binding target property (expected to be <see cref="IBrush"/>)</param>
    /// <param name="parameter">Optional parameter</param>
    /// <param name="culture">The culture to use in the converter</param>
    /// <returns>
    /// <see cref="Brushes.Black"/> if the background is light; 
    /// <see cref="Brushes.White"/> if the background is dark.
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ISolidColorBrush brush)
        {
            Color color = brush.Color;

            double luminance = ColorHelper.GetRelativeLuminance(color);
            
            return luminance > 0.5 ? Brushes.Black : Brushes.White;
        }

        return Brushes.Black;
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}