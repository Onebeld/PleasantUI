using Avalonia.Controls;
using PleasantUI.Core;
using PleasantUI.Core.Localization;
using PleasantUI.ToolKit.Controls;

namespace PleasantUI.Example.ViewModels.Pages.ToolKitPages;

public class NoticeDialogViewModel : ViewModelBase
{
    public string LastResult
    {
        get;
        set => SetProperty(ref field, value);
    } = "—";

    // Resolves a key under the NoticeDialog/ context with a hardcoded fallback
    private static string T(string key, string fallback) =>
        Localizer.TrDefault(key, fallback, "NoticeDialog");

    public async Task ShowInfo()
    {
        NoticeDialog dialog = new NoticeDialog
        {
            Title = T("InfoTitle", "Information"),
            Message = T("InfoMessage", "Your changes have been saved successfully."),
            PrimaryButtonText = T("Ok", "OK"),
            Severity = NoticeSeverity.Info
        };

        dialog.PrimaryButtonClicked += (_, _) => _ = dialog.CloseAsync();
        await dialog.ShowAsync(PleasantUiExampleApp.Main as TopLevel);
        LastResult = T("InfoResult", "Info dialog closed");
    }

    public async Task ShowWarning()
    {
        NoticeDialog dialog = new NoticeDialog
        {
            Title = T("WarningTitle", "Warning"),
            Message = T("WarningMessage", "The file you are trying to open is larger than 100MB. Opening it may slow down the application."),
            PrimaryButtonText = T("Continue", "Continue"),
            SecondaryButtonText = T("Cancel", "Cancel"),
            Severity = NoticeSeverity.Warning
        };

        dialog.PrimaryButtonClicked += (_, _) => _ = dialog.CloseAsync();
        dialog.SecondaryButtonClicked += (_, _) => _ = dialog.CloseAsync();
        await dialog.ShowAsync(PleasantUiExampleApp.Main as TopLevel);
        LastResult = T("WarningResult", "Warning dialog closed");
    }

    public async Task ShowError()
    {
        NoticeDialog dialog = new NoticeDialog
        {
            Title = T("ErrorTitle", "Error"),
            Message = T("ErrorMessage", "An unexpected error occurred while processing your request. Please try again later."),
            PrimaryButtonText = T("Ok", "OK"),
            Severity = NoticeSeverity.Error
        };

        dialog.PrimaryButtonClicked += (_, _) => _ = dialog.CloseAsync();
        await dialog.ShowAsync(PleasantUiExampleApp.Main as TopLevel);
        LastResult = T("ErrorResult", "Error dialog closed");
    }

    public async Task ShowSuccess()
    {
        NoticeDialog dialog = new()
        {
            Title = T("SuccessTitle", "Success"),
            Message = T("SuccessMessage", "Your account has been created successfully. You can now log in."),
            PrimaryButtonText = T("Login", "Login"),
            Severity = NoticeSeverity.Success
        };

        dialog.PrimaryButtonClicked += (_, _) => _ = dialog.CloseAsync();
        await dialog.ShowAsync(PleasantUiExampleApp.Main as TopLevel);
        LastResult = T("SuccessResult", "Success dialog closed");
    }

    public async Task ShowWorkInProgress()
    {
        
        // Get version-specific message
        string message = T("WipStableMessage",
            "This is a stable release of the application. All features are fully tested and ready for production use.");

        NoticeDialog dialog = new()
        {
            Title = T("WipTitle", "Development Version"),
            Message = message,
            NoticeFooterText = T("WipFooter", "- Development Team"),
            PrimaryButtonText = T("Ok", "OK")
        };

        dialog.PrimaryButtonClicked += (_, _) => _ = dialog.CloseAsync();
        await dialog.ShowAsync(PleasantUiExampleApp.Main as TopLevel);
        LastResult = $"{T("WipResult", "Work in progress dialog shown")}";
    }
}
