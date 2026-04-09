# PopConfirm

Wraps any trigger control and shows a small popup with a header, body, and Confirm / Cancel buttons before executing a command. Supports Click and/or Focus trigger modes.

## Basic usage

```xml
<PopConfirm PopupHeader="Delete item"
            PopupContent="This action cannot be undone."
            ConfirmCommand="{Binding DeleteCommand}">
    <Button Content="Delete" Theme="{DynamicResource DangerButtonTheme}" />
</PopConfirm>
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `PopupHeader` | `object?` | — | Header content shown inside the popup |
| `PopupHeaderTemplate` | `IDataTemplate?` | — | Data template for `PopupHeader` |
| `PopupContent` | `object?` | — | Body content shown inside the popup |
| `PopupContentTemplate` | `IDataTemplate?` | — | Data template for `PopupContent` |
| `ConfirmCommand` | `ICommand?` | — | Command executed when the user confirms |
| `CancelCommand` | `ICommand?` | — | Command executed when the user cancels |
| `ConfirmCommandParameter` | `object?` | — | Parameter passed to `ConfirmCommand` |
| `CancelCommandParameter` | `object?` | — | Parameter passed to `CancelCommand` |
| `TriggerMode` | `PopConfirmTriggerMode` | `Click` | Which interaction opens the popup |
| `IsDropdownOpen` | `bool` | `false` | Whether the popup is currently open |
| `Placement` | `PlacementMode` | `BottomEdgeAlignedLeft` | Popup placement relative to the wrapped content |
| `Icon` | `object?` | — | Custom icon in the popup header area (defaults to a warning icon) |
| `HandleAsyncCommand` | `bool` | `true` | When true, waits for async commands to complete before closing |

## PopConfirmTriggerMode values

| Value | Description |
|---|---|
| `Click` | Opens on pointer press / button click |
| `Focus` | Opens when the wrapped control receives focus |
| `Click \| Focus` | Opens on either interaction |

## With cancel callback

```xml
<PopConfirm PopupHeader="Discard changes?"
            PopupContent="Your unsaved changes will be lost."
            ConfirmCommand="{Binding DiscardCommand}"
            CancelCommand="{Binding KeepEditingCommand}">
    <Button Content="Discard" />
</PopConfirm>
```

## Custom icon

```xml
<PopConfirm PopupHeader="Send message"
            PopupContent="Are you sure you want to send this?">
    <PopConfirm.Icon>
        <PathIcon Data="{DynamicResource SendRegular}" Width="16" Height="16" />
    </PopConfirm.Icon>
    <Button Content="Send" Theme="{DynamicResource AccentButtonTheme}" />
</PopConfirm>
```

## Focus trigger (e.g. form field confirmation)

```xml
<PopConfirm PopupHeader="Confirm email"
            PopupContent="Send a verification link to this address?"
            TriggerMode="Focus"
            ConfirmCommand="{Binding SendVerificationCommand}">
    <TextBox Text="{Binding Email}" PlaceholderText="Email address" />
</PopConfirm>
```

## Programmatic control

```csharp
popConfirm.IsDropdownOpen = true;  // open
popConfirm.IsDropdownOpen = false; // close
```
