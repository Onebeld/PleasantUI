using Avalonia.Collections;
using Avalonia.Controls;
using PleasantUI.Core.Interfaces;

namespace PleasantUI.Controls;

/// <summary>
/// Represents the base class for PleasantUI windows, providing support for modal windows 
/// and a snackbar queue manager.
/// </summary>
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