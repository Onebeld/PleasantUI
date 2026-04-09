# PleasantTrayPopup

A borderless, transparent, topmost popup window designed for system-tray panels. Provides an optional structured layout with header, status row, content area, and footer — each independently hideable. Auto-dismisses when the window loses focus.

## Basic usage

```csharp
var popup = new PleasantTrayPopup
{
    AppTitle = "My App",
    StatusText = "Connected",
    StatusColor = Brushes.Green,
    Width = 320
};

popup.ShowNearTray();
```

## Structured layout (default)

```xml
<PleasantTrayPopup AppTitle="VPN Client"
                   StatusText="Connected"
                   Width="300">
    <PleasantTrayPopup.AppIcon>
        <PathIcon Data="{DynamicResource ShieldRegular}" Width="20" Height="20" />
    </PleasantTrayPopup.AppIcon>
    <PleasantTrayPopup.FooterContent>
        <StackPanel Orientation="Horizontal" Spacing="8" Margin="12 8">
            <Button Content="Disconnect" Theme="{DynamicResource DangerButtonTheme}" />
            <Button Content="Settings" />
        </StackPanel>
    </PleasantTrayPopup.FooterContent>
</PleasantTrayPopup>
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `UseStructuredLayout` | `bool` | `true` | When false, uses fully custom `Content` instead of the structured layout |
| `AppIcon` | `object?` | — | Icon in the header (IImage or any control) |
| `AppTitle` | `string?` | — | Application name in the header |
| `StatusColor` | `IBrush?` | — | Color of the status indicator dot |
| `StatusText` | `string?` | — | Status text next to the dot |
| `ShowHeader` | `bool` | `true` | Whether the header section is visible |
| `ShowCloseButton` | `bool` | `true` | Whether the × button in the header is visible |
| `StatusItems` | `IEnumerable<StatusItem>?` | — | Key/value pairs shown in the status info row |
| `ShowStatusRow` | `bool` | `true` | Whether the status info row is visible |
| `FooterContent` | `object?` | — | Content placed in the footer area |
| `ShowFooter` | `bool` | `true` | Whether the footer section is visible |
| `PopupCornerRadius` | `CornerRadius` | `12` | Corner radius of the popup panel |
| `PopupBackground` | `IBrush?` | — | Background brush override |
| `PopupBorderBrush` | `IBrush?` | — | Border brush override |
| `PopupBorderThickness` | `Thickness` | `1` | Border thickness |
| `PopupMargin` | `double` | `10` | Margin from the screen edge when using `ShowNearTray()` |
| `AutoDismissOnDeactivate` | `bool` | `true` | Auto-hide when the window loses focus |

## StatusItem

Key/value pairs shown in the status row:

```csharp
popup.StatusItems = new[]
{
    new StatusItem("Server", "us-east-1"),
    new StatusItem("Latency", "12 ms"),
    new StatusItem("Protocol", "WireGuard"),
};

// Or use the helper:
popup.StatusItems = PleasantTrayPopup.CreateStatusItems(new[]
{
    ("Server",   "us-east-1"),
    ("Latency",  "12 ms"),
});
```

## Events

| Event | Description |
|---|---|
| `Dismissed` | Raised when the popup is hidden or closed |

## Methods

| Method | Description |
|---|---|
| `ShowNearTray()` | Positions the popup near the system tray and shows it |
| `Dismiss()` | Hides the popup and raises `Dismissed` |

## Custom layout

Set `UseStructuredLayout="False"` and provide your own `Content`:

```xml
<PleasantTrayPopup UseStructuredLayout="False" Width="280">
    <Border CornerRadius="12" Padding="16">
        <StackPanel Spacing="12">
            <TextBlock Text="Custom content" FontWeight="SemiBold" />
            <Button Content="Action" />
        </StackPanel>
    </Border>
</PleasantTrayPopup>
```

## Tray icon integration

```csharp
TrayIcon trayIcon = new TrayIcon();
PleasantTrayPopup? popup = null;

trayIcon.Clicked += (_, _) =>
{
    if (popup is { IsVisible: true })
    {
        popup.Dismiss();
        return;
    }

    popup = new MyTrayPopup();
    popup.Dismissed += (_, _) => popup = null;
    popup.ShowNearTray();
};
```
