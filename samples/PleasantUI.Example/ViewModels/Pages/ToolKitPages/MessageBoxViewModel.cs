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
}
