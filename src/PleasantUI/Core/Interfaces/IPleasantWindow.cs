using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PleasantUI.Controls;

namespace PleasantUI.Core.Interfaces;

/// <summary>
/// Represents a PleasantUI window with specific functionalities.
/// </summary>
public interface IPleasantWindow
{
    /// <summary>
    /// Gets the snackbar queue manager for pleasant snackbars.
    /// </summary>
    SnackbarQueueManager<PleasantSnackbar> SnackbarQueueManager { get; }

    /// <summary>
    /// Gets the collection of modal windows associated with this window.
    /// </summary>
    AvaloniaList<PleasantModalWindow> ModalWindows { get; }

    /// <summary>
    /// Gets the collection of controls associated with this window.
    /// </summary>
    AvaloniaList<Control> Controls { get; }

    /// <summary>
    /// Gets the visual layer manager for this window.
    /// </summary>
    VisualLayerManager VisualLayerManager { get; }

    /// <summary>
    /// Adds a modal window to the window.
    /// </summary>
    /// <param name="modalWindow">The modal window to add.</param>
    internal void AddModalWindow(PleasantModalWindow modalWindow);

    /// <summary>
    /// Removes a modal window from the window.
    /// </summary>
    /// <param name="modalWindow">The modal window to remove.</param>
    internal void RemoveModalWindow(PleasantModalWindow modalWindow);

    /// <summary>
    /// Adds a control to the window.
    /// </summary>
    /// <param name="control">The control to add.</param>
    internal void AddControl(Control control);

    /// <summary>
    /// Removes a control from the window.
    /// </summary>
    /// <param name="control">The control to remove.</param>
    internal void RemoveControl(Control control);
}