using Avalonia.Media;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace PleasantUI.ToolKit.Messages;

/// <summary>
/// Represents a message that asynchronously sets the color of the clipboard.
/// </summary>
public class AsyncClipboardSetColorMessage : AsyncRequestMessage<bool>
{
    /// <summary>
    /// Gets the color to set.
    /// </summary>
    public Color Color { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncClipboardSetColorMessage"/> class.
    /// </summary>
    /// <param name="color">The color to set.</param>
    public AsyncClipboardSetColorMessage(Color color)
    {
        Color = color;
    }
}