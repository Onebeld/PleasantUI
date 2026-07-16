using Avalonia.Media;

namespace PleasantUI.ToolKit.Messages;

internal record struct ChangeColorMessage(Color PreviousColor, TaskCompletionSource<Color?> TaskCompletionSource);