using Avalonia.Interactivity;

namespace PleasantUI.Controls.Docking;

/// <summary>
/// Provides data for the flyout requested event.
/// </summary>
public class SideBarButtonFlyoutRequestedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SideBarButtonFlyoutRequestedEventArgs"/> class.
    /// </summary>
    /// <param name="button">The sidebar button.</param>
    /// <param name="dockHost">The dock host.</param>
    /// <param name="routedEvent">The routed event.</param>
    /// <param name="source">The source object.</param>
    public SideBarButtonFlyoutRequestedEventArgs(
        SideBarButton button,
        ReDockHost dockHost,
        RoutedEvent routedEvent,
        object source) : base(routedEvent, source)
    {
        Button = button;
        DockHost = dockHost;
    }

    /// <summary>
    /// Gets the sidebar button.
    /// </summary>
    public SideBarButton Button { get; }

    /// <summary>
    /// Gets the dock host.
    /// </summary>
    public ReDockHost DockHost { get; }
}
