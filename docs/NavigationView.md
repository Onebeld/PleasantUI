# NavigationView / NavigationViewItem

A navigation control that displays a tree of items. Supports three positions (Left, Top, Bottom), compact/overlay/inline display modes, nested items, back button, animated content transitions, and automatic responsive collapse based on window width.

## Basic usage

```xml
<NavigationView Position="Left">
    <NavigationViewItem Header="Home"
                        Icon="{DynamicResource HomeRegular}"
                        Title="Home"
                        Content="{x:Type views:HomeView}" />

    <NavigationViewItem Header="Settings"
                        Icon="{DynamicResource TuneRegular}"
                        Title="Settings"
                        Content="{x:Type views:SettingsView}" />
</NavigationView>
```

The `Content` of the selected item is displayed in the main content area. `Title` is shown in the content panel header.

## NavigationView properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Position` | `NavigationViewPosition` | `Left` | Position of the navigation pane: `Left`, `Top`, or `Bottom` |
| `TopItems` | `AvaloniaList<NavigationViewItem>` | — | Items displayed in the top navigation bar (use when `Position=Top`) |
| `BottomItems` | `AvaloniaList<NavigationViewItem>` | — | Items displayed in the bottom navigation bar (use when `Position=Bottom`) |
| `IsOpen` | `bool` | `false` | Whether the pane is expanded |
| `AlwaysOpen` | `bool` | `false` | Keeps the pane always expanded |
| `DisplayMode` | `SplitViewDisplayMode` | `CompactInline` | `CompactInline`, `CompactOverlay`, `Inline`, `Overlay` |
| `DynamicDisplayMode` | `bool` | `true` | Auto-switches to compact/overlay at narrow widths (Left position only) |
| `CompactPaneLength` | `double` | `50` | Width of the pane in compact mode |
| `OpenPaneLength` | `double` | `250` | Width of the pane when open |
| `IsFloatingHeader` | `bool` | `false` | Floats the header above the pane |
| `ShowBackButton` | `bool` | `false` | Shows a back button at the top |
| `BackButtonCommand` | `ICommand?` | `null` | Command invoked by the back button |
| `ButtonsPanelOffset` | `bool` | `false` | Pushes hamburger/back buttons down by titlebar height to sit flush below titlebar |
| `DisplayTopIndent` | `bool` | `true` | Whether the top indent is displayed |
| `NotMakeOffsetForContentPanel` | `bool` | `false` | Whether the content panel should not have an offset |
| `BindWindowSettings` | `bool` | `true` | Whether window settings should be bound |
| `TransitionAnimation` | `Animation?` | slide-up | Animation played when selected content changes |
| `SelectedContent` | `object?` | — | Currently displayed content (read-only) |
| `SelectedContentTemplate` | `IDataTemplate` | — | Data template for selected content |
| `ItemsAsStrings` | `IEnumerable<string>?` | — | Items collection as strings (read-only) |

## NavigationViewItem properties

| Property | Type | Description |
|---|---|---|
| `Header` | `string` | Label shown in the pane |
| `Icon` | `Geometry?` | Icon shown in compact mode and next to the label |
| `Title` | `object` | Heading shown in the content area when this item is selected |
| `Content` | `object?` | Content displayed in the main area when selected |
| `IsOpen` | `bool` | Whether child items are expanded |

## Position

The `Position` property controls where the navigation pane is displayed:

- `Left` (default) - Side pane with SplitView layout
- `Top` - Horizontal bar above the content area
- `Bottom` - Horizontal bar below the content area

```xml
<!-- Top navigation bar -->
<NavigationView Position="Top">
    <!-- Items are populated from the Items collection -->
</NavigationView>

<!-- Bottom navigation bar -->
<NavigationView Position="Bottom">
    <!-- Items are populated from the Items collection -->
</NavigationView>
```

## Top/Bottom navigation items

When using `Position="Top"` or `Position="Bottom"`, items are automatically populated from the main `Items` collection. The control creates proxy items internally to avoid visual tree conflicts.

For the Left position, use the standard `Items` collection. For Top/Bottom positions, you can also use the dedicated `TopItems` and `BottomItems` collections directly:

```xml
<NavigationView Position="Top">
    <NavigationView.TopItems>
        <NavigationViewItem Header="Home" Icon="{DynamicResource HomeRegular}" Content="{x:Type views:HomeView}" />
        <NavigationViewItem Header="Search" Icon="{DynamicResource SearchRegular}" Content="{x:Type views:SearchView}" />
    </NavigationView.TopItems>
</NavigationView>

<NavigationView Position="Bottom">
    <NavigationView.BottomItems>
        <NavigationViewItem Header="Home" Icon="{DynamicResource HomeRegular}" Content="{x:Type views:HomeView}" />
        <NavigationViewItem Header="Profile" Icon="{DynamicResource PersonRegular}" Content="{x:Type views:ProfileView}" />
    </NavigationView.BottomItems>
</NavigationView>
```

## Nested items

Nested items are supported in the Left position:

```xml
<NavigationView Position="Left">
    <NavigationViewItem Header="Library" Icon="{DynamicResource FolderRegular}">
        <NavigationViewItem Header="Music"   Content="{x:Type views:MusicView}" />
        <NavigationViewItem Header="Videos"  Content="{x:Type views:VideosView}" />
        <NavigationViewItem Header="Photos"  Content="{x:Type views:PhotosView}" />
    </NavigationViewItem>
</NavigationView>
```

Note: Nested items are not supported in Top/Bottom positions.

## Buttons panel offset

The `ButtonsPanelOffset` property controls whether the hamburger/back buttons panel is pushed down by the titlebar height:

```xml
<!-- Buttons sit flush below the titlebar -->
<NavigationView ButtonsPanelOffset="True" />

<!-- Buttons overlap the titlebar (original behavior) -->
<NavigationView ButtonsPanelOffset="False" />
```

This property is automatically synced with the window's `TitleBarType` - when set to `Compact`, `ButtonsPanelOffset` is set to `true`.

## Display top indent

Control whether the top indent is displayed:

```xml
<NavigationView DisplayTopIndent="False" />
```

## Content panel offset

Control whether the content panel has an offset:

```xml
<NavigationView NotMakeOffsetForContentPanel="True" />
```

## Responding to selection in code

```csharp
navigationView.SelectionChanged += (_, e) =>
{
    if (e.SelectedItem is NavigationViewItem item)
        Console.WriteLine(item.Header);
};
```
