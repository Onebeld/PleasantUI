# CrashReportDialog

A Fluent-styled modal crash-report dialog that displays three tabs (General, Exception, and Screenshot) and provides Send Report, Save Report, and Cancel actions. Extends `PleasantPopupElement` for integration with the PleasantUI overlay system.

## Basic usage

```csharp
var dialog = new CrashReportDialog
{
    ApplicationName = "MyApp",
    ApplicationVersion = "1.0.0",
    ExceptionType = "System.NullReferenceException",
    ExceptionMessage = "Object reference not set to an instance of an object",
    StackTrace = "at MyApp.Main()...",
    OccurredAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
};

var result = await dialog.ShowAsync(window);
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `ApplicationName` | `string?` | — | Application name shown in the General tab |
| `ApplicationVersion` | `string?` | — | Application version shown in the General tab |
| `ExceptionType` | `string?` | — | Fully-qualified exception type name |
| `ExceptionMessage` | `string?` | — | Exception message shown in both tabs |
| `ExceptionSource` | `string?` | — | Exception source assembly/module |
| `StackTrace` | `string?` | — | Full stack trace (inner exception + trace) |
| `OccurredAt` | `string?` | — | Formatted date/time when the crash occurred |
| `Screenshot` | `Bitmap?` | — | Screenshot bitmap (null hides the Screenshot tab) |
| `IncludeScreenshot` | `bool` | `true` | Whether the screenshot should be included in the report |
| `IsEmailRequired` | `bool` | `false` | Whether a valid email address is required before sending |
| `ShowScreenshotTab` | `bool` | `true` | Whether the Screenshot tab is shown |
| `StatusMessage` | `string?` | — | Status message shown in the sending/result state |
| `OpenAnimation` | `Animation?` | — | Open animation |
| `CloseAnimation` | `Animation?` | — | Close animation |

## Events

| Event | Description |
|---|---|
| `SendReportRequested` | Raised when the user clicks Send Report. Handler must call `ReportSuccess` or `ReportFailure` on the event args |
| `SaveReportRequested` | Raised when the user clicks Save Report |
| `Closed` | Raised when the dialog closes |

## Creating from exception

Use the convenience factory method to create a pre-populated dialog from an Exception:

```csharp
try
{
    // ... code that may throw
}
catch (Exception ex)
{
    var dialog = CrashReportDialog.FromException(
        ex,
        applicationName: "MyApp",
        applicationVersion: "1.0.0",
        screenshot: CaptureScreenshot());
    
    var result = await dialog.ShowAsync(window);
}
```

## Handling send report

Handle the `SendReportRequested` event to send the report to your server:

```csharp
dialog.SendReportRequested += async (sender, args) =>
{
    try
    {
        await SendReportToServer(args.Email, args.UserMessage, args.IncludeScreenshot);
        args.ReportSuccess?.Invoke();
    }
    catch (Exception ex)
    {
        args.ReportFailure?.Invoke(ex.Message);
    }
};
```

## Handling save report

Handle the `SaveReportRequested` event to save the report to disk:

```csharp
dialog.SaveReportRequested += (sender, args) =>
{
    var filePath = SaveReportToFile(args.UserMessage);
    // Optionally show a message to the user
};
```

## Email validation

When `IsEmailRequired` is true, the user must provide a valid email address:

```csharp
dialog.IsEmailRequired = true;
```

The dialog validates email format using a regex pattern and shows an error state for invalid emails.

## Screenshot

Provide a screenshot to include in the report:

```csharp
dialog.Screenshot = await CaptureWindowScreenshotAsync();
dialog.IncludeScreenshot = true;
```

Set `ShowScreenshotTab` to `false` to hide the screenshot tab even if a screenshot is provided.

## Result

The dialog returns a `CrashReportResult` enum:
- `Cancelled` - User dismissed without sending
- `Sent` - Report was sent successfully
- `Saved` - Report was saved to disk

```csharp
var result = await dialog.ShowAsync(window);
if (result == CrashReportResult.Sent)
{
    // Report sent successfully
}
```

## Custom animations

Provide custom open/close animations:

```xml
<dialogs:CrashReportDialog>
    <dialogs:CrashReportDialog.OpenAnimation>
        <Animation Duration="0:0:0.3">
            <KeyFrame Cue="0.0">
                <Setter Property="Opacity" Value="0" />
                <Setter Property="ScaleTransform.ScaleX" Value="0.9" />
                <Setter Property="ScaleTransform.ScaleY" Value="0.9" />
            </KeyFrame>
            <KeyFrame Cue="1.0">
                <Setter Property="Opacity" Value="1" />
                <Setter Property="ScaleTransform.ScaleX" Value="1" />
                <Setter Property="ScaleTransform.ScaleY" Value="1" />
            </KeyFrame>
        </Animation>
    </dialogs:CrashReportDialog.OpenAnimation>
</dialogs:CrashReportDialog>
```
