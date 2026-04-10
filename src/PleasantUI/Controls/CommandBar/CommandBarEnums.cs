namespace PleasantUI.Controls;

/// <summary>
/// Defines where primary commands are aligned inside a <see cref="CommandBar"/>.
/// </summary>
public enum CommandBarItemsAlignment
{
    /// <summary>Primary commands are left-aligned.</summary>
    Left,
    /// <summary>Primary commands are right-aligned.</summary>
    Right
}

/// <summary>
/// Defines whether icon buttons are shown when the <see cref="CommandBar"/> is closed.
/// </summary>
public enum CommandBarClosedDisplayMode
{
    /// <summary>Icon buttons are shown; labels are hidden.</summary>
    Compact,
    /// <summary>Only the overflow button is shown.</summary>
    Minimal,
    /// <summary>The command bar is not visible when closed.</summary>
    Hidden
}

/// <summary>
/// Defines the placement and visibility of labels on <see cref="CommandBarButton"/> items.
/// </summary>
public enum CommandBarDefaultLabelPosition
{
    /// <summary>Labels appear below the icon and are only visible when the bar is open.</summary>
    Bottom,
    /// <summary>Labels appear to the right of the icon and are always visible.</summary>
    Right,
    /// <summary>Labels are always hidden.</summary>
    Collapsed
}

/// <summary>
/// Defines when the overflow (more) button is shown on a <see cref="CommandBar"/>.
/// </summary>
public enum CommandBarOverflowButtonVisibility
{
    /// <summary>The button is shown automatically when there are overflow items.</summary>
    Auto,
    /// <summary>The button is always shown.</summary>
    Visible,
    /// <summary>The button is never shown.</summary>
    Collapsed
}
