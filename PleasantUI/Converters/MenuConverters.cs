using Avalonia.Controls.Converters;

namespace PleasantUI.Converters;

public static class MenuConverters
{
    public static readonly PlatformKeyGestureConverter KeyGesture = new();
    public static readonly MarginMultiplierConverter MarginMultiplier = new();
}