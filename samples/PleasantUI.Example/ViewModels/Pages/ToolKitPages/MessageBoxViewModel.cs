using Avalonia.Controls;
using Avalonia.Layout;
using Material.Icons;
using Material.Icons.Avalonia;
using PleasantUI.Controls;
using PleasantUI.Core;
using PleasantUI.Core.Localization;
using PleasantUI.ToolKit;
using PleasantUI.ToolKit.Enums;
using PleasantUI.ToolKit.Structures;

namespace PleasantUI.Example.ViewModels.Pages.ToolKitPages;

public class MessageBoxViewModel : ViewModelBase
{
    public string LastResult
    {
        get;
        set => SetProperty(ref field, value);
    } = "—";

    // Resolves a key under the MessageBox/ context with a hardcoded fallback
    private static string T(LocKey key, string fallback) =>
        Localizer.TrDefault(key, fallback);

    // Maps an internal result token back to a localized display string
    private static string LocalizeResult(string result) => result switch
    {
        "OK"     => T(LocKey.Ok,     "OK"),
        "Cancel" => T(LocKey.Cancel, "Cancel"),
        "Yes"    => T(LocKey.MessageBox__Yes,    "Yes"),
        "No"     => T(LocKey.MessageBox__No,     "No"),
        "Delete" => T(LocKey.MessageBox__Delete, "Delete"),
        _        => result
    };

    public async Task ShowDefault()
    {
        string result = await MessageBox.Show(
            PleasantUiExampleApp.Main,
            T(LocKey.MessageBox__Title,       "Information"),
            T(LocKey.MessageBox__DefaultText, "This is a default message box with a single OK button."));

        LastResult = LocalizeResult(result);
    }

    public async Task ShowOkCancel()
    {
        string result = await MessageBox.Show(
            PleasantUiExampleApp.Main,
            T(LocKey.MessageBox__ConfirmTitle,  "Confirm"),
            T(LocKey.MessageBox__OkCancelText,  "Do you want to proceed with this action?"),
            [
                new MessageBoxButton { Text = T(LocKey.Ok,     "OK"),     Result = "OK",     Default = true, IsKeyDown = true },
                new MessageBoxButton { Text = T(LocKey.Cancel, "Cancel"), Result = "Cancel" }
            ]);

        LastResult = LocalizeResult(result);
    }

    public async Task ShowYesNo()
    {
        string result = await MessageBox.Show(
            PleasantUiExampleApp.Main,
            T(LocKey.MessageBox__QuestionTitle, "Question"),
            T(LocKey.MessageBox__YesNoText,     "Would you like to save your changes before closing?"),
            new[]
            {
                new MessageBoxButton { Text = T(LocKey.MessageBox__Yes, "Yes"), Result = "Yes", Default = true, IsKeyDown = true },
                new MessageBoxButton { Text = T(LocKey.MessageBox__No,  "No"),  Result = "No" }
            });

        LastResult = LocalizeResult(result);
    }

    public async Task ShowYesNoCancel()
    {
        string result = await MessageBox.Show(
            PleasantUiExampleApp.Main,
            T(LocKey.MessageBox__QuestionTitle,   "Question"),
            T(LocKey.MessageBox__YesNoCancelText, "Save changes to the document before closing?"),
            new[]
            {
                new MessageBoxButton { Text = T(LocKey.MessageBox__Yes,    "Yes"),    Result = "Yes",    Default = true, IsKeyDown = true },
                new MessageBoxButton { Text = T(LocKey.MessageBox__No,     "No"),     Result = "No" },
                new MessageBoxButton { Text = T(LocKey.Cancel, "Cancel"), Result = "Cancel" }
            });

        LastResult = LocalizeResult(result);
    }

    public async Task ShowWithAdditionalText()
    {
        string result = await MessageBox.Show(
            PleasantUiExampleApp.Main,
            T(LocKey.MessageBox__ErrorTitle,  "Error"),
            T(LocKey.MessageBox__ErrorText,   "An unexpected error occurred while processing your request."),
            new[]
            {
                new MessageBoxButton { Text = T(LocKey.Ok, "OK"), Result = "OK", Default = true, IsKeyDown = true }
            },
            T(LocKey.MessageBox__ErrorDetail, "System.InvalidOperationException: Object reference not set to an instance of an object.\n   at SomeMethod() in File.cs:line 42"));

        LastResult = LocalizeResult(result);
    }

    public async Task ShowDanger()
    {
        string result = await MessageBox.Show(
            PleasantUiExampleApp.Main,
            T(LocKey.MessageBox__DangerTitle, "Delete"),
            T(LocKey.MessageBox__DangerText,  "This action is irreversible. All selected items will be permanently deleted."),
            new[]
            {
                new MessageBoxButton { Text = T(LocKey.MessageBox__Delete, "Delete"), Result = "Delete", Default = true },
                new MessageBoxButton { Text = T(LocKey.Cancel, "Cancel"), Result = "Cancel", IsKeyDown = true }
            },
            style: MessageBoxStyle.Danger);

        LastResult = LocalizeResult(result);
    }

    public async Task ShowCustomContent()
    {
        // Build extra content: warning icon + description + radio buttons
        RadioButton option1 = new() { Content = T(LocKey.MessageBox__CustomOption1, "Keep existing data"),   GroupName = "MBOptions", IsChecked = true };
        RadioButton option2 = new() { Content = T(LocKey.MessageBox__CustomOption2, "Replace with new data"), GroupName = "MBOptions" };
        RadioButton option3 = new() { Content = T(LocKey.MessageBox__CustomOption3, "Merge both"),            GroupName = "MBOptions" };

        StackPanel panel = new()
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
                        new PleasantIcon()
                        {
                            Icon   = CreateIcon(MaterialIconKind.InformationOutline),
                            Width  = 16,
                            Height = 16,
                        },
                        new TextBlock
                        {
                            Text        = T(LocKey.MessageBox__CustomHint, "Choose how to handle the conflict:"),
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

        MessageBoxResult<string> result = await MessageBox.Show<string>(
            PleasantUiExampleApp.Main,
            T(LocKey.MessageBox__CustomTitle, "Data conflict"),
            T(LocKey.MessageBox__CustomText,  "A file with this name already exists. How would you like to proceed?"),
            extraContent: panel,
            valueSelector: _ => option1.IsChecked == true ? "keep"
                              : option2.IsChecked == true ? "replace"
                              : "merge",
            buttons:
            [
                new MessageBoxButton { Text = T(LocKey.Ok, "OK"),     Result = "OK",     Default = true, IsKeyDown = true },
                new MessageBoxButton { Text = T(LocKey.Cancel, "Cancel"), Result = "Cancel" }
            ]);

        LastResult = result.Button == "Cancel"
            ? LocalizeResult("Cancel")
            : $"{LocalizeResult(result.Button)} → {result.Value}";
    }

    // ── PleasantDialog samples ────────────────────────────────────────────────

    public async Task ShowPleasantDialogRich()
    {
        PleasantDialogCheckBox remember = new() { Text = T(LocKey.PleasantDialog__RememberChoice, "Remember my choice") };

        object? result = await PleasantDialog.Show(
            PleasantUiExampleApp.Main,
            header: T(LocKey.PleasantDialog__RichTitle, "Sync settings"),
            body: T(LocKey.PleasantDialog__RichBody, "Choose how your settings should be synchronized across devices."),
            iconGeometryKey: "TuneRegular",
            subHeader: T(LocKey.PleasantDialog__RichSubHeader, "This affects all connected accounts."),
            commands:
            [
                new PleasantDialogRadioButton { Text = T(LocKey.PleasantDialog__RichOpt1, "Sync automatically"),  IsChecked = true },
                new PleasantDialogRadioButton { Text = T(LocKey.PleasantDialog__RichOpt2, "Ask before syncing") },
                new PleasantDialogRadioButton { Text = T(LocKey.PleasantDialog__RichOpt3, "Never sync") },
                remember
            ],
            buttons:
            [
                new PleasantDialogButton { Text = T(LocKey.PleasantDialog__Save, "Save"),   DialogResult = PleasantDialogResult.OK,     IsDefault = true },
                new PleasantDialogButton { Text = T(LocKey.Cancel, "Cancel"), DialogResult = PleasantDialogResult.Cancel }
            ],
            footer: new TextBlock
            {
                Text        = T(LocKey.PleasantDialog__RichFooter, "Changes take effect after restarting the application."),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Foreground  = null  // inherits theme color
            },
            footerExpandable: true,
            footerToggleText: T(LocKey.PleasantDialog__MoreDetails, "More details"));

        LastResult = result is PleasantDialogResult r
            ? r.ToString()
            : result?.ToString() ?? "—";
    }

    public async Task ShowPleasantDialogProgress()
    {
        CancellationTokenSource cts = new();

        object? result = await PleasantDialog.Show(
            PleasantUiExampleApp.Main,
            header: T(LocKey.PleasantDialog__ProgressTitle, "Processing"),
            body: T(LocKey.PleasantDialog__ProgressBody, "Please wait while the operation completes…"),
            iconGeometryKey: "ProgressHelper",
            subHeader: T(LocKey.PleasantDialog__ProgressSubHeader, "0%"),
            buttons: new[]
            {
                new PleasantDialogButton
                {
                    Text         = T(LocKey.Cancel, "Cancel"),
                    DialogResult = PleasantDialogResult.Cancel,
                    IsDefault    = true
                }
            },
            onDialogReady: d =>
            {
                _ = d;
                d.SetProgressBarState(0);

                // Run simulated work on a background thread
                _ = Task.Run(async () =>
                {
                    for (int i = 0; i <= 100; i += 5)
                    {
                        if (cts.Token.IsCancellationRequested) break;
                        await Task.Delay(120, cts.Token).ContinueWith(_ => { }, cts.Token);

                        int captured = i;
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            d.SetProgressBarState(captured);
                            // Update subheader text to show percentage
                            TextBlock? sub = d.FindControl<TextBlock>("SubHeaderText");
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
        LastResult = result.ToString() ?? "—";
    }

    public async Task ShowPleasantDialogDanger()
    {
        object? result = await PleasantDialog.Show(
            PleasantUiExampleApp.Main,
            header: T(LocKey.MessageBox__DangerTitle, "Permanently delete account"),
            body: T(LocKey.PleasantDialog__DangerBody, "This will remove all your data, settings, and history. This cannot be undone."),
            iconGeometryKey: "ErrorCircleRegular",
            style: MessageBoxStyle.Danger,
            commands:
            [
                new PleasantDialogCommandLink
                {
                    Text        = T(LocKey.PleasantDialog__DangerCmd1, "Delete everything"),
                    Description = T(LocKey.PleasantDialog__DangerCmd1Desc, "Removes all files, preferences, and account data permanently."),
                    DialogResult = PleasantDialogResult.OK,
                    ClosesOnInvoked = true
                },
                new PleasantDialogCommandLink
                {
                    Text        = T(LocKey.PleasantDialog__DangerCmd2, "Export data first"),
                    Description = T(LocKey.PleasantDialog__DangerCmd2Desc, "Download a copy of your data before deletion."),
                    DialogResult = "export",
                    ClosesOnInvoked = true
                }
            ],
            buttons:
            [
                new PleasantDialogButton { Text = T(LocKey.Cancel, "Cancel"), DialogResult = PleasantDialogResult.Cancel, IsDefault = true }
            ]);

        LastResult = result?.ToString() ?? "—";
    }
    
    private MaterialIcon CreateIcon(MaterialIconKind kind)
    {
        return new MaterialIcon()
        {
            Kind = kind
        };
    }
}
