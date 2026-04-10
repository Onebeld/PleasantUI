# PleasantCaptionButtons

Represents a set of caption buttons (close, minimize, maximize, fullscreen) for a `PleasantWindow`. Automatically handles window state changes and button visibility based on window configuration.

## Usage

The caption buttons are typically used within a `PleasantTitleBar` template and are automatically wired to the host window:

```xml
<chrome:PleasantCaptionButtons Host="{Binding RelativeSource={RelativeSource AncestorType=chrome:PleasantWindow}}" />
```

## Properties

| Property | Type | Description |
|---|---|---|
| `Host` | `PleasantWindow?` | The host window for these caption buttons |

## Button types

The `Type` enum (defined on `PleasantWindow`) controls which buttons are visible:

- `All` - All buttons are visible (Close, Minimize, Maximize)
- `CloseAndCollapse` - Close and Minimize buttons are visible
- `CloseAndExpand` - Close and Maximize buttons are visible
- `Close` - Only the Close button is visible
- `None` - No buttons are visible

Set on the host window:

```csharp
window.CaptionButtons = PleasantCaptionButtons.Type.CloseAndCollapse;
```

## Platform behavior

- **Windows**: Shows standard close, minimize, maximize buttons
- **macOS**: The maximize button toggles fullscreen mode instead of maximize/restore
- **Fullscreen mode**: In fullscreen, only Close and FullScreen buttons are shown

## Window state pseudo-classes

The control applies pseudo-classes based on window state:
- `:minimized` - Applied when window is minimized
- `:normal` - Applied when window is in normal state
- `:maximized` - Applied when window is maximized
- `:fullscreen` - Applied when window is in fullscreen mode
- `:isactive` - Applied when window is inactive

Use these for styling:

```xml
<Style Selector="chrome:PleasantCaptionButtons:minimized">
    <Setter Property="Opacity" Value="0.5" />
</Style>
```

## Detaching

When the title bar is removed or the window is destroyed, call `Detach()` to clean up subscriptions:

```csharp
captionButtons.Detach();
```

## Template parts

The control template must provide:
- `PART_CloseButton` - Close button
- `PART_MaximizeButton` - Maximize/Restore button
- `PART_MinimizeButton` - Minimize button
- `PART_FullScreenButton` - Fullscreen toggle button (optional)

## Notes

- Button visibility is automatically updated based on window properties like `CanResize`, `CanMinimize`, and individual button visibility properties
- The control automatically subscribes to window state changes and updates pseudo-classes
