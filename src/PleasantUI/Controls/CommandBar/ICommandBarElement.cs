namespace PleasantUI.Controls;

/// <summary>
/// Defines the contract for elements that can be placed inside a <see cref="CommandBar"/>.
/// </summary>
public interface ICommandBarElement
{
    /// <summary>
    /// Gets or sets the order in which this element moves to the overflow menu
    /// when space is limited. Elements with the same non-zero order move together.
    /// Zero means no ordered grouping — the element overflows individually from the end.
    /// </summary>
    int DynamicOverflowOrder { get; set; }

    /// <summary>
    /// Gets or sets whether the element is shown with no label and reduced padding.
    /// </summary>
    bool IsCompact { get; set; }

    /// <summary>
    /// Gets whether this element is currently located in the overflow menu.
    /// </summary>
    bool IsInOverflow { get; }
}
