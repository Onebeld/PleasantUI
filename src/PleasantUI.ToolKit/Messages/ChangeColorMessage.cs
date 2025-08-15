using Avalonia.Media;

namespace PleasantUI.ToolKit.Messages;

internal class ChangeColorMessage
{
    public Color PreviousColor { get; }
    
    public TaskCompletionSource<Color?> TaskCompletionSource { get; }

    public ChangeColorMessage(Color previousColor, TaskCompletionSource<Color?> taskCompletionSource)
    {
        PreviousColor = previousColor;
        TaskCompletionSource = taskCompletionSource;
    }
}