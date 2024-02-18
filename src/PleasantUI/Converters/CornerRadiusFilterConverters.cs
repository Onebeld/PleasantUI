using Avalonia.Controls.Converters;

namespace PleasantUI.Converters;

/// <inheritdoc cref="CornerRadiusFilterConverter" />
public static class CornerRadiusFilterConverters
{
    public static readonly CornerRadiusFilterConverter Top =
        new() { Filter = Corners.TopLeft | Corners.TopRight };
    
    public static readonly CornerRadiusFilterConverter Right =
        new() { Filter = Corners.TopRight | Corners.BottomRight };

    public static readonly CornerRadiusFilterConverter Bottom =
        new() { Filter = Corners.BottomLeft | Corners.BottomRight };

    public static readonly CornerRadiusFilterConverter Left = 
        new() { Filter = Corners.TopLeft | Corners.BottomLeft };
    
    public static readonly CornerRadiusToDoubleConverter TopLeft =
        new() { Corner = Corners.TopLeft };
    
    public static readonly CornerRadiusToDoubleConverter TopRight =
        new() { Corner = Corners.TopRight };

    public static readonly CornerRadiusToDoubleConverter BottomLeft =
        new() { Corner = Corners.BottomLeft };
    
    public static readonly CornerRadiusToDoubleConverter BottomRight =
        new() { Corner = Corners.BottomRight };
}