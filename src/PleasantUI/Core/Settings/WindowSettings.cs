namespace PleasantUI.Core.Settings;

/// <summary>
/// Represents settings related to the appearance and behavior of windows.
/// </summary>
public class WindowSettings : ViewModelBase
{
    private bool _enableBlur;
    private bool _enableCustomTitleBar = true;
    private double _opacityLevel = 0.8;

    /// <summary>
    /// Gets or sets a value indicating whether to enable blur effect for windows.
    /// </summary>
    public bool EnableBlur
    {
        get => _enableBlur;
        set => RaiseAndSet(ref _enableBlur, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether to enable a custom title bar for windows.
    /// </summary>
    public bool EnableCustomTitleBar
    {
        get => _enableCustomTitleBar;
        set => RaiseAndSet(ref _enableCustomTitleBar, value);
    }

    /// <summary>
    /// Specifies the opacity level for windows
    /// </summary>
    public double OpacityLevel
    {
        get => _opacityLevel;
        set => RaiseAndSet(ref _opacityLevel, value);
    }
}