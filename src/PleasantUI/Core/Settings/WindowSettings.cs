namespace PleasantUI.Core.Settings;

/// <summary>
/// Represents settings related to the appearance and behavior of windows.
/// </summary>
public class WindowSettings : ViewModelBase
{
    /// <summary>
    /// Gets or sets a value indicating whether to enable blur effect for windows.
    /// </summary>
    public bool EnableBlur
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable a custom title bar for windows.
    /// </summary>
    public bool EnableCustomTitleBar
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    /// <summary>
    /// Specifies the opacity level for windows
    /// </summary>
    public double OpacityLevel
    {
        get;
        set => SetProperty(ref field, value);
    } = 0.8;
}