using Avalonia.Media;

namespace PleasantUI.Core.Constants;

public static class ShadowDepths
{
    public static readonly BoxShadow SmallWindow = new()
    {
        Blur = 10,
        OffsetX = 0,
        OffsetY = 1,
        Color = Color.FromArgb(60, 0, 0, 0)
    };

    public static readonly BoxShadow ModalWindowDepth = new()
    {
        Blur = 60,
        OffsetX = 0,
        OffsetY = 6,
        Color = Color.FromArgb(110, 0, 0, 0)
    };

    public static readonly BoxShadow ColorPreviewer = new()
    {
        Blur = 10,
        OffsetX = 0,
        OffsetY = 0,
        Color = Color.FromArgb(190, 0, 0, 0)
    };
}