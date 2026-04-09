# InstallWizard / WizardStep

A multi-step installation wizard with a sidebar step list, progress bar, and Back / Next / Cancel navigation. Can be embedded inline, shown as a `ContentDialog` modal, or hosted in a standalone `PleasantWindow`.

## Basic usage

```xml
<InstallWizard AppName="My App Setup" x:Name="Wizard"
               Finished="OnFinished" Cancelled="OnCancelled">
    <InstallWizard.Steps>
        <WizardStep Header="Welcome"
                    Description="Welcome to the setup wizard.">
            <WizardStep.Content>
                <TextBlock Text="Click Next to begin installation." Margin="20" />
            </WizardStep.Content>
        </WizardStep>

        <WizardStep Header="License">
            <WizardStep.Content>
                <ScrollViewer>
                    <TextBlock Text="{Binding LicenseText}" Margin="20" />
                </ScrollViewer>
            </WizardStep.Content>
        </WizardStep>

        <WizardStep Header="Install">
            <WizardStep.Content>
                <views:InstallProgressView />
            </WizardStep.Content>
        </WizardStep>
    </InstallWizard.Steps>
</InstallWizard>
```

## InstallWizard properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Steps` | `IList<WizardStep>` | — | Ordered list of wizard steps |
| `CurrentStepIndex` | `int` | `0` | Zero-based index of the currently visible step |
| `AppName` | `string?` | — | Application name shown in the sidebar header |
| `AppIcon` | `object?` | — | Icon shown in the sidebar header (PathIcon, Image, etc.) |
| `FooterText` | `string?` | — | Copyright / footer text at the bottom of the sidebar |
| `NextButtonText` | `string` | `"Next"` | Label for the Next/Finish button |
| `BackButtonText` | `string` | `"Back"` | Label for the Back button |
| `CancelButtonText` | `string` | `"Cancel"` | Label for the Cancel button |
| `ShowCancelButton` | `bool` | `true` | Whether the Cancel button is visible |
| `Progress` | `double` | — | Progress value 0–100 based on current step (read-only) |
| `CurrentStep` | `WizardStep?` | — | The currently active step (read-only) |

## InstallWizard events

| Event | Description |
|---|---|
| `Finished` | Raised when the user clicks Next on the last step |
| `Cancelled` | Raised when the user clicks Cancel |
| `StepChanged` | Raised whenever the active step changes — args include `StepIndex` and `Step` |

## WizardStep properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Header` | `string?` | — | Step title shown in the sidebar and content header |
| `Description` | `string?` | — | Optional subtitle shown below the header in the content area |
| `Content` | `object?` | — | Body content for this step |
| `ContentTemplate` | `IDataTemplate?` | — | Data template for `Content` (preferred over setting a Control directly) |
| `IsCompleted` | `bool` | `false` | Whether this step has been completed |
| `CompletionState` | `WizardStepCompletionState` | `Success` | Visual state of the completion indicator |
| `IsActive` | `bool` | — | Whether this is the currently visible step (set automatically) |
| `StepNumber` | `int` | — | 1-based display number in the sidebar (set automatically) |

## WizardStepCompletionState values

| Value | Description |
|---|---|
| `Success` | Green checkmark |
| `Warning` | Yellow/orange warning icon |
| `Error` | Red error icon |

## Show as modal dialog

```csharp
bool finished = await InstallWizard.ShowAsModalAsync(wizard, this);
if (finished)
    CompleteInstallation();
```

## Show as standalone window

```csharp
bool finished = await InstallWizard.ShowAsWindowAsync(wizard, owner: this, width: 760, height: 500);
```

## Programmatic navigation

```csharp
wizard.GoNext();   // advance to next step (raises Finished on last step)
wizard.GoBack();   // go to previous step
wizard.CurrentStepIndex = 2; // jump directly to a step
```

## Marking a step complete with a state

```csharp
wizard.CurrentStep!.IsCompleted    = true;
wizard.CurrentStep!.CompletionState = WizardStepCompletionState.Warning;
wizard.GoNext();
```
