using System.Globalization;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PleasantUI.Converters;

public class ForegroundBasedAccentConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is not Color color) return AvaloniaProperty.UnsetValue;
        
        double lum = ColorHelper.GetRelativeLuminance(color);
        return new SolidColorBrush(lum <= 0.2 ? Colors.White : Colors.Black);
    }
}