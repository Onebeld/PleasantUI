using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using PleasantUI.Controls;

namespace PleasantUI.Example.Desktop;

/// <summary>
/// Splash screen shown while the example app initializes.
/// Demonstrates custom content with the PleasantUI logo, app name, and a progress indicator.
/// </summary>
public class ExampleSplashScreen : IPleasantSplashScreen
{
    public string? AppName => null;          // using custom content instead
    public IImage? AppIcon => null;          // using custom content instead
    public IBrush? Background => null;       // inherits window background
    public int MinimumShowTime => 2000;      // show for at least 2 seconds

    public object? SplashScreenContent => BuildContent();

    public async Task RunTasks(CancellationToken cancellationToken)
    {
        // Simulate app initialization work
        await Task.Delay(500, cancellationToken);   // load settings
        await Task.Delay(500, cancellationToken);   // load themes
        await Task.Delay(300, cancellationToken);   // prepare resources
    }

    private static object BuildContent()
    {
        // Logo image from app resources
        var logo = new Image
        {
            Width  = 96,
            Height = 96,
            Stretch = Stretch.Uniform
        };

        // Bind to the DrawingImage resource defined in Images.axaml
        if (Application.Current!.TryFindResource("PleasantUILogo", out object? logoResource) &&
            logoResource is IImage image)
        {
            logo.Source = image;
        }

        var appName = new TextBlock
        {
            Text              = "PleasantUI",
            FontSize          = 28,
            FontWeight        = FontWeight.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin            = new Thickness(0, 16, 0, 0)
        };

        var subtitle = new TextBlock
        {
            Text              = "Example Application",
            FontSize          = 14,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin            = new Thickness(0, 4, 0, 0),
            Opacity           = 0.6
        };

        var progress = new ProgressBar
        {
            IsIndeterminate   = true,
            Width             = 160,
            Height            = 3,
            Margin            = new Thickness(0, 24, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var panel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment   = VerticalAlignment.Center,
            Children            = { logo, appName, subtitle, progress }
        };

        return panel;
    }
}
