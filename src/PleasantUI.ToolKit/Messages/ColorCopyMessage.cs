using Avalonia.Media;

namespace PleasantUI.ToolKit.Messages;

public class ColorCopyMessage
{
    public Color Color { get; }

    public ColorCopyMessage(Color color)
    {
        Color = color;
    }
}