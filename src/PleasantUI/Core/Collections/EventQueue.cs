using System.Collections;

namespace PleasantUI.Core.Collections;

/// <summary>
/// Represents a queue that raises an event when an item is dequeued.
/// </summary>
/// <typeparam name="T">The type of elements in the queue.</typeparam>
public class EventQueue<T> : IReadOnlyCollection<T>
{
    private readonly Queue<T> _internalQueue = new();

    /// <summary>
    /// Вызывается после того, как элемент был добавлен в очередь.
    /// </summary>
    public event EventHandler<T>? Enqueued;

    /// <summary>
    /// Вызывается после того, как элемент был извлечен из очереди.
    /// </summary>
    public event EventHandler<T>? Dequeued;

    public int Count => _internalQueue.Count;

    public void Enqueue(T item)
    {
        _internalQueue.Enqueue(item);
        OnItemEnqueued(item);
    }

    public T Dequeue()
    {
        T item = _internalQueue.Dequeue();
        OnItemDequeued(item);
        return item;
    }

    public T Peek() => _internalQueue.Peek();

    protected virtual void OnItemEnqueued(T item)
    {
        Enqueued?.Invoke(this, item);
    }

    protected virtual void OnItemDequeued(T item)
    {
        Dequeued?.Invoke(this, item);
    }

    public IEnumerator<T> GetEnumerator() => _internalQueue.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}