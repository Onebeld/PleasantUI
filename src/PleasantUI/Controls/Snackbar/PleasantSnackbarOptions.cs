using Avalonia.Controls.Notifications;
using Avalonia.Media;

namespace PleasantUI.Controls;

/// <summary>
/// Represents options for configuring a PleasantSnackbar notification.
/// </summary>
public class PleasantSnackbarOptions
{
    /// <summary>
    /// Gets or sets the message displayed in the snackbar.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the text for an optional action button in the snackbar.
    /// </summary>
    public string? ButtonText { get; set; }

    /// <summary>
    /// Gets or sets the action to be performed when the button is clicked.
    /// </summary>
    public Action? ButtonAction { get; set; }

    /// <summary>
    /// Gets or sets the icon displayed in the snackbar.
    /// </summary>
    public Geometry? Icon { get; set; }

    /// <summary>
    /// Gets or sets the type of notification (e.g., Information, Warning, Error, Success).
    /// Default is <see cref="NotificationType.Information"/>.
    /// </summary>
    public NotificationType NotificationType { get; set; } = NotificationType.Information;

    /// <summary>
    /// Gets or sets the duration for which the snackbar is displayed.
    /// </summary>
    public TimeSpan TimeSpan { get; set; } = default;

    /// <summary>
    /// Initializes a new instance of the <see cref="PleasantSnackbarOptions"/> class with the specified message.
    /// </summary>
    /// <param name="message">The message to display in the snackbar.</param>
    public PleasantSnackbarOptions(string message)
    {
        Message = message;
    }
}
