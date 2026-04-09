using Avalonia.Controls;
using Avalonia.Interactivity;
using PleasantUI.Controls;
using PleasantUI.Example.Views.Pages;

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

        dialog.IsEmailRequired     = true;
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
