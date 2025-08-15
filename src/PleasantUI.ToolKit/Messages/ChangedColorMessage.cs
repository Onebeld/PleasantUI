using Avalonia.Media;
using PleasantUI.ToolKit.Models;

namespace PleasantUI.ToolKit.Messages;

internal class ChangedColorMessage
{
    public ThemeColor ThemeColor { get; }
    
    public Color NewColor { get; }
    
    public Color PreviousColor { get; }

    public ChangedColorMessage(ThemeColor themeColor, Color newColor, Color previousColor)
    {
        ThemeColor = themeColor;
        NewColor = newColor;
        PreviousColor = previousColor;
    }
}