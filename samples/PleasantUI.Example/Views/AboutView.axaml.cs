using Avalonia.Controls;
using Avalonia.Media;
using PleasantUI.Core;
using System.Diagnostics;

namespace PleasantUI.Example.Views;

public partial class AboutView : UserControl
{
    public AboutView()
    {
        InitializeComponent();
        
        VersionTextBlock.Text = PleasantSettings.InformationalVersion;
        
        // Set version type badge
        var badgeTextBlock = VersionTypeBadge.Child as TextBlock;
        if (badgeTextBlock != null)
        {
            badgeTextBlock.Text = PleasantSettings.VersionTypeDescription;
        }
        VersionTypeBadge.Background = GetVersionTypeColor(PleasantSettings.VersionType);
    }

    private IBrush GetVersionTypeColor(PleasantVersionType versionType)
    {
        return versionType switch
        {
            PleasantVersionType.Stable => new SolidColorBrush(Color.Parse("#107C10")), // Green
            PleasantVersionType.BugFix => new SolidColorBrush(Color.Parse("#008272")), // Teal
            PleasantVersionType.Alpha => new SolidColorBrush(Color.Parse("#E81123")), // Red
            PleasantVersionType.Beta => new SolidColorBrush(Color.Parse("#FF8C00")), // Orange
            PleasantVersionType.ReleaseCandidate => new SolidColorBrush(Color.Parse("#8A2BE2")), // Purple
            PleasantVersionType.Canary => new SolidColorBrush(Color.Parse("#FFB900")), // Yellow
            _ => new SolidColorBrush(Color.Parse("#605E5C")) // Gray
        };
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