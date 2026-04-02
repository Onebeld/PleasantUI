# MessageBox

A modal dialog for simple confirmations and notifications. Supports multiple button configurations, danger styling, additional text (e.g. stack traces), and arbitrary extra content with value capture.

## Basic usage

```csharp
string result = await MessageBox.Show(window, "Confirm", "Do you want to proceed?");
// result == "OK"
```

## Button configurations

```csharp
// OK / Cancel
string result = await MessageBox.Show(window, "Confirm", "Save changes?",
    new[]
    {
        new MessageBoxButton { Text = "OK",     Result = "OK",     Default = true, IsKeyDown = true },
        new MessageBoxButton { Text = "Cancel", Result = "Cancel" }
    });

// Yes / No / Cancel
string result = await MessageBox.Show(window, "Question", "Save before closing?",
    new[]
    {
        new MessageBoxButton { Text = "Yes",    Result = "Yes",    Default = true, IsKeyDown = true },
        new MessageBoxButton { Text = "No",     Result = "No" },
        new MessageBoxButton { Text = "Cancel", Result = "Cancel" }
    });
```

## MessageBoxButton properties

| Property | Type | Description |
|---|---|---|
| `Text` | `string` | Button label |
| `Result` | `string` | Value returned when clicked |
| `Default` | `bool` | Gets accent/danger styling and is the Enter-key target |
| `IsKeyDown` | `bool` | Enter key triggers this button |
| `ThemeKey` | `string?` | Resource key of a `ControlTheme` to apply (e.g. `"DangerButtonTheme"`) |

## Danger style

```csharp
string result = await MessageBox.Show(window,
    "Delete account",
    "This action is irreversible.",
    new[]
    {
        new MessageBoxButton { Text = "Delete", Result = "Delete", Default = true },
        new MessageBoxButton { Text = "Cancel", Result = "Cancel", IsKeyDown = true }
    },
    style: MessageBoxStyle.Danger);
```

The danger style shows a red header with a warning icon. The default button automatically gets `DangerButtonTheme`.

## With additional text (stack trace / details)

```csharp
await MessageBox.Show(window, "Error", "An unexpected error occurred.",
    new[] { new MessageBoxButton { Text = "OK", Result = "OK", Default = true } },
    additionalText: exception.ToString());
```

The additional text is rendered in a read-only monospace `TextBox` with horizontal scroll.

## Custom extra content with value capture

```csharp
var radio1 = new RadioButton { Content = "Keep existing", GroupName = "g", IsChecked = true };
var radio2 = new RadioButton { Content = "Replace",       GroupName = "g" };
var panel  = new StackPanel  { Children = { radio1, radio2 } };

var result = await MessageBox.Show<string>(window,
    "Conflict",
    "A file with this name already exists.",
    extraContent: panel,
    valueSelector: _ => radio1.IsChecked == true ? "keep" : "replace",
    buttons: new[]
    {
        new MessageBoxButton { Text = "OK",     Result = "OK",     Default = true },
        new MessageBoxButton { Text = "Cancel", Result = "Cancel" }
    });

// result.Button == "OK" or "Cancel"
// result.Value == "keep" or "replace"
```

## onContentReady callback

Wire up event handlers on the extra content before the dialog is shown:

```csharp
var check = new CheckBox { Content = "Don't show again" };

await MessageBox.Show(window, "Info", "Welcome!",
    extraContent: check,
    onContentReady: ctrl =>
    {
        ((CheckBox)ctrl).IsCheckedChanged += (_, _) => UpdatePreference();
    });
```
