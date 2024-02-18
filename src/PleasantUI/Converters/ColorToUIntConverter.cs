using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PleasantUI.Converters;

public class ColorToUIntConverter : IValueConverter
{
    public static readonly ColorToUIntConverter Instance = new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Color color)
            return color.ToUInt32();

        return AvaloniaProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is uint color)
            return Color.FromUInt32(color);
        
        return AvaloniaProperty.UnsetValue;
    }
}