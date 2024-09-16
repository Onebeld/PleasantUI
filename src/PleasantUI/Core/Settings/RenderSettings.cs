namespace PleasantUI.Core.Settings;

/// <summary>
/// Represents settings related to the renderer.
/// </summary>
public class RenderSettings : ViewModelBase
{
    private bool _enableShadowing = true;

    /// <summary>
    /// Adjusts the switching of shadows in the entire program
    /// </summary>
    public bool EnableShadowing
    {
        get => _enableShadowing;
        set => RaiseAndSet(ref _enableShadowing, value);
    }
}