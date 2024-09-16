namespace PleasantUI.Core.Enums;

/// <summary>
/// Represents the types of pleasant caption buttons.
/// </summary>
public enum PleasantCaptionButtonsType
{
    /// <summary>
    /// All buttons are visible (Close, Collapse, Expand).
    /// </summary>
    All = 0,

    /// <summary>
    /// Close and Collapse buttons are visible.
    /// </summary>
    CloseAndCollapse = 1,

    /// <summary>
    /// Close and Expand buttons are visible.
    /// </summary>
    CloseAndExpand = 2,

    /// <summary>
    /// Only the Close button is visible.
    /// </summary>
    Close = 3,

    /// <summary>
    /// No buttons are visible.
    /// </summary>
    None = 4
}