using Avalonia.Controls.Converters;
using Avalonia.Controls.Primitives.Converters;
using Avalonia.Data.Converters;
using Avalonia.Media;
using PleasantUI.Core.Helpers;

namespace PleasantUI.Converters;

public static class ColorConverters
{
    public static readonly ToBrushConverter ToBrush = new();
    public static readonly AccentColorConverter AccentColor = new();

    public static readonly IValueConverter UIntToBrush =
        new FuncValueConverter<uint, IBrush>(value => new SolidColorBrush(value));
    public static readonly IValueConverter UIntToForeground =
        new FuncValueConverter<uint, IBrush>(value =>
        {
            double lum = ColorHelper.GetRelativeLuminance(Color.FromUInt32(value));
            return new SolidColorBrush(lum <= 0.2 ? Colors.White : Colors.Black);
        });
}