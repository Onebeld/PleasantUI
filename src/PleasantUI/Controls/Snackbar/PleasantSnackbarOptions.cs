using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media;

namespace PleasantUI.Controls;

/// <summary>
/// Represents options for configuring a <see cref="PleasantSnackbar"/> notification.
/// </summary>
public class PleasantSnackbarOptions
{
    /// <summary>
    /// Gets or sets the message displayed in the snackbar.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets an optional bold title shown above the message.
    /// When null the snackbar renders as a single-line notification (original behaviour).
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the text for an optional action button in the snackbar.
    /// Ignored when <see cref="ActionButton"/> is set.
    /// </summary>
    public string? ButtonText { get; set; }

    /// <summary>
    /// Gets or sets the action to be performed when the text button is clicked.
    /// Ignored when <see cref="ActionButton"/> is set.
    /// </summary>
    public Action? ButtonAction { get; set; }

    /// <summary>
    /// Gets or sets an arbitrary action control placed in the snackbar.
    /// When set, <see cref="ButtonText"/> and <see cref="ButtonAction"/> are ignored.
    /// </summary>
    public Control? ActionButton { get; set; }

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
    /// Gets or sets whether the user can dismiss the snackbar by clicking a close button.
    /// When true a close (×) button is shown on the right side.
    /// </summary>
    public bool IsClosable { get; set; } = false;

    /// <summary>
    /// Raised just before the snackbar closes.
    /// Set <see cref="SnackbarClosingEventArgs.Cancel"/> to true to prevent closing.
    /// </summary>
    public EventHandler<SnackbarClosingEventArgs>? Closing { get; set; }

    /// <summary>
    /// Raised after the snackbar has closed.
    /// </summary>
    public EventHandler<SnackbarClosedEventArgs>? Closed { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PleasantSnackbarOptions"/> class with the specified message.
    /// </summary>
    /// <param name="message">The message to display in the snackbar.</param>
    public PleasantSnackbarOptions(string message)
    {
        Message = message;
    }
}
