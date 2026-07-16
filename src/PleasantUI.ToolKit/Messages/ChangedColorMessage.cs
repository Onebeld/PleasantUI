using Avalonia.Media;
using PleasantUI.ToolKit.Models;

namespace PleasantUI.ToolKit.Messages;

internal record struct ChangedColorMessage(ThemeColor ThemeColor, Color NewColor, Color PreviousColor);