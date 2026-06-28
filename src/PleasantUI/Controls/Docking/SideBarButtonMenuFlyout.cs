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

        List<Control> list = new();

        // Move to menu
        MenuItem moveMenu = new()
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
            MenuItem displayMenu = new()
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
            SideBarButtonDisplayModeChangedEventArgs args = new(ReDockHost.ButtonDisplayModeChangedEvent, this)
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
            SideBar? oldSideBar = button.FindAncestorOfType<SideBar>();
            SideBar? newSideBar = area.SideBar;

            if (oldSideBar is null || newSideBar is null) return;

            DockAreaLocation? oldLocation = button.DockLocation;
            DockAreaLocation newLocation = area.Location;

            if (oldLocation is null || oldLocation == newLocation) return;

            SideBarButtonMoveEventArgs args = new(ReDockHost.ButtonMoveEvent, this)
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
