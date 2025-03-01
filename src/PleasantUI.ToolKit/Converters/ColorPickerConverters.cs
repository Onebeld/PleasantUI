using Avalonia.Controls.Converters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.Converters;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PleasantUI.ToolKit.Converters;

/// <summary>
/// Provides converters for color-related transformations.
/// </summary>
public static class ColorPickerConverters
{
    /// <inheritdoc cref="ToBrushConverter" />
    public static readonly ToBrushConverter ToBrush = new();

    /// <inheritdoc cref="ToColorConverter" />
    public static readonly ToColorConverter ToColor = new();

    /// <inheritdoc cref="AccentColorConverter" />
    public static readonly AccentColorConverter AccentColor = new();

    /// <summary>
    /// Converts a <see cref="Color"/> to its display name or hexadecimal representation.
    /// </summary>
    public static readonly FuncValueConverter<Color, string> ColorToName = new(color =>
    {
        string name = ColorHelper.ToDisplayName(color);

        return string.IsNullOrEmpty(name) ? color.ToString() : name;
    });
}