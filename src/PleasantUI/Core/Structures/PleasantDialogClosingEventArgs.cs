namespace PleasantUI.Core.Structures;

/// <summary>
/// Event args for the <see cref="PleasantUI.ToolKit.PleasantDialog.Closing"/> event.
/// Set <see cref="Cancel"/> to true to prevent the dialog from closing.
/// </summary>
public sealed class PleasantDialogClosingEventArgs : EventArgs
{
    public PleasantDialogClosingEventArgs(object result) => Result = result;

    /// <summary>The result that would be returned if the dialog closes.</summary>
    public object Result { get; }

    /// <summary>Set to true to cancel the close.</summary>
    public bool Cancel { get; set; }
}
