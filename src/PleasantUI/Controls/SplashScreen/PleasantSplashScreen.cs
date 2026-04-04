using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace PleasantUI.Controls;

/// <summary>
/// The overlay control rendered on top of <see cref="PleasantWindow"/> content during startup.
/// Populated automatically from <see cref="IPleasantSplashScreen"/> — do not instantiate directly.
/// </summary>
public class PleasantSplashScreen : TemplatedControl
{
    /// <summary>Defines the <see cref="SplashScreen"/> property.</summary>
    public static readonly StyledProperty<IPleasantSplashScreen?> SplashScreenProperty =
        AvaloniaProperty.Register<PleasantSplashScreen, IPleasantSplashScreen?>(nameof(SplashScreen));

    /// <summary>Defines the <see cref="AppName"/> property.</summary>
    public static readonly StyledProperty<string?> AppNameProperty =
        AvaloniaProperty.Register<PleasantSplashScreen, string?>(nameof(AppName));

    /// <summary>Defines the <see cref="AppIcon"/> property.</summary>
    public static readonly StyledProperty<IImage?> AppIconProperty =
        AvaloniaProperty.Register<PleasantSplashScreen, IImage?>(nameof(AppIcon));

    /// <summary>Defines the <see cref="SplashContent"/> property.</summary>
    public static readonly StyledProperty<object?> SplashContentProperty =
        AvaloniaProperty.Register<PleasantSplashScreen, object?>(nameof(SplashContent));

    /// <summary>The <see cref="IPleasantSplashScreen"/> driving this control.</summary>
    public IPleasantSplashScreen? SplashScreen
    {
        get => GetValue(SplashScreenProperty);
        set => SetValue(SplashScreenProperty, value);
    }

    /// <summary>Application name displayed when no icon or custom content is set.</summary>
    public string? AppName
    {
        get => GetValue(AppNameProperty);
        set => SetValue(AppNameProperty, value);
    }

    /// <summary>Image displayed in the splash screen.</summary>
    public IImage? AppIcon
    {
        get => GetValue(AppIconProperty);
        set => SetValue(AppIconProperty, value);
    }

    /// <summary>Fully custom content — overrides icon and name.</summary>
    public object? SplashContent
    {
        get => GetValue(SplashContentProperty);
        set => SetValue(SplashContentProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SplashScreenProperty && change.NewValue is IPleasantSplashScreen splash)
        {
            // Populate display properties from the interface
            if (splash.Background is not null)
                Background = splash.Background;

            if (splash.SplashScreenContent is not null)
                SplashContent = splash.SplashScreenContent;
            else if (splash.AppIcon is not null)
                AppIcon = splash.AppIcon;
            else if (!string.IsNullOrEmpty(splash.AppName))
                AppName = splash.AppName;
        }
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(PleasantSplashScreen);
}
