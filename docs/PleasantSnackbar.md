# PleasantSnackbar

A temporary pill-shaped notification that appears at the bottom of the window. Supports severity variants, title, action buttons, close button, and Closing/Closed events.

## Basic usage

```csharp
PleasantSnackbar.Show(window, new PleasantSnackbarOptions("File saved successfully.")
{
    Icon             = MaterialIcons.CheckCircleOutline,
    NotificationType = NotificationType.Success
});
```

## PleasantSnackbarOptions

| Property | Type | Default | Description |
|---|---|---|---|
| `Message` | `string` | required | Body text |
| `Title` | `string?` | `null` | Bold title above the message — pill expands vertically |
| `Icon` | `Geometry?` | `null` | Icon shown in the colored pill on the left |
| `NotificationType` | `Information` / `Success` / `Warning` / `Error` | `Information` | Controls the icon pill color |
| `TimeSpan` | `TimeSpan` | 3 s | How long the snackbar stays visible |
| `ButtonText` | `string?` | `null` | Text for an inline action button |
| `ButtonAction` | `Action?` | `null` | Callback when the action button is clicked |
| `ActionButton` | `Control?` | `null` | Arbitrary control in the action slot (overrides `ButtonText`/`ButtonAction`) |
| `IsClosable` | `bool` | `false` | Shows a × dismiss button |
| `Closing` | `EventHandler<SnackbarClosingEventArgs>?` | `null` | Fired before close — set `Cancel = true` to prevent it |
| `Closed` | `EventHandler<SnackbarClosedEventArgs>?` | `null` | Fired after close with the `SnackbarCloseReason` |

## Severity variants

```csharp
// Information (default)
PleasantSnackbar.Show(window, new PleasantSnackbarOptions("Update available."));

// Success
PleasantSnackbar.Show(window, new PleasantSnackbarOptions("Saved.")
    { NotificationType = NotificationType.Success, Icon = MaterialIcons.CheckCircleOutline });

// Warning
PleasantSnackbar.Show(window, new PleasantSnackbarOptions("Low disk space.")
    { NotificationType = NotificationType.Warning, Icon = MaterialIcons.AlertOutline });

// Error
PleasantSnackbar.Show(window, new PleasantSnackbarOptions("Upload failed.")
    { NotificationType = NotificationType.Error, Icon = MaterialIcons.CloseCircleOutline });
```

## With title (two-line pill)

```csharp
PleasantSnackbar.Show(window, new PleasantSnackbarOptions("Your changes have been saved to the cloud.")
{
    Title            = "Saved",
    Icon             = MaterialIcons.CloudCheckOutline,
    NotificationType = NotificationType.Success
});
```

## With action button

```csharp
PleasantSnackbar.Show(window, new PleasantSnackbarOptions("Item moved to trash.")
{
    ButtonText   = "Undo",
    ButtonAction = () => RestoreItem()
});
```

## With custom action control

```csharp
var btn = new Button { Content = "View", Theme = accentTheme };
btn.Click += (_, _) => OpenDetails();

PleasantSnackbar.Show(window, new PleasantSnackbarOptions("New update available.")
{
    ActionButton = btn
});
```

## Closable with events

```csharp
PleasantSnackbar.Show(window, new PleasantSnackbarOptions("Background sync running.")
{
    IsClosable = true,
    TimeSpan   = TimeSpan.FromSeconds(10),
    Closing    = (_, args) =>
    {
        if (args.Reason == SnackbarCloseReason.CloseButton)
            CancelSync();
    },
    Closed = (_, args) => Console.WriteLine($"Closed: {args.Reason}")
});
```

## Show overloads

```csharp
PleasantSnackbar.Show(options);                    // uses active window
PleasantSnackbar.Show(window, options);            // Window
PleasantSnackbar.Show(pleasantWindow, options);    // IPleasantWindow
PleasantSnackbar.Show(topLevel, options);          // TopLevel
```
