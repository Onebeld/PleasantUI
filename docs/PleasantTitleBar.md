# PleasantTitleBar

Represents the title bar of a `PleasantWindow`. Provides standard title bar functionality including window dragging, caption buttons, and title/subtitle display with platform-specific layouts for Windows and macOS.

## Usage

The title bar is typically used within a `PleasantWindow` template:

```xml
<chrome:PleasantTitleBar />
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `IsTitleVisible` | `bool` | `true` | Whether the title panel (icon + title + subtitle) is visible |
| `LeftClearance` | `double` | `40.0` | Width of the reserved left column in the titlebar grid |

## Attached properties

| Property | Type | Default | Description |
|---|---|---|---|
| `IsTitleBarHitTestVisible` (attached) | `bool` | `true` | Controls whether the drag area responds to hit-testing |

```xml
<chrome:PleasantTitleBar IsTitleBarHitTestVisible="False" />
```

## Title bar types

The `Type` enum (defined on `PleasantWindow`) controls the title bar layout:

- `Classic` - Regular title bar
- `ClassicExtended` - Slightly larger title bar with more room for custom content
- `Compact` - Compact title bar that takes minimal vertical space

Set on the host window:

```csharp
window.TitleBarType = PleasantTitleBar.Type.ClassicExtended;
```

## Platform behavior

### Windows
- Left clearance column (default 40px) for window menu/hamburger button
- Standard caption buttons on the right
- Drag area for window movement

### macOS
- Native traffic light buttons (close, minimize, maximize) in the left column
- Custom caption buttons can be used with `OverrideMacOSCaption`
- Traffic light area width adjusts based on fullscreen state

## Window state pseudo-classes

The control applies pseudo-classes based on window state:
- `:active` - Applied when window is active
- `:minimized` - Applied when window is minimized
- `:normal` - Applied when window is in normal state
- `:maximized` - Applied when window is maximized
- `:isactive` - Applied when window is inactive
- `:titlebar` - Applied for Classic title bar type

## Double-click to maximize

Double-clicking on the drag area toggles maximize/restore (if the window can resize):

```csharp
// Handled automatically by the control
```

## Custom content

Set custom content in the title bar via the host window:

```csharp
window.TitleContent = new TextBox { PlaceholderText = "Search..." };
window.LeftTitleBarContent = new Button { Content = "Menu" };
```

## Left clearance

Adjust the left clearance width (Windows only):

```xml
<chrome:PleasantTitleBar LeftClearance="0" />
```

Set to 0 to push the logo/title to the far left when no hamburger menu overlaps the title bar.

## Template parts

The control template must provide:
- `PART_PleasantCaptionButtons` - Caption buttons control
- `PART_CloseMenuItem` - Close menu item
- `PART_ExpandMenuItem` - Maximize menu item
- `PART_ReestablishMenuItem` - Restore menu item
- `PART_CollapseMenuItem` - Minimize menu item
- `PART_DragWindow` - Border for window dragging
- `PART_Icon` - Icon display panel
- `PART_Title` - Title display panel
- `PART_Subtitle` - Subtitle text block
- `PART_LogoPath` - Logo path (optional)
- `PART_LeftTitleBarContent` - Left content presenter
- `PART_TitleBarContent` - Center content presenter
- `PART_TitlePanel` - Title stack panel
- `PART_TitleBarGrid` - Main layout grid

## Notes

- The control automatically subscribes to window property changes and updates the UI
- Drag functionality is built-in via the `PART_DragWindow` border
- Platform-specific layouts are applied automatically
