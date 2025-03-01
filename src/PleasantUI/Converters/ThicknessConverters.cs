using Avalonia;
using Avalonia.Data.Converters;

namespace PleasantUI.Converters;

/// <summary>
/// Provides a set of predefined thickness converters for setting margin, padding, or border thickness.
/// </summary>
public static class ThicknessConverters
{
    /// <summary>Applies thickness to the top only.</summary>
    public static readonly FuncValueConverter<double, Thickness> Top = new(d => new Thickness(0, d, 0, 0));

    /// <summary>Applies thickness to the left only.</summary>
    public static readonly FuncValueConverter<double, Thickness> Left = new(d => new Thickness(d, 0, 0, 0));

    /// <summary>Applies thickness to the right only.</summary>
    public static readonly FuncValueConverter<double, Thickness> Right = new(d => new Thickness(0, 0, d, 0));

    /// <summary>Applies thickness to the bottom only.</summary>
    public static readonly FuncValueConverter<double, Thickness> Bottom = new(d => new Thickness(0, 0, 0, d));

    /// <summary>Applies thickness to the top and left sides.</summary>
    public static readonly FuncValueConverter<double, Thickness> TopLeft = new(d => new Thickness(d, d, 0, 0));

    /// <summary>Applies thickness to the top and right sides.</summary>
    public static readonly FuncValueConverter<double, Thickness> TopRight = new(d => new Thickness(0, d, d, 0));

    /// <summary>Applies thickness to the bottom and left sides.</summary>
    public static readonly FuncValueConverter<double, Thickness> BottomLeft = new(d => new Thickness(d, 0, 0, d));

    /// <summary>Applies thickness to the bottom and right sides.</summary>
    public static readonly FuncValueConverter<double, Thickness> BottomRight = new(d => new Thickness(0, 0, d, d));

    /// <summary>Applies thickness to both left and right sides.</summary>
    public static readonly FuncValueConverter<double, Thickness> LeftRight = new(d => new Thickness(d, 0, d, 0));

    /// <summary>Applies thickness to the top, left, and right sides.</summary>
    public static readonly FuncValueConverter<double, Thickness> TopLeftRight = new(d => new Thickness(d, d, d, 0));

    /// <summary>Applies thickness to the top, bottom, and right sides.</summary>
    public static readonly FuncValueConverter<double, Thickness> TopBottomRight = new(d => new Thickness(d, 0, d, d));

    /// <summary>Applies thickness uniformly to all sides.</summary>
    public static readonly FuncValueConverter<double, Thickness> All = new(d => new Thickness(d, d, d, d));
}
