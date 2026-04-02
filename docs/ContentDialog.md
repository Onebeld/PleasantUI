# ContentDialog

A modal overlay dialog that blocks interaction with the rest of the window. Renders inside the window's overlay layer with a dimmed background, open/close animations, and an optional bottom button panel.

## Basic usage in AXAML

```xml
<ContentDialog x:Name="MyDialog"
               MinWidth="400"
               MinHeight="200">
    <SmoothScrollViewer>
        <StackPanel Margin="20" Spacing="10">
            <TextBlock Text="Are you sure?" FontWeight="SemiBold" FontSize="18" />
            <TextBlock Text="This action cannot be undone." />
        </StackPanel>
    </SmoothScrollViewer>
    <ContentDialog.BottomPanelContent>
        <UniformGrid Margin="15 0" Rows="0" Columns="2">
            <Button Theme="{StaticResource AccentButtonTheme}" Content="Confirm" />
            <Button Content="Cancel" />
        </UniformGrid>
    </ContentDialog.BottomPanelContent>
</ContentDialog>
```

## Showing from code

```csharp
var dialog = new MyCustomDialog();
await dialog.ShowAsync(window);          // Window
await dialog.ShowAsync(pleasantWindow);  // IPleasantWindow
await dialog.ShowAsync(topLevel);        // TopLevel
await dialog.ShowAsync();               // uses active window
```

## Closing from code

```csharp
await dialog.CloseAsync();              // close without result
await dialog.CloseAsync("confirmed");   // close with a result value
```

## Properties

| Property | Type | Description |
|---|---|---|
| `BottomPanelContent` | `object` | Content placed in the bottom panel (typically buttons) |
| `IsClosed` | `bool` | Whether the dialog has been closed |
| `IsClosing` | `bool` | Whether the dialog is currently closing |
| `OpenAnimation` | `Animation?` | Animation played when opening |
| `CloseAnimation` | `Animation?` | Animation played when closing |
| `ShowBackgroundAnimation` | `Animation?` | Animation for the dim overlay on open |
| `HideBackgroundAnimation` | `Animation?` | Animation for the dim overlay on close |

## Events

| Event | Description |
|---|---|
| `Closed` | Fired after the dialog has fully closed |

## Custom dialog class

Subclass `ContentDialog` to create reusable dialogs:

```csharp
public class ConfirmDialog : ContentDialog
{
    public ConfirmDialog()
    {
        InitializeComponent();
        // wire up buttons
    }

    public static async Task<bool> Show(IPleasantWindow parent, string message)
    {
        var dialog = new ConfirmDialog();
        // configure...
        string? result = await dialog.ShowAsync<string>(parent);
        return result == "confirmed";
    }
}
```

> **Note:** `MessageBox` and `PleasantDialog` in `PleasantUI.ToolKit` are built on top of `ContentDialog` and cover most common dialog scenarios without needing to subclass it directly.
