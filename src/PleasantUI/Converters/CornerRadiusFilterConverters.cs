using Avalonia.Controls.Converters;

namespace PleasantUI.Converters;

/// <inheritdoc cref="CornerRadiusFilterConverter" />
public static class CornerRadiusFilterConverters
{
    /// <summary>
    /// A <see cref="CornerRadiusFilterConverter" /> that filters the top corners (TopLeft and TopRight).
    /// </summary>
    public static readonly CornerRadiusFilterConverter Top =
        new() { Filter = Corners.TopLeft | Corners.TopRight };

    /// <summary>
    /// A <see cref="CornerRadiusFilterConverter" /> that filters the right corners (TopRight and BottomRight).
    /// </summary>
    public static readonly CornerRadiusFilterConverter Right =
        new() { Filter = Corners.TopRight | Corners.BottomRight };

    /// <summary>
    /// A <see cref="CornerRadiusFilterConverter" /> that filters the bottom corners (BottomLeft and BottomRight).
    /// </summary>
    public static readonly CornerRadiusFilterConverter Bottom =
        new() { Filter = Corners.BottomLeft | Corners.BottomRight };

    /// <summary>
    /// A <see cref="CornerRadiusFilterConverter" /> that filters the left corners (TopLeft and BottomLeft).
    /// </summary>
    public static readonly CornerRadiusFilterConverter Left =
        new() { Filter = Corners.TopLeft | Corners.BottomLeft };

    /// <summary>
    /// A <see cref="CornerRadiusToDoubleConverter" /> that converts the TopLeft corner radius to a double value.
    /// </summary>
    public static readonly CornerRadiusToDoubleConverter TopLeft =
        new() { Corner = Corners.TopLeft };

    /// <summary>
    /// A <see cref="CornerRadiusToDoubleConverter" /> that converts the TopRight corner radius to a double value.
    /// </summary>
    public static readonly CornerRadiusToDoubleConverter TopRight =
        new() { Corner = Corners.TopRight };

    /// <summary>
    /// A <see cref="CornerRadiusToDoubleConverter" /> that converts the BottomLeft corner radius to a double value.
    /// </summary>
    public static readonly CornerRadiusToDoubleConverter BottomLeft =
        new() { Corner = Corners.BottomLeft };

    /// <summary>
    /// A <see cref="CornerRadiusToDoubleConverter" /> that converts the BottomRight corner radius to a double value.
    /// </summary>
    public static readonly CornerRadiusToDoubleConverter BottomRight =
        new() { Corner = Corners.BottomRight };
}