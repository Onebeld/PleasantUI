namespace PleasantUI.Controls.Docking;

/// <summary>
/// Represents a location within the docking system.
/// </summary>
/// <param name="ButtonLocation">The location of the button within the sidebar.</param>
/// <param name="LeftRight">The side of the window where the sidebar is located.</param>
public record DockAreaLocation(SideBarButtonLocation ButtonLocation, SideBarLocation LeftRight)
{
    /// <summary>
    /// Left sidebar, upper top location.
    /// </summary>
    public static readonly DockAreaLocation LeftUpperTop = new(SideBarButtonLocation.UpperTop, SideBarLocation.Left);

    /// <summary>
    /// Left sidebar, upper bottom location.
    /// </summary>
    public static readonly DockAreaLocation LeftUpperBottom = new(SideBarButtonLocation.UpperBottom, SideBarLocation.Left);

    /// <summary>
    /// Left sidebar, lower top location.
    /// </summary>
    public static readonly DockAreaLocation LeftLowerTop = new(SideBarButtonLocation.LowerTop, SideBarLocation.Left);

    /// <summary>
    /// Left sidebar, lower bottom location.
    /// </summary>
    public static readonly DockAreaLocation LeftLowerBottom = new(SideBarButtonLocation.LowerBottom, SideBarLocation.Left);

    /// <summary>
    /// Right sidebar, upper top location.
    /// </summary>
    public static readonly DockAreaLocation RightUpperTop = new(SideBarButtonLocation.UpperTop, SideBarLocation.Right);

    /// <summary>
    /// Right sidebar, upper bottom location.
    /// </summary>
    public static readonly DockAreaLocation RightUpperBottom = new(SideBarButtonLocation.UpperBottom, SideBarLocation.Right);

    /// <summary>
    /// Right sidebar, lower top location.
    /// </summary>
    public static readonly DockAreaLocation RightLowerTop = new(SideBarButtonLocation.LowerTop, SideBarLocation.Right);

    /// <summary>
    /// Right sidebar, lower bottom location.
    /// </summary>
    public static readonly DockAreaLocation RightLowerBottom = new(SideBarButtonLocation.LowerBottom, SideBarLocation.Right);

    /// <summary>
    /// Parses a string representation of a dock area location.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <returns>The parsed <see cref="DockAreaLocation"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the string is not a valid location.</exception>
    public static DockAreaLocation Parse(string s)
    {
        if (string.Equals(s, "LeftUpperTop", StringComparison.OrdinalIgnoreCase))
            return LeftUpperTop;
        if (string.Equals(s, "LeftUpperBottom", StringComparison.OrdinalIgnoreCase))
            return LeftUpperBottom;
        if (string.Equals(s, "LeftLowerTop", StringComparison.OrdinalIgnoreCase))
            return LeftLowerTop;
        if (string.Equals(s, "LeftLowerBottom", StringComparison.OrdinalIgnoreCase))
            return LeftLowerBottom;
        if (string.Equals(s, "RightUpperTop", StringComparison.OrdinalIgnoreCase))
            return RightUpperTop;
        if (string.Equals(s, "RightUpperBottom", StringComparison.OrdinalIgnoreCase))
            return RightUpperBottom;
        if (string.Equals(s, "RightLowerTop", StringComparison.OrdinalIgnoreCase))
            return RightLowerTop;
        if (string.Equals(s, "RightLowerBottom", StringComparison.OrdinalIgnoreCase))
            return RightLowerBottom;

        throw new ArgumentException("Invalid DockAreaLocation string", nameof(s));
    }
}
