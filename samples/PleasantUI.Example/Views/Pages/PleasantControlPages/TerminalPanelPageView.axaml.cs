using Avalonia.Interactivity;
using PleasantUI.Example.Views.Pages;

namespace PleasantUI.Example.Views.Pages.PleasantControlPages;

public partial class TerminalPanelPageView : LocalizedUserControl
{
    private int _line;

    public TerminalPanelPageView()
    {
        InitializeComponent();
        WireHandlers();
        Terminal.AppendOutput("PleasantUI Terminal Demo\nType a command and press Enter.\n\n");
    }

    protected override void ReinitializeComponent()
    {
        InitializeComponent();
        WireHandlers();
    }

    private void WireHandlers()
    {
        Terminal.CommandSubmitted    -= OnCommand;
        SimulateOutputBtn.Click      -= OnSimulate;
        ClearBtn.Click               -= OnClear;

        Terminal.CommandSubmitted    += OnCommand;
        SimulateOutputBtn.Click      += OnSimulate;
        ClearBtn.Click               += OnClear;
    }

    private void OnCommand(object? s, string cmd)
        => Terminal.AppendOutput($"$ {cmd}\nCommand received: '{cmd}'\n\n");

    private void OnSimulate(object? s, RoutedEventArgs e)
        => Terminal.AppendOutput($"[{DateTime.Now:HH:mm:ss}] Simulated output line #{++_line}\n");

    private void OnClear(object? s, RoutedEventArgs e)
        => Terminal.ClearOutput();
}
