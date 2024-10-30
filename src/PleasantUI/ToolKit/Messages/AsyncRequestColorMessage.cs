using Avalonia.Media;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace PleasantUI.ToolKit.Messages;

/// <summary>
/// Represents a message that asynchronously requests the user to select a color.
/// </summary>
public class AsyncRequestColorMessage : AsyncRequestMessage<Color?>
{
    /// <summary>
    /// Gets the previous color that was selected.
    /// </summary>
    public Color PreviousColor { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncRequestColorMessage"/> class.
    /// </summary>
    /// <param name="previousColor">The previous color that was selected.</param>
    public AsyncRequestColorMessage(Color previousColor)
    {
        PreviousColor = previousColor;
    }
}