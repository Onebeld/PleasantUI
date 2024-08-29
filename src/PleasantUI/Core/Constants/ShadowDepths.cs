using Avalonia.Media;

namespace PleasantUI.Core.Constants;

/// <summary>
/// Represents a collection of predefined shadow depths for different UI elements.
/// </summary>
public static class ShadowDepths
{
    /// <summary>
    /// Shadow depth for small windows.
    /// </summary>
    public static readonly BoxShadow SmallWindow = new()
    {
        Blur = 10,
        OffsetX = 0,
        OffsetY = 1,
        Color = Color.FromArgb(60, 0, 0, 0)
    };

    /// <summary>
    /// Shadow depth for modal windows.
    /// </summary>
    public static readonly BoxShadow ModalWindowDepth = new()
    {
        Blur = 60,
        OffsetX = 0,
        OffsetY = 6,
        Color = Color.FromArgb(110, 0, 0, 0)
    };

    /// <summary>
    /// Shadow depth for the color previewer element.
    /// </summary>
    public static readonly BoxShadow ColorPreviewer = new()
    {
        Blur = 10,
        OffsetX = 0,
        OffsetY = 0,
        Color = Color.FromArgb(190, 0, 0, 0)
    };
}