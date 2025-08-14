using Avalonia.Media;

namespace PleasantUI.ToolKit.Messages;

public class PasteColorMessage
{
    public TaskCompletionSource<Color?> TaskCompletionSource { get; }

    public PasteColorMessage(TaskCompletionSource<Color?> taskCompletionSource)
    {
        TaskCompletionSource = taskCompletionSource;
    }
}