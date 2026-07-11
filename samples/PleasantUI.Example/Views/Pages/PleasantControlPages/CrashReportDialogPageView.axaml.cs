using Avalonia.Controls;
using Avalonia.Interactivity;
using PleasantUI.ToolKit.Controls;

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
        OpenBasicBtn.Click -= OnOpenBasic;
        OpenEmailRequiredBtn.Click -= OnOpenEmailRequired;
        OpenBasicBtn.Click += OnOpenBasic;
        OpenEmailRequiredBtn.Click += OnOpenEmailRequired;
    }

    private async void OnOpenBasic(object? s, RoutedEventArgs e)
    {
        NullReferenceException ex = new("Object reference not set to an instance of an object.");

        CrashReportDialog dialog = CrashReportDialog.FromException(
            ex,
            applicationName: "PleasantUI Example",
            applicationVersion: "1.0.0");

        dialog.SendReportRequested += OnSendRequested;

        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        CrashReportResult result = await dialog.ShowAsync<CrashReportResult>(topLevel);

        ResultLabel.Text = result.ToString();
    }

    private async void OnOpenEmailRequired(object? s, RoutedEventArgs e)
    {
        InvalidOperationException ex = new("Cannot perform this operation in the current state.");

        CrashReportDialog dialog = CrashReportDialog.FromException(
            ex,
            applicationName: "PleasantUI Example",
            applicationVersion: "1.0.0");

        dialog.IsEmailRequired = true;
        dialog.SendReportRequested += OnSendRequested;

        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        CrashReportResult result = await dialog.ShowAsync<CrashReportResult>(topLevel);

        ResultLabel.Text = result.ToString();
    }

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
}