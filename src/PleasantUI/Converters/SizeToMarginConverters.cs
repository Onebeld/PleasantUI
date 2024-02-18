using Avalonia;
using Avalonia.Data.Converters;

namespace PleasantUI.Converters;

/// <summary>
/// Represents a class containing converters for converting size to margin values.
/// </summary>
public class SizeToMarginConverters
{
    /// <summary>
    /// Represents a value converter that converts a height value to a top margin value.
    /// </summary>
    public static readonly IValueConverter HeightToTopMargin =
        new FuncValueConverter<double, Thickness>(value => new Thickness(0, value, 0, 0));
}