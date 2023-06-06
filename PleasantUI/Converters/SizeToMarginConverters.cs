using Avalonia;
using Avalonia.Data.Converters;

namespace PleasantUI.Converters;

public class SizeToMarginConverters
{
    public static readonly IValueConverter HeightToTopMargin =
        new FuncValueConverter<double, Thickness>(value => new Thickness(0, value, 0, 0));
}