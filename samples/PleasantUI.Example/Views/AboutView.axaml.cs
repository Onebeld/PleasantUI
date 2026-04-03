using Avalonia.Controls;
using System.Diagnostics;

namespace PleasantUI.Example.Views;

public partial class AboutView : UserControl
{
    public AboutView()
    {
        InitializeComponent();
        
        Version pleasantUiVersion = typeof(PleasantTheme).Assembly.GetName().Version!;
        VersionTextBlock.Text = $"Version: {pleasantUiVersion.Major}.{pleasantUiVersion.Minor}.{pleasantUiVersion.Build}";
    }

    private void GitHubButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://github.com/Onebeld/PleasantUI") { UseShellExecute = true });
    }

    private void PatreonButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://patreon.com/onebeld") { UseShellExecute = true });
    }

    private void BoostyButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://boosty.to/onebeld") { UseShellExecute = true });
    }
}