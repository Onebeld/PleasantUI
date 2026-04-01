namespace PleasantUI.Controls;

/// <summary>
/// Defines the reason a <see cref="PleasantSnackbar"/> is closing.
/// </summary>
public enum SnackbarCloseReason
{
    /// <summary>The snackbar timed out.</summary>
    Timeout,
    /// <summary>The user clicked the close button.</summary>
    CloseButton,
    /// <summary>The user clicked anywhere on the snackbar.</summary>
    UserDismiss,
    /// <summary>Closed programmatically.</summary>
    Programmatic
}

/// <summary>
/// Event args for <see cref="PleasantSnackbarOptions.Closing"/>.
/// Set <see cref="Cancel"/> to true to prevent the snackbar from closing.
/// </summary>
public sealed class SnackbarClosingEventArgs : EventArgs
{
    public SnackbarClosingEventArgs(SnackbarCloseReason reason) => Reason = reason;

    /// <summary>The reason the snackbar is closing.</summary>
    public SnackbarCloseReason Reason { get; }

    /// <summary>Set to true to cancel the close.</summary>
    public bool Cancel { get; set; }
}

/// <summary>
/// Event args for <see cref="PleasantSnackbarOptions.Closed"/>.
/// </summary>
public sealed class SnackbarClosedEventArgs : EventArgs
{
    public SnackbarClosedEventArgs(SnackbarCloseReason reason) => Reason = reason;

    /// <summary>The reason the snackbar closed.</summary>
    public SnackbarCloseReason Reason { get; }
}
