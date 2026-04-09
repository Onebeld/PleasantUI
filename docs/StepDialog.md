# StepDialog

A modal dialog that presents a sequence of numbered `StepItem` steps. Supports a title, description, status message, primary/secondary action buttons, and open/close animations consistent with `ContentDialog`.

## Basic usage

```xml
<StepDialog Title="Setup Wizard"
           Description="Follow these steps to configure your application"
           PrimaryButtonText="Next"
           SecondaryButtonText="Cancel">
    <StepItem Title="Step 1" Description="Configure settings" />
    <StepItem Title="Step 2" Description="Connect to server" />
    <StepItem Title="Step 3" Description="Complete setup" />
</StepDialog>
```

```csharp
await stepDialog.ShowAsync(window);
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Title` | `string?` | — | Dialog title |
| `Description` | `string?` | — | Optional description shown below the title |
| `StatusMessage` | `string?` | — | Status message shown at the bottom of the steps |
| `PrimaryButtonText` | `string?` | — | Text of the primary action button (null hides the button) |
| `SecondaryButtonText` | `string?` | — | Text of the secondary action button (null hides the button) |
| `MinDialogWidth` | `double` | `420` | Minimum width of the dialog card |
| `OpenAnimation` | `Animation?` | — | Animation played when the dialog opens |
| `CloseAnimation` | `Animation?` | — | Animation played when the dialog closes |
| `Steps` | `AvaloniaList<StepItem>` | — | Collection of steps displayed in the dialog |

## Events

| Event | Description |
|---|---|
| `PrimaryButtonClicked` | Raised when the primary button is clicked |
| `SecondaryButtonClicked` | Raised when the secondary button is clicked |
| `Closed` | Raised when the dialog is closed |

## Adding steps

Add steps in XAML:

```xml
<StepDialog>
    <StepItem Title="Step 1" Description="Configure settings" />
    <StepItem Title="Step 2" Description="Connect to server" />
    <StepItem Title="Step 3" Description="Complete setup" />
</StepDialog>
```

Or programmatically:

```csharp
stepDialog.Steps.Add(new StepItem
{
    Title = "Step 1",
    Description = "Configure settings"
});
```

## Status message

Display a status message at the bottom of the steps:

```xml
<StepDialog StatusMessage="Waiting for authentication…" />
```

```csharp
stepDialog.StatusMessage = "Processing step 2 of 3...";
```

## Button handling

Handle button clicks:

```csharp
stepDialog.PrimaryButtonClicked += (sender, e) =>
{
    // Move to next step or complete
};

stepDialog.SecondaryButtonClicked += (sender, e) =>
{
    // Cancel or go back
};
```

## Show/hide buttons

Hide buttons by setting their text to null:

```xml
<StepDialog PrimaryButtonText="Next"
           SecondaryButtonText="{x:Null}" />
```

## Custom animations

Provide custom open/close animations:

```xml
<StepDialog>
    <StepDialog.OpenAnimation>
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
    </StepDialog.OpenAnimation>
</StepDialog>
```

## Example

```csharp
var dialog = new StepDialog
{
    Title = "Installation Wizard",
    Description = "Complete these steps to install the application",
    PrimaryButtonText = "Next",
    SecondaryButtonText = "Cancel"
};

dialog.Steps.Add(new StepItem { Title = "Welcome", Description = "Introduction" });
dialog.Steps.Add(new StepItem { Title = "License", Description = "Accept terms" });
dialog.Steps.Add(new StepItem { Title = "Install", Description = "Install files" });

dialog.PrimaryButtonClicked += (sender, e) =>
{
    // Handle next step
};

await dialog.ShowAsync(window);
```

## Pseudo-classes

The control applies pseudo-classes based on state:
- `:open` - Applied when the dialog is open
- `:hasStatus` - Applied when a status message is set
- `:hasPrimary` - Applied when primary button text is set
- `:hasSecondary` - Applied when secondary button text is set

## Template parts

The control template must provide:
- `PART_CloseButton` - Close button
- `PART_PrimaryButton` - Primary action button
- `PART_SecondaryButton` - Secondary action button
- `PART_StepsHost` - ItemsControl that hosts the steps
