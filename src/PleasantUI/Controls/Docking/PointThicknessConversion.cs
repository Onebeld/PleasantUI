using Avalonia;

namespace PleasantUI.Controls.Docking;

/// <summary>
/// Provides extension methods for converting points to thickness values.
/// </summary>
public static class PointThicknessConversion
{
    /// <summary>
    /// Converts a point to a thickness.
    /// </summary>
    /// <param name="point">The point to convert.</param>
    /// <returns>A thickness with the point's coordinates.</returns>
    public static Thickness ToThickness(this Point point) => new(point.X, point.Y, 0, 0);
}
