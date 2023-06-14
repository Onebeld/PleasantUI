using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using PleasantUI.Extensions;

namespace PleasantUI.Converters;

public class IntToColumnDefinitionWidthConverter : IValueConverter
{
    public static readonly IntToColumnDefinitionWidthConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        double d = value!.ToString()!.GetDouble();
        return new GridLength(d, GridUnitType.Pixel);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}