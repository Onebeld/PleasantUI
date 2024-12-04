using Avalonia;
using Avalonia.Data.Converters;

namespace PleasantUI.Converters;

public static class ThicknessConverters
{
    public static readonly FuncValueConverter<double, Thickness> Top = new(d => new Thickness(0, d, 0, 0));
    
    public static readonly FuncValueConverter<double, Thickness> Left = new(d => new Thickness(d, 0, 0, 0));
    
    public static readonly FuncValueConverter<double, Thickness> Right = new(d => new Thickness(0, 0, d, 0));
    
    public static readonly FuncValueConverter<double, Thickness> Bottom = new(d => new Thickness(0, 0, 0, d));
    
    public static readonly FuncValueConverter<double, Thickness> TopLeft = new(d => new Thickness(d, d, 0, 0));
    
    public static readonly FuncValueConverter<double, Thickness> TopRight = new(d => new Thickness(0, d, d, 0));
    
    public static readonly FuncValueConverter<double, Thickness> BottomLeft = new(d => new Thickness(d, 0, 0, d));
    
    public static readonly FuncValueConverter<double, Thickness> BottomRight = new(d => new Thickness(0, 0, d, d));
    
    public static readonly FuncValueConverter<double, Thickness> LeftRight = new(d => new Thickness(d, 0, d, 0));
    
    public static readonly FuncValueConverter<double, Thickness> TopLeftRight = new(d => new Thickness(d, d, d, 0));
    
    public static readonly FuncValueConverter<double, Thickness> TopBottomRight = new(d => new Thickness(d, 0, d, d));
    
    public static readonly FuncValueConverter<double, Thickness> All = new(d => new Thickness(d, d, d, d));
}