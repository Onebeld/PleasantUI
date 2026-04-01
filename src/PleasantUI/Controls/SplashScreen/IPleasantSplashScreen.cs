using Avalonia.Media;

namespace PleasantUI.Controls;

/// <summary>
/// Defines a splash screen shown during <see cref="PleasantWindow"/> startup.
/// </summary>
public interface IPleasantSplashScreen
{
    /// <summary>Application name shown when no icon or custom content is provided.</summary>
    string? AppName { get; }

    /// <summary>Image shown in the splash screen. Takes priority over <see cref="AppName"/>.</summary>
    IImage? AppIcon { get; }

    /// <summary>
    /// Fully custom content. When set, <see cref="AppIcon"/> and <see cref="AppName"/> are ignored.
    /// </summary>
    object? SplashScreenContent { get; }

    /// <summary>
    /// Background brush for the splash screen overlay.
    /// When null the window's background color is used.
    /// </summary>
    IBrush? Background { get; }

    /// <summary>
    /// Minimum time (ms) the splash screen is shown even if <see cref="RunTasks"/> finishes early.
    /// Prevents a jarring flash for fast loads.
    /// </summary>
    int MinimumShowTime { get; }

    /// <summary>
    /// Called by <see cref="PleasantWindow"/> to run background initialization tasks
    /// while the splash screen is visible.
    /// </summary>
    Task RunTasks(CancellationToken cancellationToken);
}
