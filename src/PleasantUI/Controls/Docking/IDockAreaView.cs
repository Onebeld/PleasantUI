using Avalonia.Controls;

namespace PleasantUI.Controls.Docking;

/// <summary>
/// Defines the interface for a view that can host dock areas.
/// </summary>
public interface IDockAreaView
{
    /// <summary>
    /// Gets the dock areas and their associated controls.
    /// </summary>
    /// <returns>An array of tuples containing dock areas and controls.</returns>
    (DockArea, Control)[] GetArea();

    /// <summary>
    /// Called when a dock area is attached to this view.
    /// </summary>
    /// <param name="dockArea">The dock area being attached.</param>
    void OnAttachedToDockArea(DockArea dockArea)
    {
    }

    /// <summary>
    /// Called when a dock area is detached from this view.
    /// </summary>
    /// <param name="dockArea">The dock area being detached.</param>
    void OnDetachedFromDockArea(DockArea dockArea)
    {
    }
}
