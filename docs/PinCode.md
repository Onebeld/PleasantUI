# PinCode

A PIN / OTP entry control that renders a row of individual character cells. Supports digit-only, letter-only, or mixed input, optional password masking, clipboard paste, and full keyboard navigation.

## Basic usage

```xml
<!-- 4-digit PIN -->
<PinCode Count="4" Mode="Digit" />

<!-- 6-character OTP, masked -->
<PinCode Count="6" Mode="LetterOrDigit" PasswordChar="●" />
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Count` | `int` | `4` | Number of character cells |
| `Mode` | `PinCodeMode` | `LetterOrDigit` | Which character types are accepted |
| `PasswordChar` | `char` | `\0` (none) | When non-zero, each filled cell shows this character instead of the actual input |
| `Spacing` | `double` | `8` | Gap between cells in pixels |
| `CompleteCommand` | `ICommand?` | `null` | Command invoked when all cells are filled — receives `IList<string>` of digits |
| `Digits` | `IList<string>` | — | Current entered values, one entry per cell (read-only) |

## PinCodeMode values

| Value | Description |
|---|---|
| `Digit` | Only `0–9` accepted |
| `Letter` | Only letters accepted |
| `LetterOrDigit` | Letters and digits accepted |

## Events

| Event | Description |
|---|---|
| `Complete` | Raised (bubbling) when all cells are filled — args contain the `Digits` list |

## Handling completion

```csharp
// Via event
pinCode.Complete += (_, e) =>
{
    string pin = string.Concat(e.Digits);
    Verify(pin);
};
```

```xml
<!-- Via command binding -->
<PinCode Count="6"
         Mode="Digit"
         CompleteCommand="{Binding VerifyPinCommand}" />
```

## Keyboard navigation

- Arrow keys / Tab move focus between cells
- Backspace clears the current cell, then moves back
- Delete clears the current cell and moves forward
- Enter triggers the `Complete` event / command
- Paste (Ctrl+V / Cmd+V) fills cells from the clipboard, skipping invalid characters

## Password masking

```xml
<PinCode Count="4" Mode="Digit" PasswordChar="●" />
```

Any non-null character works as the mask — `●`, `*`, `•`, etc.
