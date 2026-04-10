# SmoothScrollViewer

A `ScrollViewer` replacement with inertia-based smooth scrolling. Supports configurable easing and duration, horizontal/vertical scroll bar visibility, scroll chaining, and programmatic scroll methods.

## Basic usage

```xml
<SmoothScrollViewer>
    <StackPanel>
        <!-- long content -->
    </StackPanel>
</SmoothScrollViewer>
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `HorizontalScrollBarVisibility` | `ScrollBarVisibility` | `Disabled` | `Disabled`, `Auto`, `Hidden`, `Visible` |
| `VerticalScrollBarVisibility` | `ScrollBarVisibility` | `Auto` | `Disabled`, `Auto`, `Hidden`, `Visible` |
| `SmoothScrollEasing` | `Easing?` | — | Easing function for the smooth scroll animation |
| `SmoothScrollDuration` | `TimeSpan` | — | Duration of the smooth scroll animation |
| `AllowAutoHide` | `bool` | inherited | Whether scroll bars auto-hide when not in use |
| `IsScrollChainingEnabled` | `bool` | `true` | Whether scrolling propagates to a parent scrollable when the limit is reached |
| `Offset` | `Vector` | — | Current scroll offset (get/set) |
| `Extent` | `Size` | — | Total size of the scrollable content (read-only) |
| `Viewport` | `Size` | — | Size of the visible viewport (read-only) |

## Events

| Event | Description |
|---|---|
| `ScrollChanged` | Fired when scroll position, extent, or viewport size changes |

## Smooth scroll animation

```xml
<SmoothScrollViewer SmoothScrollDuration="0:0:0.3"
                    SmoothScrollEasing="{x:Static Easings.CubicEaseOut}">
    <ItemsControl ItemsSource="{Binding Items}" />
</SmoothScrollViewer>
```

## Horizontal scroll

```xml
<SmoothScrollViewer HorizontalScrollBarVisibility="Auto"
                    VerticalScrollBarVisibility="Disabled">
    <StackPanel Orientation="Horizontal">
        <!-- wide content -->
    </StackPanel>
</SmoothScrollViewer>
```

## Programmatic scrolling

```csharp
// Line scroll
viewer.LineUp();
viewer.LineDown();
viewer.LineLeft();
viewer.LineRight();

// Page scroll
viewer.PageUp();
viewer.PageDown();

// Jump to start / end
viewer.ScrollToHome();
viewer.ScrollToEnd();

// Set offset directly
viewer.Offset = new Vector(0, 500);
```

## Attached properties

`HorizontalScrollBarVisibility` and `VerticalScrollBarVisibility` can be set as attached properties on child controls:

```xml
<SmoothScrollViewer>
    <TextBox SmoothScrollViewer.HorizontalScrollBarVisibility="Auto"
             AcceptsReturn="True" />
</SmoothScrollViewer>
```

## Disable scroll chaining

Prevent scroll events from bubbling to a parent scrollable:

```xml
<SmoothScrollViewer IsScrollChainingEnabled="False">
    <!-- nested scrollable content -->
</SmoothScrollViewer>
```
