# NavigationView / NavigationViewItem

A collapsible side navigation panel backed by a `TreeView`. Supports compact/overlay/inline display modes, nested items, back button, animated content transitions, and automatic responsive collapse based on window width.

## Basic usage

```xml
<NavigationView>
    <NavigationViewItem Header="Home"
                        Icon="{StaticResource HomeRegular}"
                        Title="Home"
                        Content="{x:Type views:HomeView}" />

    <NavigationViewItem Header="Settings"
                        Icon="{StaticResource TuneRegular}"
                        Title="Settings"
                        Content="{x:Type views:SettingsView}" />
</NavigationView>
```

The `Content` of the selected item is displayed in the main content area. `Title` is shown in the content panel header.

## NavigationView properties

| Property | Type | Default | Description |
|---|---|---|---|
| `IsOpen` | `bool` | `false` | Whether the pane is expanded |
| `AlwaysOpen` | `bool` | `false` | Keeps the pane always expanded |
| `DisplayMode` | `SplitViewDisplayMode` | `CompactInline` | `CompactInline`, `CompactOverlay`, `Inline`, `Overlay` |
| `DynamicDisplayMode` | `bool` | `true` | Auto-switches to compact/overlay at narrow widths |
| `CompactPaneLength` | `double` | `50` | Width of the pane in compact mode |
| `OpenPaneLength` | `double` | `250` | Width of the pane when open |
| `IsFloatingHeader` | `bool` | `false` | Floats the header above the pane |
| `ShowBackButton` | `bool` | `false` | Shows a back button at the top |
| `BackButtonCommand` | `ICommand?` | `null` | Command invoked by the back button |
| `TransitionAnimation` | `Animation?` | slide-up | Animation played when selected content changes |
| `SelectedContent` | `object?` | — | Currently displayed content (read-only) |

## NavigationViewItem properties

| Property | Type | Description |
|---|---|---|
| `Header` | `string` | Label shown in the pane |
| `Icon` | `Geometry?` | Icon shown in compact mode and next to the label |
| `Title` | `object` | Heading shown in the content area when this item is selected |
| `Content` | `object?` | Content displayed in the main area when selected |
| `IsOpen` | `bool` | Whether child items are expanded |

## Nested items

```xml
<NavigationViewItem Header="Library" Icon="{StaticResource FolderRegular}">
    <NavigationViewItem Header="Music"   Content="{x:Type views:MusicView}" />
    <NavigationViewItem Header="Videos"  Content="{x:Type views:VideosView}" />
    <NavigationViewItem Header="Photos"  Content="{x:Type views:PhotosView}" />
</NavigationViewItem>
```

## Pinning items to the bottom

Use `DockPanel.Dock="Bottom"` on items you want at the bottom of the pane:

```xml
<NavigationView>
    <NavigationViewItem Header="Home" DockPanel.Dock="Top" ... />
    <NavigationViewItem Header="Settings" DockPanel.Dock="Bottom" ... />
    <NavigationViewItem Header="About"    DockPanel.Dock="Bottom" ... />
</NavigationView>
```

## Responding to selection in code

```csharp
navigationView.SelectionChanged += (_, e) =>
{
    if (e.SelectedItem is NavigationViewItem item)
        Console.WriteLine(item.Header);
};
```
