using Avalonia.Controls.Converters;

namespace PleasantUI.Converters;

/// <summary>
/// Provides static methods and converters for working with menus.
/// </summary>
public static class MenuConverters
{
    /// <inheritdoc cref="PlatformKeyGestureConverter" />
    public static readonly PlatformKeyGestureConverter KeyGesture = new();

    /// <inheritdoc cref="MarginMultiplierConverter" />
    public static readonly MarginMultiplierConverter MarginMultiplier = new();
}