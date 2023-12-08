using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace PleasantUI.Converters;

public class FitSquarelyWithinAspectRatioConverter : IValueConverter
{
    public static readonly FitSquarelyWithinAspectRatioConverter Instance = new();
    
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return AvaloniaProperty.UnsetValue;
        
        Rect bounds = (Rect)value;
        return Math.Min(bounds.Width, bounds.Height);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}