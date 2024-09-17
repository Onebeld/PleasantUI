using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PleasantUI.Converters;

public class ColorToTransparentConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is Color color)
            return new SolidColorBrush(Color.FromArgb(0, color.R, color.G, color.B));

        return AvaloniaProperty.UnsetValue;
    }
}