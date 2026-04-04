using Avalonia.Interactivity;

namespace PleasantUI.Controls.Docking;

/// <summary>
/// Provides data for the <see cref="ReDockHost.ButtonMoveEvent"/> event.
/// </summary>
public class SideBarButtonMoveEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SideBarButtonMoveEventArgs"/> class.
    /// </summary>
    /// <param name="routedEvent">The routed event associated with this event args.</param>
    /// <param name="source">The source object.</param>
    public SideBarButtonMoveEventArgs(RoutedEvent? routedEvent, object? source) : base(routedEvent, source)
    {
    }

    /// <summary>
    /// Gets the item being moved.
    /// </summary>
    public required object? Item { get; init; }

    /// <summary>
    /// Gets the button being moved.
    /// </summary>
    public required SideBarButton Button { get; init; }

    /// <summary>
    /// Gets the source sidebar.
    /// </summary>
    public required SideBar SourceSideBar { get; init; }

    /// <summary>
    /// Gets the source dock area location.
    /// </summary>
    public required DockAreaLocation SourceLocation { get; init; }

    /// <summary>
    /// Gets the destination sidebar.
    /// </summary>
    public required SideBar DestinationSideBar { get; init; }

    /// <summary>
    /// Gets the destination dock area location.
    /// </summary>
    public required DockAreaLocation DestinationLocation { get; init; }

    /// <summary>
    /// Gets the destination index.
    /// </summary>
    public required int DestinationIndex { get; init; }
}
