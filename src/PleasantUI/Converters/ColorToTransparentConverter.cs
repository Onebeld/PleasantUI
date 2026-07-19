using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PleasantUI.Converters;

/// <summary>
/// A converter class for converting a color to its transparent counterpart
/// </summary>
public class ColorToTransparentConverter : IValueConverter
{
    /// <summary>
    /// An instance of the <see cref="ColorToTransparentConverter"/> class object
    /// </summary>
    public static readonly ColorToTransparentConverter Instance = new();
    
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Color color)
            return new SolidColorBrush(Color.FromArgb(0, color.R, color.G, color.B));

        return AvaloniaProperty.UnsetValue;
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}