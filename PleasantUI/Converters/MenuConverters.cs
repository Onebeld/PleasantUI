using Avalonia.Controls.Converters;

namespace PleasantUI.Converters;

public static class MenuConverters
{
    /// <inheritdoc cref="PlatformKeyGestureConverter"/>
    public static readonly PlatformKeyGestureConverter KeyGesture = new();
    
    /// <inheritdoc cref="MarginMultiplierConverter"/>
    public static readonly MarginMultiplierConverter MarginMultiplier = new();
}