namespace PleasantUI.Controls;

/// <summary>
/// Defines how items are laid out in a <see cref="Timeline"/>.
/// </summary>
public enum TimelineDisplayMode
{
    /// <summary>Content and time are both placed to the right of the axis line.</summary>
    Left,

    /// <summary>Content and time are both placed to the left of the axis line.</summary>
    Right,

    /// <summary>Time is on the left of the axis, content on the right.</summary>
    Center,

    /// <summary>Items alternate left/right across the axis.</summary>
    Alternate,
}

/// <summary>
/// Placement of a single <see cref="TimelineItem"/> relative to the axis line.
/// </summary>
public enum TimelineItemPosition
{
    /// <summary>Content and time are placed to the right of the axis.</summary>
    Left,

    /// <summary>Content and time are placed to the left of the axis.</summary>
    Right,

    /// <summary>Time is on the left, content on the right of the axis.</summary>
    Separate,
}

/// <summary>
/// Visual state / severity of a <see cref="TimelineItem"/>.
/// </summary>
public enum TimelineItemType
{
    /// <inheritdoc cref="TimelineItemType"/>
    Default,
    /// <inheritdoc cref="TimelineItemType"/>
    Ongoing,
    /// <inheritdoc cref="TimelineItemType"/>
    Success,
    /// <inheritdoc cref="TimelineItemType"/>
    Warning,
    /// <inheritdoc cref="TimelineItemType"/>
    Error,
}
