using Avalonia.Controls;
using Avalonia.Controls.Converters;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace PleasantUI.Converters;

/// <summary>
/// Provides a set of converters to be used in various scenarios.
/// </summary>
public static class OtherConverters
{
    /// <inheritdoc cref="EnumToBoolConverter"/>
    public static readonly EnumToBoolConverter EnumToBool = new();
    
    /// <summary>
    /// Converts a <see cref="Bitmap"/> to an <see cref="Image"/>
    /// </summary>
    public static readonly FuncValueConverter<Bitmap?, Image?> BitmapToImage = new(bitmap =>
    {
        if (bitmap is null) return null;
        
        return new Image
        {
            Source = bitmap
        };
    });

    public static readonly FuncValueConverter<ISolidColorBrush?, ISolidColorBrush?> ForegroundFromColor = new(brush =>
    {
        if (brush is null) return null;

        double lum = ColorHelper.GetRelativeLuminance(brush.Color);
        Color foregroundColor = lum <= 0.2 ? Colors.White : Colors.Black;

        return new SolidColorBrush(foregroundColor);
    });
}