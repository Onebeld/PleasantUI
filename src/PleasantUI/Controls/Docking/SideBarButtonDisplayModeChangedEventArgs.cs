using Avalonia.Interactivity;

namespace PleasantUI.Controls.Docking;

/// <summary>
/// Provides data for the <see cref="ReDockHost.ButtonDisplayModeChangedEvent"/> event.
/// </summary>
public class SideBarButtonDisplayModeChangedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SideBarButtonDisplayModeChangedEventArgs"/> class.
    /// </summary>
    /// <param name="routedEvent">The routed event associated with this event args.</param>
    /// <param name="source">The source object.</param>
    public SideBarButtonDisplayModeChangedEventArgs(RoutedEvent? routedEvent, object? source) : base(routedEvent, source)
    {
    }

    /// <summary>
    /// Gets the new display mode.
    /// </summary>
    public required DockableDisplayMode DisplayMode { get; init; }

    /// <summary>
    /// Gets the item whose display mode changed.
    /// </summary>
    public required object? Item { get; init; }

    /// <summary>
    /// Gets the button whose display mode changed.
    /// </summary>
    public required SideBarButton Button { get; init; }
}
