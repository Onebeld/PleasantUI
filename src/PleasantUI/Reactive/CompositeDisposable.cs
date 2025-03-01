using System.Collections;

namespace PleasantUI.Reactive;

internal sealed class CompositeDisposable : ICollection<IDisposable>, IDisposable
{
    private const int ShrinkThreshold = 64;

    /// <summary>
    /// An empty enumerator for the <see cref="GetEnumerator" />
    /// method to avoid allocation on disposed or empty composites.
    /// </summary>
    private static readonly CompositeEnumerator EmptyEnumerator = new(Array.Empty<IDisposable?>());

    private readonly object _gate = new();
    private int _count;
    private List<IDisposable?> _disposables;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeDisposable" /> class with the specified number of
    /// disposables.
    /// </summary>
    /// <param name="capacity">The number of disposables that the new CompositeDisposable can initially store.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity" /> is less than zero.</exception>
    public CompositeDisposable(int capacity)
    {
        if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));

        _disposables = new List<IDisposable?>(capacity);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeDisposable"/> class
    /// from a group of disposables.
    /// </summary>
    /// <param name="disposables">Disposables that will be disposed together.</param>
    /// <exception cref="ArgumentNullException"><paramref name="disposables"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Any of the disposables in the <paramref name="disposables"/> collection is <c>null</c>.
    /// </exception>
    public CompositeDisposable(params IDisposable[] disposables)
    {
        if (disposables == null) throw new ArgumentNullException(nameof(disposables));

        _disposables = ToList(disposables);
        Volatile.Write(ref _count, _disposables.Count);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeDisposable"/> class
    /// from a group of disposables.
    /// </summary>
    /// <param name="disposables">Disposables that will be disposed together.</param>
    /// <exception cref="ArgumentNullException"><paramref name="disposables"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Any of the disposables in the <paramref name="disposables"/> collection is <c>null</c>.
    /// </exception>
    public CompositeDisposable(IList<IDisposable> disposables)
    {
        if (disposables == null) throw new ArgumentNullException(nameof(disposables));

        _disposables = ToList(disposables);
        Volatile.Write(ref _count, _disposables.Count);
    }

    /// <summary>
    /// Gets a value that indicates whether the object is disposed.
    /// </summary>
    public bool IsDisposed => Volatile.Read(ref _disposed);

    /// <summary>
    /// Gets the number of disposables contained in the <see cref="CompositeDisposable" />.
    /// </summary>
    public int Count => Volatile.Read(ref _count);

    /// <summary>
    /// Adds a disposable to the <see cref="CompositeDisposable" /> or disposes the disposable if the
    /// <see cref="CompositeDisposable" /> is disposed.
    /// </summary>
    /// <param name="item">Disposable to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="item" /> is <c>null</c>.</exception>
    public void Add(IDisposable item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        lock (_gate)
        {
            if (!_disposed)
            {
                _disposables.Add(item);

                // If read atomically outside the lock, it should be written atomically inside
                // the plain read on _count is fine here because manipulation always happens
                // from inside a lock.
                Volatile.Write(ref _count, _count + 1);
                return;
            }
        }

        item.Dispose();
    }

    /// <summary>
    /// Removes and disposes the first occurrence of a disposable from the <see cref="CompositeDisposable" />.
    /// </summary>
    /// <param name="item">Disposable to remove.</param>
    /// <returns>true if found; false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="item" /> is <c>null</c>.</exception>
    public bool Remove(IDisposable item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        lock (_gate)
        {
            // If this composite is already disposed and if the item was in there,
            // it has been already removed/disposed.
            if (_disposed) return false;

            // We use a null sentinel value instead of removing the element from the list,
            // to avoid the cost of Array.Copy operations performed by Remove.
            List<IDisposable?> current = _disposables;

            int i = current.IndexOf(item);
            if (i < 0)
                return false;

            current[i] = null;

            if (current.Capacity > ShrinkThreshold && _count < current.Capacity / 2)
            {
                List<IDisposable?> fresh = new(current.Capacity / 2);
                foreach (IDisposable? d in current)
                {
                    if (d != null)
                        fresh.Add(d);
                }
                _disposables = fresh;
            }

            // Atomically update the count.
            Volatile.Write(ref _count, _count - 1);
        }

        // Dispose the item and return success.
        item.Dispose();
        return true;
    }

    /// <summary>
    /// Removes and disposes all disposables from the CompositeDisposable, without disposing the CompositeDisposable itself.
    /// </summary>
    public void Clear()
    {
        IDisposable?[] previousDisposables;

        lock (_gate)
        {
            if (_disposed) return;

            List<IDisposable?> current = _disposables;
            previousDisposables = current.ToArray();
            current.Clear();

            Volatile.Write(ref _count, 0);
        }

        foreach (IDisposable? d in previousDisposables)
        {
            d?.Dispose();
        }
    }

    /// <summary>
    /// Determines whether the <see cref="CompositeDisposable" /> contains a specific disposable.
    /// </summary>
    /// <param name="item">Disposable to search for.</param>
    /// <returns>true if the disposable was found; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="item" /> is <c>null</c>.</exception>
    public bool Contains(IDisposable item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        lock (_gate)
        {
            if (_disposed) return false;

            return _disposables.Contains(item);
        }
    }

    /// <summary>
    /// Copies the disposables contained in the <see cref="CompositeDisposable" /> to an array, starting at a particular
    /// array index.
    /// </summary>
    /// <param name="array">Array to copy the contained disposables to.</param>
    /// <param name="arrayIndex">Target index at which to copy the first disposable of the group.</param>
    /// <exception cref="ArgumentNullException"><paramref name="array" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="arrayIndex" /> is less than zero. -or -
    /// <paramref name="arrayIndex" /> is larger than or equal to the array length.
    /// </exception>
    public void CopyTo(IDisposable[] array, int arrayIndex)
    {
        if (array == null) throw new ArgumentNullException(nameof(array));

        if (arrayIndex < 0 || arrayIndex >= array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

        lock (_gate)
        {
            // disposed composites are always empty
            if (_disposed) return;

            if (arrayIndex + _count > array.Length)
                // there is not enough space beyond arrayIndex 
                // to accommodate all _count disposables in this composite
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            int i = arrayIndex;

            foreach (IDisposable? d in _disposables)
                if (d != null)
                    array[i++] = d;
        }
    }

    /// <summary>
    /// Always returns false.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="CompositeDisposable" />.
    /// </summary>
    /// <returns>An enumerator to iterate over the disposables.</returns>
    public IEnumerator<IDisposable> GetEnumerator()
    {
        lock (_gate)
        {
            if (_disposed || _count == 0) return EmptyEnumerator;

            // the copy is unavoidable but the creation
            // of an outer IEnumerable is avoidable
            return new CompositeEnumerator(_disposables.ToArray());
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="CompositeDisposable" />.
    /// </summary>
    /// <returns>An enumerator to iterate over the disposables.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Disposes all disposables in the group and removes them from the group.
    /// </summary>
    public void Dispose()
    {
        List<IDisposable?>? currentDisposables = null;

        lock (_gate)
        {
            if (!_disposed)
            {
                currentDisposables = _disposables;

                // nulling out the reference is faster no risk to
                // future Add/Remove because _disposed will be true
                // and thus _disposables won't be touched again.
                _disposables = null!; // NB: All accesses are guarded by _disposed checks.

                Volatile.Write(ref _count, 0);
                Volatile.Write(ref _disposed, true);
            }
        }

        if (currentDisposables != null)
            foreach (IDisposable? d in currentDisposables)
                d?.Dispose();
    }

    private static List<IDisposable?> ToList(IEnumerable<IDisposable> disposables)
    {
        int capacity = disposables switch
        {
            IDisposable[] a => a.Length,
            ICollection<IDisposable> c => c.Count,
            _ => 12
        };

        List<IDisposable?> list = new(capacity);
        foreach (IDisposable d in disposables)
        {
            if (d == null)
                throw new ArgumentException("Disposables can't contain null", nameof(disposables));
            list.Add(d);
        }
        return list;
    }

    /// <summary>
    /// An enumerator for an array of disposables.
    /// </summary>
    private sealed class CompositeEnumerator : IEnumerator<IDisposable>
    {
        private readonly IDisposable?[] _disposables;
        private int _index;

        public CompositeEnumerator(IDisposable?[] disposables)
        {
            _disposables = disposables;
            _index = -1;
        }

        public IDisposable Current => _disposables[_index]!; // _index is advanced to non-null positions.

        object IEnumerator.Current => _disposables[_index]!;

        public void Dispose()
        {
            // Clear the array to avoid retaining references.
            Array.Clear(_disposables, 0, _disposables.Length);
        }

        public bool MoveNext()
        {
            for (; ; )
            {
                _index++;

                if (_index >= _disposables.Length)
                    return false;

                // Skip null entries.
                if (_disposables[_index] != null)
                    return true;
            }
        }

        public void Reset() => _index = -1;
    }
}