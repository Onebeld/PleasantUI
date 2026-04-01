using Avalonia.Controls;
using Avalonia.Layout;
using PleasantUI.Core;
using PleasantUI.Core.Enums;
using PleasantUI.Core.Localization;
using PleasantUI.Core.Structures;
using PleasantUI.ToolKit;

namespace PleasantUI.Example.ViewModels.Pages.ToolKitPages;

public class MessageBoxViewModel : ViewModelBase
{
    private string _lastResult = "—";

    public string LastResult
    {
        get => _lastResult;
        set => SetProperty(ref _lastResult, value);
    }

    // Resolves a key under the MessageBox/ context with a hardcoded fallback
    private static string T(string key, string fallback) =>
        Localizer.TrDefault(key, fallback, "MessageBox");

    // Maps an internal result token back to a localized display string
    private static string LocalizeResult(string result) => result switch
    {
        "OK"     => T("Ok",     "OK"),
        "Cancel" => T("Cancel", "Cancel"),
        "Yes"    => T("Yes",    "Yes"),
        "No"     => T("No",     "No"),
        "Delete" => T("Delete", "Delete"),
        _        => result
    };

    public async Task ShowDefault()
    {
        string result = await MessageBox.Show(
            PleasantUiExampleApp.Main,
            T("Title",       "Information"),
            T("DefaultText", "This is a default message box with a single OK button."));

        LastResult = LocalizeResult(result);
    }

    public async Task ShowOkCancel()
    {
        string result = await MessageBox.Show(
            PleasantUiExampleApp.Main,
            T("ConfirmTitle",  "Confirm"),
            T("OkCancelText",  "Do you want to proceed with this action?"),
            new[]
            {
                new MessageBoxButton { Text = T("Ok",     "OK"),     Result = "OK",     Default = true, IsKeyDown = true },
                new MessageBoxButton { Text = T("Cancel", "Cancel"), Result = "Cancel" }
            });

        LastResult = LocalizeResult(result);
    }

    public async Task ShowYesNo()
    {
        string result = await MessageBox.Show(
            PleasantUiExampleApp.Main,
            T("QuestionTitle", "Question"),
            T("YesNoText",     "Would you like to save your changes before closing?"),
            new[]
            {
                new MessageBoxButton { Text = T("Yes", "Yes"), Result = "Yes", Default = true, IsKeyDown = true },
                new MessageBoxButton { Text = T("No",  "No"),  Result = "No" }
            });

        LastResult = LocalizeResult(result);
    }

    public async Task ShowYesNoCancel()
    {
        string result = await MessageBox.Show(
            PleasantUiExampleApp.Main,
            T("QuestionTitle",   "Question"),
            T("YesNoCancelText", "Save changes to the document before closing?"),
            new[]
            {
                new MessageBoxButton { Text = T("Yes",    "Yes"),    Result = "Yes",    Default = true, IsKeyDown = true },
                new MessageBoxButton { Text = T("No",     "No"),     Result = "No" },
                new MessageBoxButton { Text = T("Cancel", "Cancel"), Result = "Cancel" }
            });

        LastResult = LocalizeResult(result);
    }

    public async Task ShowWithAdditionalText()
    {
        string result = await MessageBox.Show(
            PleasantUiExampleApp.Main,
            T("ErrorTitle",  "Error"),
            T("ErrorText",   "An unexpected error occurred while processing your request."),
            new[]
            {
                new MessageBoxButton { Text = T("Ok", "OK"), Result = "OK", Default = true, IsKeyDown = true }
            },
            T("ErrorDetail", "System.InvalidOperationException: Object reference not set to an instance of an object.\n   at SomeMethod() in File.cs:line 42"));

        LastResult = LocalizeResult(result);
    }

    public async Task ShowDanger()
    {
        string result = await MessageBox.Show(
            PleasantUiExampleApp.Main,
            T("DangerTitle", "Delete"),
            T("DangerText",  "This action is irreversible. All selected items will be permanently deleted."),
            new[]
            {
                new MessageBoxButton { Text = T("Delete", "Delete"), Result = "Delete", Default = true },
                new MessageBoxButton { Text = T("Cancel", "Cancel"), Result = "Cancel", IsKeyDown = true }
            },
            style: MessageBoxStyle.Danger);

        LastResult = LocalizeResult(result);
    }

    public async Task ShowCustomContent()
    {
        // Build extra content: warning icon + description + radio buttons
        var option1 = new RadioButton { Content = T("CustomOption1", "Keep existing data"),   GroupName = "MBOptions", IsChecked = true };
        var option2 = new RadioButton { Content = T("CustomOption2", "Replace with new data"), GroupName = "MBOptions" };
        var option3 = new RadioButton { Content = T("CustomOption3", "Merge both"),            GroupName = "MBOptions" };

        var panel = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    Children =
                    {
                        new PathIcon
                        {
                            Data   = MaterialIcons.InformationOutline,
                            Width  = 16,
                            Height = 16,
                        },
                        new TextBlock
                        {
                            Text        = T("CustomHint", "Choose how to handle the conflict:"),
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    }
                },
                option1,
                option2,
                option3
            }
        };

        var result = await MessageBox.Show<string>(
            PleasantUiExampleApp.Main,
            T("CustomTitle", "Data conflict"),
            T("CustomText",  "A file with this name already exists. How would you like to proceed?"),
            extraContent: panel,
            valueSelector: _ => option1.IsChecked == true ? "keep"
                              : option2.IsChecked == true ? "replace"
                              : "merge",
            buttons: new[]
            {
                new MessageBoxButton { Text = T("Ok", "OK"),     Result = "OK",     Default = true, IsKeyDown = true },
                new MessageBoxButton { Text = T("Cancel", "Cancel"), Result = "Cancel" }
            });

        LastResult = result.Button == "Cancel"
            ? LocalizeResult("Cancel")
            : $"{LocalizeResult(result.Button)} → {result.Value}";
    }

    // ── PleasantDialog samples ────────────────────────────────────────────────

    private static string TD(string key, string fallback) =>
        Localizer.TrDefault(key, fallback, "PleasantDialog");

    public async Task ShowPleasantDialogRich()
    {
        var remember = new PleasantDialogCheckBox { Text = TD("RememberChoice", "Remember my choice") };

        var result = await PleasantDialog.Show(
            PleasantUiExampleApp.Main,
            header: TD("RichTitle", "Sync settings"),
            body: TD("RichBody", "Choose how your settings should be synchronized across devices."),
            iconGeometryKey: "TuneRegular",
            subHeader: TD("RichSubHeader", "This affects all connected accounts."),
            commands: new PleasantDialogCommand[]
            {
                new PleasantDialogRadioButton { Text = TD("RichOpt1", "Sync automatically"),  IsChecked = true },
                new PleasantDialogRadioButton { Text = TD("RichOpt2", "Ask before syncing") },
                new PleasantDialogRadioButton { Text = TD("RichOpt3", "Never sync") },
                remember
            },
            buttons: new[]
            {
                new PleasantDialogButton { Text = TD("Save", "Save"),   DialogResult = PleasantDialogResult.OK,     IsDefault = true },
                new PleasantDialogButton { Text = TD("Cancel", "Cancel"), DialogResult = PleasantDialogResult.Cancel }
            },
            footer: new TextBlock
            {
                Text        = TD("RichFooter", "Changes take effect after restarting the application."),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Foreground  = null  // inherits theme color
            },
            footerExpandable: true,
            footerToggleText: TD("MoreDetails", "More details"));

        LastResult = result is PleasantDialogResult r
            ? r.ToString()
            : result?.ToString() ?? "—";
    }

    public async Task ShowPleasantDialogProgress()
    {
        PleasantDialog? dialogRef = null;
        var cts = new CancellationTokenSource();

        var result = await PleasantDialog.Show(
            PleasantUiExampleApp.Main,
            header: TD("ProgressTitle", "Processing"),
            body: TD("ProgressBody", "Please wait while the operation completes…"),
            iconGeometryKey: "ProgressHelper",
            subHeader: TD("ProgressSubHeader", "0%"),
            buttons: new[]
            {
                new PleasantDialogButton
                {
                    Text         = TD("Cancel", "Cancel"),
                    DialogResult = PleasantDialogResult.Cancel,
                    IsDefault    = true
                }
            },
            onDialogReady: d =>
            {
                dialogRef = d;
                d.SetProgressBarState(0);

                // Run simulated work on a background thread
                _ = Task.Run(async () =>
                {
                    for (int i = 0; i <= 100; i += 5)
                    {
                        if (cts.Token.IsCancellationRequested) break;
                        await Task.Delay(120, cts.Token).ContinueWith(_ => { });

                        int captured = i;
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            d.SetProgressBarState(captured);
                            // Update subheader text to show percentage
                            var sub = d.FindControl<TextBlock>("SubHeaderText");
                            if (sub is not null)
                            {
                                sub.Text      = $"{captured}%";
                                sub.IsVisible = true;
                            }
                        });

                        if (captured >= 100)
                        {
                            await Task.Delay(300);
                            Avalonia.Threading.Dispatcher.UIThread.Post(() => _ = d.CloseAsync());
                            break;
                        }
                    }
                }, cts.Token);
            });

        cts.Cancel();
        LastResult = result?.ToString() ?? "—";
    }

    public async Task ShowPleasantDialogDanger()
    {
        var result = await PleasantDialog.Show(
            PleasantUiExampleApp.Main,
            header: TD("DangerTitle", "Permanently delete account"),
            body: TD("DangerBody", "This will remove all your data, settings, and history. This cannot be undone."),
            iconGeometryKey: "ErrorCircleRegular",
            style: MessageBoxStyle.Danger,
            commands: new PleasantDialogCommand[]
            {
                new PleasantDialogCommandLink
                {
                    Text        = TD("DangerCmd1", "Delete everything"),
                    Description = TD("DangerCmd1Desc", "Removes all files, preferences, and account data permanently."),
                    DialogResult = PleasantDialogResult.OK,
                    ClosesOnInvoked = true
                },
                new PleasantDialogCommandLink
                {
                    Text        = TD("DangerCmd2", "Export data first"),
                    Description = TD("DangerCmd2Desc", "Download a copy of your data before deletion."),
                    DialogResult = "export",
                    ClosesOnInvoked = true
                }
            },
            buttons: new[]
            {
                new PleasantDialogButton { Text = TD("Cancel", "Cancel"), DialogResult = PleasantDialogResult.Cancel, IsDefault = true }
            });

        LastResult = result?.ToString() ?? "—";
    }
}
