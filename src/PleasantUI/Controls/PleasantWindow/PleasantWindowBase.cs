using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PleasantUI.Core.Interfaces;

namespace PleasantUI.Controls;

public abstract class PleasantWindowBase : Window, IPleasantWindow
{
    /// <summary>
    /// Gets the snackbar queue manager for pleasant snackbars.
    /// </summary>
    public SnackbarQueueManager<PleasantSnackbar> SnackbarQueueManager { get; set; }

    /// <summary>
    /// Gets the collection of modal windows associated with this window.
    /// </summary>
    public AvaloniaList<PleasantModalWindow> ModalWindows { get; } = [];
}