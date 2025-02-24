using PleasantUI.Core.Collections;

namespace PleasantUI.Controls;

/// <summary>
/// Manages a queue of PleasantSnackbars to be displayed on a PleasantWindow.
/// </summary>
/// <typeparam name="T">The type of PleasantSnackbar to be managed. Must be a subclass of PleasantSnackbar.</typeparam>
public class SnackbarQueueManager<T> : EventQueue<T> where T : PleasantSnackbar
{
    /// <summary>
    /// Called when an item is dequeued from the queue.
    /// Removes the dequeued snackbar from the parent window and adds the next snackbar in the queue (if any).
    /// </summary>
    /// <param name="item">The snackbar that was dequeued.</param>
    protected override void OnItemDequeued(T item)
    {
        base.OnItemDequeued(item);

        item.DeleteHost();

        if (Count > 0)
            Peek().CreateHost();
    }
}