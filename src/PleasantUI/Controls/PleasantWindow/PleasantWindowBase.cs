using Avalonia.Collections;
using Avalonia.Controls;
using PleasantUI.Core.Interfaces;

namespace PleasantUI.Controls;

public abstract class PleasantWindowBase : Window, IPleasantWindow
{
    /// <summary>
    /// Gets the snackbar queue manager for pleasant snackbars.
    /// </summary>
    public SnackbarQueueManager<PleasantSnackbar> SnackbarQueueManager { get; } = new();

    /// <summary>
    /// Gets the collection of modal windows associated with this window.
    /// </summary>
    public AvaloniaList<PleasantPopupElement> ModalWindows { get; } = [];
}