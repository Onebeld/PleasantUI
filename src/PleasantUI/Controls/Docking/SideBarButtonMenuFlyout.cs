using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Data;

namespace PleasantUI.Controls.Docking;

/// <summary>
/// Represents a menu flyout for sidebar buttons.
/// </summary>
internal class SideBarButtonMenuFlyout : MenuFlyout
{
    private readonly ReDockHost _dockHost;

    /// <summary>
    /// Initializes a new instance of the <see cref="SideBarButtonMenuFlyout"/> class.
    /// </summary>
    /// <param name="dockHost">The dock host.</param>
    public SideBarButtonMenuFlyout(ReDockHost dockHost)
    {
        _dockHost = dockHost;

        var list = new List<Control>();

        // Move to menu
        var moveMenu = new MenuItem
        {
            Header = "Move to",
            ItemsSource = dockHost.DockAreas
        };

        moveMenu.DataTemplates.Add(new FuncDataTemplate<DockArea>(_ => true,
            o => new TextBlock
            {
                [!TextBlock.TextProperty] = new Binding { Source = o, Path = "LocalizedName" }
            }));

        moveMenu.AddHandler(MenuItem.ClickEvent, OnMoveToSubItemClick);
        list.Add(moveMenu);

        // Display mode menu (if floating is enabled)
        if (dockHost.IsFloatingEnabled)
        {
            var displayMenu = new MenuItem
            {
                Header = "Display mode",
                ItemsSource = new List<Control>
                {
                    new MenuItem { Header = "Docked", Tag = DockableDisplayMode.Docked },
                    new MenuItem { Header = "Floating", Tag = DockableDisplayMode.Floating }
                }
            };

            displayMenu.AddHandler(MenuItem.ClickEvent, OnDisplayModeClick);
            list.Add(displayMenu);
        }

        ItemsSource = list;
    }

    private void OnDisplayModeClick(object? sender, RoutedEventArgs e)
    {
        if (e.Source is MenuItem { Tag: DockableDisplayMode mode } &&
            Target is SideBarButton button)
        {
            var args = new SideBarButtonDisplayModeChangedEventArgs(ReDockHost.ButtonDisplayModeChangedEvent, this)
            {
                DisplayMode = mode,
                Item = button.DataContext,
                Button = button
            };

            _dockHost.RaiseEvent(args);
        }
    }

    private void OnMoveToSubItemClick(object? sender, RoutedEventArgs e)
    {
        if (e.Source is MenuItem { DataContext: DockArea area } &&
            Target is SideBarButton button)
        {
            var oldSideBar = button.FindAncestorOfType<SideBar>();
            var newSideBar = area.SideBar;

            if (oldSideBar is null || newSideBar is null) return;

            var oldLocation = button.DockLocation;
            var newLocation = area.Location;

            if (oldLocation is null || oldLocation == newLocation) return;

            var args = new SideBarButtonMoveEventArgs(ReDockHost.ButtonMoveEvent, this)
            {
                Item = button.DataContext,
                Button = button,
                SourceSideBar = oldSideBar,
                SourceLocation = oldLocation,
                DestinationSideBar = newSideBar,
                DestinationLocation = newLocation,
                DestinationIndex = 0
            };

            _dockHost.RaiseEvent(args);
        }
    }
}
