using Avalonia.Interactivity;
using PleasantUI.Controls;
using PleasantUI.Example.Views.Pages;
using PleasantUI.ToolKit.Controls;

namespace PleasantUI.Example.Views.Pages.PleasantControlPages;

public partial class LogViewerPanelPageView : LocalizedUserControl
{
    private int _counter;

    public LogViewerPanelPageView()
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
        OpenPanelBtn.Click    -= OnOpen;
        AddInfoBtn.Click      -= OnAddInfo;
        AddWarningBtn.Click   -= OnAddWarning;
        AddErrorBtn.Click     -= OnAddError;
        AddDebugBtn.Click     -= OnAddDebug;

        OpenPanelBtn.Click    += OnOpen;
        AddInfoBtn.Click      += OnAddInfo;
        AddWarningBtn.Click   += OnAddWarning;
        AddErrorBtn.Click     += OnAddError;
        AddDebugBtn.Click     += OnAddDebug;
    }

    private void OnOpen(object? s, RoutedEventArgs e)    => LogPanel.Open();
    private void OnAddInfo(object? s, RoutedEventArgs e)    => Add(LogLevel.Information, "Application started successfully.");
    private void OnAddWarning(object? s, RoutedEventArgs e) => Add(LogLevel.Warning,     "Memory usage is above 80%.");
    private void OnAddError(object? s, RoutedEventArgs e)   => Add(LogLevel.Error,       "Failed to connect to remote server.", "Connection timed out after 30 s.");
    private void OnAddDebug(object? s, RoutedEventArgs e)   => Add(LogLevel.Debug,       $"Debug tick #{++_counter}");

    private void Add(LogLevel level, string msg, string? details = null)
        => LogPanel.Append(level, msg, source: "Example", details: details);
}
