using Avalonia.Media;

namespace PleasantUI.ToolKit.Messages;

internal record struct PasteColorMessage(TaskCompletionSource<Color?> TaskCompletionSource);