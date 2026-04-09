# CommandBar

A toolbar control that displays primary commands inline and moves overflow items into a popup menu. Supports dynamic overflow, label positions, and open/closed states.

## Basic usage

```xml
<CommandBar>
    <CommandBarButton Content="New" />
    <CommandBarButton Content="Open" />
    <CommandBarButton Content="Save" />
    <CommandBar.SecondaryCommands>
        <CommandBarButton Content="Settings" />
        <CommandBarButton Content="About" />
    </CommandBar.SecondaryCommands>
</CommandBar>
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `PrimaryCommands` | `IAvaloniaList<ICommandBarElement>` | — | Collection of primary command elements |
| `SecondaryCommands` | `IAvaloniaList<ICommandBarElement>` | — | Collection of secondary command elements shown in overflow popup |
| `IsOpen` | `bool` | `false` | Whether the overflow popup is open |
| `IsSticky` | `bool` | `true` | Whether the popup stays open on light-dismiss |
| `ClosedDisplayMode` | `CommandBarClosedDisplayMode` | `Compact` | How the bar appears when closed (`Compact`, `Minimal`, `Hidden`) |
| `DefaultLabelPosition` | `CommandBarDefaultLabelPosition` | — | Label position for primary command buttons (`Bottom`, `Right`, `Collapsed`) |
| `ItemsAlignment` | `CommandBarItemsAlignment` | — | How primary commands are aligned (`Left`, `Right`) |
| `OverflowButtonVisibility` | `CommandBarOverflowButtonVisibility` | `Auto` | When the overflow button is shown (`Auto`, `Visible`, `Hidden`) |
| `IsDynamicOverflowEnabled` | `bool` | `true` | Whether primary commands automatically move to overflow when space is limited |

## Events

| Event | Description |
|---|---|
| `Opening` | Raised when the overflow popup starts opening |
| `Opened` | Raised when the overflow popup has opened |
| `Closing` | Raised when the overflow popup starts closing |
| `Closed` | Raised when the overflow popup has closed |

## Primary vs secondary commands

Primary commands are shown inline in the toolbar. Secondary commands are always shown in the overflow popup:

```xml
<CommandBar>
    <CommandBarButton Content="Save" Icon="{DynamicResource SaveIcon}" />
    <CommandBarButton Content="Print" Icon="{DynamicResource PrintIcon}" />
    
    <CommandBar.SecondaryCommands>
        <CommandBarButton Content="Options" />
        <CommandBarButton Content="Help" />
    </CommandBar.SecondaryCommands>
</CommandBar>
```

## Dynamic overflow

When `IsDynamicOverflowEnabled` is `true`, primary commands automatically move to the overflow menu when space is limited. Use `DynamicOverflowOrder` to control which items overflow first:

```xml
<CommandBar IsDynamicOverflowEnabled="True">
    <CommandBarButton Content="Primary" DynamicOverflowOrder="0" />
    <CommandBarButton Content="Secondary" DynamicOverflowOrder="1" />
    <CommandBarButton Content="Tertiary" DynamicOverflowOrder="2" />
</CommandBar>
```

Items with higher `DynamicOverflowOrder` values overflow first.

## Label positions

Control where button labels appear:

```xml
<CommandBar DefaultLabelPosition="Right">
    <CommandBarButton Content="Save" />
    <CommandBarButton Content="Open" />
</CommandBar>
```

- `Bottom`: Labels below icons (default)
- `Right`: Labels to the right of icons
- `Collapsed`: Labels hidden (icons only)

## Closed display modes

Control how the bar appears when the overflow popup is closed:

```xml
<CommandBar ClosedDisplayMode="Minimal">
    <!-- Commands -->
</CommandBar>
```

- `Compact`: Shows primary commands in a compact layout
- `Minimal`: Shows only icons (if available) or a single button
- `Hidden`: Hides the bar entirely when closed

## Toggle buttons

Use toggle buttons for stateful commands:

```xml
<CommandBar>
    <CommandBarButton Content="Bold" />
    <CommandBarToggleButton Content="Italic" />
    <CommandBarToggleButton Content="Underline" />
</CommandBar>
```

## Separators

Add visual separators between command groups:

```xml
<CommandBar>
    <CommandBarButton Content="New" />
    <CommandBarButton Content="Open" />
    <CommandBarSeparator />
    <CommandBarButton Content="Save" />
    <CommandBarButton Content="Save As" />
</CommandBar>
```

## Handling open/close events

```csharp
commandBar.Opening += (sender, e) => { /* Prepare overflow menu */ };
commandBar.Opened += (sender, e) => { /* Animation complete */ };
commandBar.Closing += (sender, e) => { /* Cleanup */ };
commandBar.Closed += (sender, e) => { /* Fully closed */ };
```

## Programmatic control

```csharp
// Open/close overflow
commandBar.IsOpen = true;
commandBar.IsOpen = false;
```
