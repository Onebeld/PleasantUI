namespace PleasantUI.Core.Settings;

public class WindowSettings : ViewModelBase
{
    private bool _enableBlur;
    private bool _enableCustomTitleBar = true;
    private double _opacityLevel = 0.8;

    public bool EnableBlur
    {
        get => _enableBlur;
        set => RaiseAndSet(ref _enableBlur, value);
    }

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