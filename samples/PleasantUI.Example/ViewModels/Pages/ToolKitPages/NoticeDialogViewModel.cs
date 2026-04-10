using Avalonia.Controls;
using PleasantUI.Core;
using PleasantUI.Core.Localization;
using PleasantUI.ToolKit.Controls;

namespace PleasantUI.Example.ViewModels.Pages.ToolKitPages;

public class NoticeDialogViewModel : ViewModelBase
{
    private string _lastResult = "—";

    public string LastResult
    {
        get => _lastResult;
        set => SetProperty(ref _lastResult, value);
    }

    // Resolves a key under the NoticeDialog/ context with a hardcoded fallback
    private static string T(string key, string fallback) =>
        Localizer.TrDefault(key, fallback, "NoticeDialog");

    public async Task ShowInfo()
    {
        var dialog = new NoticeDialog
        {
            Title = T("InfoTitle", "Information"),
            Message = T("InfoMessage", "Your changes have been saved successfully."),
            PrimaryButtonText = T("Ok", "OK"),
            Severity = NoticeSeverity.Info
        };

        dialog.PrimaryButtonClicked += (_, _) => _ = dialog.CloseAsync();
        await dialog.ShowAsync(PleasantUiExampleApp.Main as Avalonia.Controls.TopLevel);
        LastResult = T("InfoResult", "Info dialog closed");
    }

    public async Task ShowWarning()
    {
        var dialog = new NoticeDialog
        {
            Title = T("WarningTitle", "Warning"),
            Message = T("WarningMessage", "The file you are trying to open is larger than 100MB. Opening it may slow down the application."),
            PrimaryButtonText = T("Continue", "Continue"),
            SecondaryButtonText = T("Cancel", "Cancel"),
            Severity = NoticeSeverity.Warning
        };

        dialog.PrimaryButtonClicked += (_, _) => _ = dialog.CloseAsync();
        dialog.SecondaryButtonClicked += (_, _) => _ = dialog.CloseAsync();
        await dialog.ShowAsync(PleasantUiExampleApp.Main as Avalonia.Controls.TopLevel);
        LastResult = T("WarningResult", "Warning dialog closed");
    }

    public async Task ShowError()
    {
        var dialog = new NoticeDialog
        {
            Title = T("ErrorTitle", "Error"),
            Message = T("ErrorMessage", "An unexpected error occurred while processing your request. Please try again later."),
            PrimaryButtonText = T("Ok", "OK"),
            Severity = NoticeSeverity.Error
        };

        dialog.PrimaryButtonClicked += (_, _) => _ = dialog.CloseAsync();
        await dialog.ShowAsync(PleasantUiExampleApp.Main as Avalonia.Controls.TopLevel);
        LastResult = T("ErrorResult", "Error dialog closed");
    }

    public async Task ShowSuccess()
    {
        var dialog = new NoticeDialog
        {
            Title = T("SuccessTitle", "Success"),
            Message = T("SuccessMessage", "Your account has been created successfully. You can now log in."),
            PrimaryButtonText = T("Login", "Login"),
            Severity = NoticeSeverity.Success
        };

        dialog.PrimaryButtonClicked += (_, _) => _ = dialog.CloseAsync();
        await dialog.ShowAsync(PleasantUiExampleApp.Main as Avalonia.Controls.TopLevel);
        LastResult = T("SuccessResult", "Success dialog closed");
    }

    public async Task ShowWorkInProgress()
    {
        var versionType = PleasantUI.Core.PleasantSettings.VersionType;
        string version = PleasantUI.Core.PleasantSettings.InformationalVersion;
        
        // Get version-specific message
        string message = versionType switch
        {
            PleasantUI.Core.PleasantVersionType.Stable => T("WipStableMessage", "This is a stable release of the application. All features are fully tested and ready for production use."),
            PleasantUI.Core.PleasantVersionType.BugFix => T("WipBugfixMessage", "This is a bugfix release containing important fixes for issues found in the stable version."),
            PleasantUI.Core.PleasantVersionType.Alpha => T("WipAlphaMessage", "This is an alpha release. Features may be incomplete and unstable. Use with caution."),
            PleasantUI.Core.PleasantVersionType.Beta => T("WipBetaMessage", "This is a beta release. Features are mostly complete but may still contain bugs."),
            PleasantUI.Core.PleasantVersionType.ReleaseCandidate => T("WipRcMessage", "This is a release candidate. It is close to the final release but may still have issues."),
            PleasantUI.Core.PleasantVersionType.Canary => T("WipCanaryMessage", "This is a canary build with the latest changes. It may be unstable and is intended for testing purposes only."),
            _ => T("WipDefaultMessage", "This version of the application is in active development. Features may change or be removed in future releases.")
        };

        var dialog = new NoticeDialog
        {
            Title = T("WipTitle", "Development Version"),
            Message = message,
            NoticeFooterText = T("WipFooter", "- Development Team"),
            PrimaryButtonText = T("Ok", "OK"),
            Severity = NoticeSeverity.WorkInProgress,
            Version = version,
            VersionType = PleasantUI.Core.PleasantSettings.VersionTypeDescription,
            VersionTypeEnum = PleasantUI.Core.PleasantSettings.VersionType,
            VersionLabel = T("VersionLabel", "Version")
        };

        dialog.PrimaryButtonClicked += (_, _) => _ = dialog.CloseAsync();
        await dialog.ShowAsync(PleasantUiExampleApp.Main as Avalonia.Controls.TopLevel);
        LastResult = $"{T("WipResult", "Work in progress dialog shown")}: {versionType}";
    }
}
