# PleasantDialog

A rich modal dialog with header icon, subheader, command items (radio buttons, checkboxes, command links), progress bar, expandable footer, and full lifecycle events.

## Basic usage

```csharp
object result = await PleasantDialog.Show(window,
    header: "Sync settings",
    body:   "Choose how your settings should be synchronized.");
```

## Parameters

| Parameter | Type | Description |
|---|---|---|
| `header` | `string` | Bold title (localization key or raw string) |
| `body` | `string?` | Body text below the header |
| `buttons` | `IReadOnlyList<PleasantDialogButton>?` | Bottom buttons — defaults to a single OK |
| `commands` | `IReadOnlyList<PleasantDialogCommand>?` | Radio buttons, checkboxes, or command links in the body |
| `iconGeometryKey` | `string?` | Resource key of a `Geometry` shown next to the header |
| `headerBackground` | `IBrush?` | Custom header background |
| `iconForeground` | `IBrush?` | Custom icon color |
| `subHeader` | `string?` | Secondary text below the title |
| `extraContent` | `Control?` | Arbitrary control placed below commands |
| `onContentReady` | `Action<Control>?` | Called after extra content is placed, before dialog shows |
| `onDialogReady` | `Action<PleasantDialog>?` | Called with the dialog instance before it is shown — use for progress bar updates |
| `footer` | `object?` | Footer content |
| `footerExpandable` | `bool` | Whether the footer is hidden behind a toggle |
| `footerToggleText` | `string?` | Label for the footer toggle button |
| `style` | `MessageBoxStyle` | `Default` or `Danger` (red header) |

## PleasantDialogButton

```csharp
new PleasantDialogButton
{
    Text         = "Save",
    DialogResult = PleasantDialogResult.OK,
    IsDefault    = true   // gets accent styling and Enter key
}
```

Predefined buttons: `PleasantDialogButton.OK`, `.Cancel`, `.Yes`, `.No`, `.Close`, `.Retry`

## Command items

```csharp
commands: new PleasantDialogCommand[]
{
    new PleasantDialogRadioButton { Text = "Option A", IsChecked = true },
    new PleasantDialogRadioButton { Text = "Option B" },
    new PleasantDialogCheckBox   { Text = "Remember my choice" },
    new PleasantDialogCommandLink
    {
        Text        = "Advanced setup",
        Description = "Configure additional options manually.",
        ClosesOnInvoked = true,
        DialogResult    = "advanced"
    }
}
```

## Rich dialog example

```csharp
object result = await PleasantDialog.Show(window,
    header:          "Sync settings",
    body:            "Choose how your settings should be synchronized.",
    iconGeometryKey: "TuneRegular",
    subHeader:       "Affects all connected accounts.",
    commands: new PleasantDialogCommand[]
    {
        new PleasantDialogRadioButton { Text = "Sync automatically",  IsChecked = true },
        new PleasantDialogRadioButton { Text = "Ask before syncing" },
        new PleasantDialogCheckBox   { Text = "Remember my choice" }
    },
    buttons: new[]
    {
        new PleasantDialogButton { Text = "Save",   DialogResult = PleasantDialogResult.OK,     IsDefault = true },
        new PleasantDialogButton { Text = "Cancel", DialogResult = PleasantDialogResult.Cancel }
    },
    footer:           new TextBlock { Text = "Changes take effect after restart." },
    footerExpandable: true,
    footerToggleText: "More details");
```

## Danger style with command links

```csharp
object result = await PleasantDialog.Show(window,
    header: "Delete account",
    body:   "This cannot be undone.",
    style:  MessageBoxStyle.Danger,
    commands: new PleasantDialogCommand[]
    {
        new PleasantDialogCommandLink
        {
            Text        = "Delete everything",
            Description = "Removes all files and account data permanently.",
            DialogResult    = PleasantDialogResult.OK,
            ClosesOnInvoked = true
        }
    },
    buttons: new[]
    {
        new PleasantDialogButton { Text = "Cancel", DialogResult = PleasantDialogResult.Cancel, IsDefault = true }
    });
```

## Progress bar

Use `onDialogReady` to get a reference to the dialog and call `SetProgressBarState` from a background task:

```csharp
object result = await PleasantDialog.Show(window,
    header: "Processing",
    body:   "Please wait…",
    buttons: new[] { new PleasantDialogButton { Text = "Cancel", DialogResult = PleasantDialogResult.Cancel } },
    onDialogReady: dialog =>
    {
        dialog.SetProgressBarState(0);
        _ = Task.Run(async () =>
        {
            for (int i = 0; i <= 100; i += 5)
            {
                await Task.Delay(100);
                dialog.SetProgressBarState(i);
            }
            Dispatcher.UIThread.Post(() => _ = dialog.CloseAsync());
        });
    });
```

## Closing event

```csharp
var d = /* PleasantDialog instance */;
d.Closing += (_, args) =>
{
    if (!CanClose())
        args.Cancel = true;
};
```
