using Avalonia.Controls.Notifications;
using Avalonia.Media;

namespace PleasantUI.Controls;

public class PleasantSnackbarOptions
{
    public string Message { get; set; }
    public string? ButtonText { get; set; }
    public Action? ButtonAction { get; set; }
    public Geometry? Icon { get; set; }
    public NotificationType NotificationType { get; set; } = NotificationType.Information;
    public TimeSpan TimeSpan { get; set; } = default;

    public PleasantSnackbarOptions(string message)
    {
        Message = message;
    }
}