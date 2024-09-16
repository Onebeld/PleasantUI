using Avalonia.Controls.Converters;
using Avalonia.Controls.Primitives.Converters;
using Avalonia.Data.Converters;
using Avalonia.Media;
using PleasantUI.Core.Helpers;

namespace PleasantUI.Converters;

/// <summary>
/// Provides static methods and converters for working with colors.
/// </summary>
public static class ColorConverters
{
    /// <inheritdoc cref="ToBrushConverter" />
    public static readonly ToBrushConverter ToBrush = new();

    /// <inheritdoc cref="ToColorConverter" />
    public static readonly ToColorConverter ToColor = new();

    /// <inheritdoc cref="AccentColorConverter" />
    public static readonly AccentColorConverter AccentColor = new();

    /// <summary>
    /// Converts an unsigned integer value to a brush object.
    /// </summary>
    public static readonly IValueConverter UIntToBrush =
        new FuncValueConverter<uint, IBrush>(value => new SolidColorBrush(value));

    /// <summary>
    /// Converts a color value to a string representation.
    /// </summary>
    public static readonly IValueConverter ColorToString =
        new FuncValueConverter<Color, string>(value => value.ToString().ToUpper());

    /// <summary>
    /// A value converter that converts a UInt32 value to a SolidColorBrush for foreground color based on its relative
    /// luminance.
    /// </summary>
    /// <remarks>
    /// This converter is designed to be used with data binding to dynamically set the foreground color of a UI element
    /// based on the luminance of a UInt32 value.
    /// The luminance is calculated using the relative luminance formula defined in the ColorHelper.GetRelativeLuminance
    /// method.
    /// If the calculated luminance is less than or equal to 0.2, the converter returns a white SolidColorBrush. Otherwise,
    /// it returns a black SolidColorBrush.
    /// </remarks>
    public static readonly IValueConverter UIntToForeground =
        new FuncValueConverter<uint, IBrush>(value =>
        {
            double lum = ColorHelper.GetRelativeLuminance(Color.FromUInt32(value));
            return new SolidColorBrush(lum <= 0.2 ? Colors.White : Colors.Black);
        });
}