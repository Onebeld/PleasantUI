using PleasantUI.Core.Collections;

namespace PleasantUI.Controls;

/// <summary>
/// Manages a queue of PleasantSnackbars to be displayed on a PleasantWindow.
/// </summary>
/// <typeparam name="T">The type of PleasantSnackbar to be managed. Must be a subclass of PleasantSnackbar.</typeparam>
public class SnackbarQueueManager<T> : EventQueue<T> where T : PleasantSnackbar
{
    private readonly Lock _lock = new();
    private bool _isProcessingDequeued;

    /// <summary>
    /// Automatically displays the Snackbar if it is the only one in the queue.
    /// </summary>
    protected override void OnItemEnqueued(T item)
    {
        base.OnItemEnqueued(item);

        if (Count == 1)
            item.CreateHost();
    }

    /// <summary>
    /// Closes the current Snackbar and launches the next one from the queue (if there is one).
    /// </summary>
    protected override void OnItemDequeued(T item)
    {
        lock (_lock)
        {
            if (_isProcessingDequeued) return;
            _isProcessingDequeued = true;
        }

        try
        {
            base.OnItemDequeued(item);

            item.DeleteHost();

            if (Count > 0)
                Peek().CreateHost();
        }
        finally
        {
            lock (_lock)
            {
                _isProcessingDequeued = false;
            }
        }
    }
}