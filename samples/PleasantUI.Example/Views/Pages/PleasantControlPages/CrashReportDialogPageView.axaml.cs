using Avalonia.Controls;
using Avalonia.Interactivity;
using PleasantUI.ToolKit.Controls;
using PleasantUI.Example.Views.Pages;
using PleasantUI.Core.Localization;

namespace PleasantUI.Example.Views.Pages.PleasantControlPages;

public partial class CrashReportDialogPageView : LocalizedUserControl
{
    public CrashReportDialogPageView()
    {
        InitializeComponent();
        WireHandlers();
    }

    protected override void ReinitializeComponent()
    {
        InitializeComponent();
        WireHandlers();
    }

    private void WireHandlers()
    {
        OpenBasicBtn.Click         -= OnOpenBasic;
        OpenEmailRequiredBtn.Click -= OnOpenEmailRequired;
        OpenBasicBtn.Click         += OnOpenBasic;
        OpenEmailRequiredBtn.Click += OnOpenEmailRequired;
    }

    private async void OnOpenBasic(object? s, RoutedEventArgs e)
    {
        var ex = new NullReferenceException("Object reference not set to an instance of an object.");

        var dialog = CrashReportDialog.FromException(
            ex,
            applicationName:    "PleasantUI Example",
            applicationVersion: "1.0.0");

        dialog.SendReportLabel = Localizer.Tr("CrashReportDialog/SendReportLabel");
        dialog.SaveReportLabel = Localizer.Tr("CrashReportDialog/SaveReportLabel");
        dialog.CancelLabel = Localizer.Tr("CrashReportDialog/CancelLabel");
        dialog.EmailLabel = Localizer.Tr("CrashReportDialog/EmailLabel");
        dialog.EmailValidationError = Localizer.Tr("CrashReportDialog/EmailValidationError");
        dialog.IncludeScreenshotLabel = Localizer.Tr("CrashReportDialog/IncludeScreenshotLabel");
        dialog.MessageLabel = Localizer.Tr("CrashReportDialog/MessageLabel");
        dialog.GeneralTabLabel = Localizer.Tr("CrashReportDialog/GeneralTabLabel");
        dialog.ExceptionTabLabel = Localizer.Tr("CrashReportDialog/ExceptionTabLabel");
        dialog.ScreenshotTabLabel = Localizer.Tr("CrashReportDialog/ScreenshotTabLabel");
        dialog.SendingMessage = Localizer.Tr("CrashReportDialog/SendingMessage");
        dialog.SuccessMessage = Localizer.Tr("CrashReportDialog/SuccessMessage");
        dialog.FailureMessagePrefix = Localizer.Tr("CrashReportDialog/FailureMessagePrefix");
        dialog.CrashedText = Localizer.Tr("CrashReportDialog/CrashedText");
        dialog.OccurredAtText = Localizer.Tr("CrashReportDialog/OccurredAtText");
        dialog.WhatWereYouDoingText = Localizer.Tr("CrashReportDialog/WhatWereYouDoingText");
        dialog.ApplicationLabel = Localizer.Tr("CrashReportDialog/ApplicationLabel");
        dialog.VersionLabel = Localizer.Tr("CrashReportDialog/VersionLabel");
        dialog.UserMessagePlaceholder = Localizer.Tr("CrashReportDialog/UserMessagePlaceholder");

        dialog.SendReportRequested += OnSendRequested;
        dialog.SaveReportRequested += OnSaveRequested;

        var topLevel = TopLevel.GetTopLevel(this);
        var result   = await dialog.ShowAsync(topLevel);

        ResultLabel.Text = result.ToString();
    }

    private async void OnOpenEmailRequired(object? s, RoutedEventArgs e)
    {
        var ex = new InvalidOperationException("Cannot perform this operation in the current state.");

        var dialog = CrashReportDialog.FromException(
            ex,
            applicationName:    "PleasantUI Example",
            applicationVersion: "1.0.0");

        dialog.IsEmailRequired = true;
        dialog.SendReportLabel = Localizer.Tr("CrashReportDialog/SendReportLabel");
        dialog.SaveReportLabel = Localizer.Tr("CrashReportDialog/SaveReportLabel");
        dialog.CancelLabel = Localizer.Tr("CrashReportDialog/CancelLabel");
        dialog.EmailLabel = Localizer.Tr("CrashReportDialog/EmailLabel");
        dialog.EmailValidationError = Localizer.Tr("CrashReportDialog/EmailValidationError");
        dialog.IncludeScreenshotLabel = Localizer.Tr("CrashReportDialog/IncludeScreenshotLabel");
        dialog.MessageLabel = Localizer.Tr("CrashReportDialog/MessageLabel");
        dialog.GeneralTabLabel = Localizer.Tr("CrashReportDialog/GeneralTabLabel");
        dialog.ExceptionTabLabel = Localizer.Tr("CrashReportDialog/ExceptionTabLabel");
        dialog.ScreenshotTabLabel = Localizer.Tr("CrashReportDialog/ScreenshotTabLabel");
        dialog.SendingMessage = Localizer.Tr("CrashReportDialog/SendingMessage");
        dialog.SuccessMessage = Localizer.Tr("CrashReportDialog/SuccessMessage");
        dialog.FailureMessagePrefix = Localizer.Tr("CrashReportDialog/FailureMessagePrefix");
        dialog.CrashedText = Localizer.Tr("CrashReportDialog/CrashedText");
        dialog.OccurredAtText = Localizer.Tr("CrashReportDialog/OccurredAtText");
        dialog.WhatWereYouDoingText = Localizer.Tr("CrashReportDialog/WhatWereYouDoingText");
        dialog.ApplicationLabel = Localizer.Tr("CrashReportDialog/ApplicationLabel");
        dialog.VersionLabel = Localizer.Tr("CrashReportDialog/VersionLabel");
        dialog.UserMessagePlaceholder = Localizer.Tr("CrashReportDialog/UserMessagePlaceholder");
        dialog.SendReportRequested += OnSendRequested;
        dialog.SaveReportRequested += OnSaveRequested;

        var topLevel = TopLevel.GetTopLevel(this);
        var result   = await dialog.ShowAsync(topLevel);

        ResultLabel.Text = result.ToString();
    }

    // ── Send / Save handlers ──────────────────────────────────────────────────

    private static void OnSendRequested(object? sender, SendReportEventArgs e)
    {
        // Simulate an async send — in a real app you'd call your reporting API here.
        _ = Task.Run(async () =>
        {
            await Task.Delay(1500); // simulate network latency
            // Report success back to the dialog so it transitions to the success state.
            e.ReportSuccess?.Invoke();
        });
    }

    private static void OnSaveRequested(object? sender, SaveReportEventArgs e)
    {
        // In a real app you'd open a SaveFileDialog and write the report.
        // Here we just acknowledge it.
        System.Diagnostics.Debug.WriteLine($"[CrashReport] Save requested. Message: {e.UserMessage}");
    }
}
