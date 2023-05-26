using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using PleasantUI.Extensions;

namespace PleasantUI.Converters;

public class IntToColumnDefinitionWidthConverter : IValueConverter
{
    private static readonly Lazy<IntToColumnDefinitionWidthConverter> Lazy = new(() => new IntToColumnDefinitionWidthConverter());

    public static IntToColumnDefinitionWidthConverter Instance
    {
        get => Lazy.Value;
    }

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