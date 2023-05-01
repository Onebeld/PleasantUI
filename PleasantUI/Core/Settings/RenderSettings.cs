namespace PleasantUI.Core.Settings;

public class RenderSettings : ViewModelBase
{
    private bool _enableShadowing = true;

    public bool EnableShadowing
    {
        get => _enableShadowing;
        set => RaiseAndSet(ref _enableShadowing, value);
    }
}