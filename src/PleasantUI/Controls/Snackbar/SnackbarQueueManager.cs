using PleasantUI.Core.Collections;
using PleasantUI.Core.Interfaces;

namespace PleasantUI.Controls;

/// <summary>
/// Manages a queue of PleasantSnackbars to be displayed on a PleasantWindow.
/// </summary>
/// <typeparam name="T">The type of PleasantSnackbar to be managed. Must be a subclass of PleasantSnackbar.</typeparam>
public class SnackbarQueueManager<T> : EventQueue<T> where T : PleasantSnackbar
{
    private readonly IPleasantWindow _parent;

    /// <summary>
    /// Initializes a new instance of the SnackbarQueueManager class.
    /// </summary>
    /// <param name="parent">The PleasantWindow that will host the snackbars.</param>
    public SnackbarQueueManager(IPleasantWindow parent)
    {
        _parent = parent;
    }

    /// <summary>
    /// Called when an item is dequeued from the queue.
    /// Removes the dequeued snackbar from the parent window and adds the next snackbar in the queue (if any).
    /// </summary>
    /// <param name="item">The snackbar that was dequeued.</param>
    protected override void OnItemDequeued(T item)
    {
        base.OnItemDequeued(item);

        /*_parent.RemoveControl(item);

        if (Count > 0)
            _parent.AddControl(Peek());*/
    }
}