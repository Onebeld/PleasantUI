namespace PleasantUI.Core.Collections;

/// <summary>
/// Represents a queue that raises an event when an item is dequeued.
/// </summary>
/// <typeparam name="T">The type of elements in the queue.</typeparam>
public class EventQueue<T> : Queue<T>
{
    /// <summary>
    /// Occurs when an item is dequeued from the queue.
    /// </summary>
    public event EventHandler<T>? Dequeued;
    
    /// <summary>
    /// Removes and returns the object at the beginning of the queue.
    /// Raises the <see cref="Dequeued"/> event after dequeuing the item.
    /// </summary>
    /// <returns>The object that is removed from the beginning of the queue.</returns>
    public new T Dequeue()
    {
        T item = base.Dequeue();
        OnItemDequeued(item);

        return item;
    }

    /// <summary>
    /// Raises the <see cref="Dequeued"/> event.
    /// </summary>
    /// <param name="item">The item that was dequeued.</param>
    protected virtual void OnItemDequeued(T item)
    {
        Dequeued?.Invoke(this, item);
    }
}