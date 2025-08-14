namespace PleasantUI.Core.Settings;

/// <summary>
/// Represents settings related to the appearance and behavior of windows.
/// </summary>
public class WindowSettings : ViewModelBase
{
    private bool _enableBlur = true;
    private bool _enableCustomTitleBar = true;
    private double _opacityLevel = 0.8;

    /// <summary>
    /// Gets or sets a value indicating whether to enable blur effect for windows.
    /// </summary>
    public bool EnableBlur
    {
        get => _enableBlur;
        set => SetProperty(ref _enableBlur, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether to enable a custom title bar for windows.
    /// </summary>
    public bool EnableCustomTitleBar
    {
        get => _enableCustomTitleBar;
        set => SetProperty(ref _enableCustomTitleBar, value);
    }

    /// <summary>
    /// Specifies the opacity level for windows
    /// </summary>
    public double OpacityLevel
    {
        get => _opacityLevel;
        set => SetProperty(ref _opacityLevel, value);
    }
}