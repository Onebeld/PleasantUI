# PleasantDrawer

A panel that slides in from an edge of the screen, overlaying the main content. Supports light-dismiss (click outside to close), optional title, close button, and all four positions (Left, Right, Top, Bottom).

## Basic usage

```xml
<PleasantDrawer Title="Settings"
                Position="Right"
                PanelWidth="400">
    <StackPanel Spacing="10">
        <TextBlock Text="Drawer content" />
        <CheckBox Content="Enable feature" />
        <Button Content="Save" />
    </StackPanel>
</PleasantDrawer>
```

```csharp
await drawer.ShowAsync(window);
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Title` | `string?` | — | Title shown in the drawer header |
| `Position` | `DrawerPosition` | `Right` | Which edge the drawer slides in from (`Left`, `Right`, `Top`, `Bottom`) |
| `CanLightDismiss` | `bool` | `true` | Whether clicking the overlay background closes the drawer |
| `IsModal` | `bool` | `false` | Whether the drawer blocks interaction with the rest of the UI |
| `PanelWidth` | `double` | — | Width of the drawer panel (for Left/Right positions) |
| `PanelHeight` | `double` | — | Height of the drawer panel (for Top/Bottom positions) |
| `FooterContent` | `object?` | — | Optional footer content (e.g. action buttons) shown at the bottom |
| `OpenAnimation` | `Animation?` | — | Slide-in animation applied to the drawer panel |
| `CloseAnimation` | `Animation?` | — | Slide-out animation applied to the drawer panel |
| `ShowOverlayAnimation` | `Animation?` | — | Fade-in animation for the overlay background |
| `HideOverlayAnimation` | `Animation?` | — | Fade-out animation for the overlay background |

## Events

| Event | Description |
|---|---|
| `Opened` | Raised when the drawer has fully opened |
| `Closed` | Raised when the drawer has fully closed |

## Methods

| Method | Description |
|---|---|
| `ShowAsync(TopLevel)` | Shows the drawer on the specified TopLevel |
| `ShowAsync<T>(TopLevel)` | Shows the drawer and returns a typed result when it closes |
| `CloseAsync(object? result)` | Closes the drawer, optionally passing a result value |

## Positions

Control which edge the drawer slides from:

```xml
<!-- Right side (default) -->
<PleasantDrawer Position="Right" PanelWidth="400" />

<!-- Left side -->
<PleasantDrawer Position="Left" PanelWidth="400" />

<!-- Top edge -->
<PleasantDrawer Position="Top" PanelHeight="300" />

<!-- Bottom edge -->
<PleasantDrawer Position="Bottom" PanelHeight="300" />
```

## Light dismiss

By default, clicking the overlay (outside the drawer) closes it. Disable this behavior:

```xml
<PleasantDrawer CanLightDismiss="False" />
```

## Modal drawers

Make the drawer modal to block interaction with the rest of the UI:

```xml
<PleasantDrawer IsModal="True" />
```

## Footer content

Add action buttons or other content at the bottom of the drawer:

```xml
<PleasantDrawer Title="Confirm">
    <TextBlock Text="Are you sure?" />
    
    <PleasantDrawer.FooterContent>
        <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Spacing="8">
            <Button Content="Cancel" />
            <Button Content="Confirm" Theme="{DynamicResource AccentButtonTheme}" />
        </StackPanel>
    </PleasantDrawer.FooterContent>
</PleasantDrawer>
```

## Returning a result

Show the drawer and wait for a result:

```csharp
var result = await drawer.ShowAsync<string>(window);
if (result == "confirmed")
{
    // User confirmed
}
```

Close with a result:

```csharp
await drawer.CloseAsync("confirmed");
```

## Custom animations

Provide custom open/close animations:

```xml
<PleasantDrawer>
    <PleasantDrawer.OpenAnimation>
        <Animation Duration="0:0:0.3">
            <KeyFrame Cue="0.0">
                <Setter Property="TranslateTransform.X" Value="400" />
            </KeyFrame>
            <KeyFrame Cue="1.0">
                <Setter Property="TranslateTransform.X" Value="0" />
            </KeyFrame>
        </Animation>
    </PleasantDrawer.OpenAnimation>
</PleasantDrawer>
```

## Events

Handle open/close events:

```csharp
drawer.Opened += (sender, e) => { /* Drawer opened */ };
drawer.Closed += (sender, e) => { /* Drawer closed */ };
```
