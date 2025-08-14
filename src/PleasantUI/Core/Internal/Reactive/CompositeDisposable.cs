using System.Collections;

namespace PleasantUI.Core.Internal.Reactive;

internal class CompositeDisposable : ICollection<IDisposable>, IDisposable
{
    private readonly List<IDisposable> _disposables;
    private bool _disposed;
    
    public int Count { get { lock (_disposables) { return _disposables.Count; } } }
    
    public bool IsReadOnly => false;
    
    public bool IsDisposed => _disposed;
    
    public CompositeDisposable() : this([]) { }

    public CompositeDisposable(IEnumerable<IDisposable>? disposables)
    {
        if (disposables is null)
            throw new ArgumentNullException(nameof(disposables));
        _disposables = new List<IDisposable>(disposables);
    }
    
    public IEnumerator<IDisposable> GetEnumerator()
    {
        lock (_disposables)
        {
            return _disposables.ToArray().AsEnumerable().GetEnumerator();
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(IDisposable? item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        lock (_disposables)
        {
            if (_disposed)
            {
                item.Dispose();
            }
            else
            {
                _disposables.Add(item);
            }
        }
    }

    public void Clear()
    {
        lock (_disposables)
        {
            IDisposable[] toDispose = _disposables.ToArray();
            _disposables.Clear();
            foreach (IDisposable d in toDispose) d.Dispose();
        }
    }

    public bool Contains(IDisposable item)
    {
        return false;
    }

    public void CopyTo(IDisposable[] array, int arrayIndex)
    {
    }

    public bool Remove(IDisposable? item)
    {
        if (item == null) return false;
        lock (_disposables)
        {
            bool removed = _disposables.Remove(item);
            if (removed)
                item.Dispose();
            return removed;
        }
    }
    
    public void Dispose()
    {
        lock (_disposables)
        {
            if (_disposed) return;
            
            Clear();
            _disposed = true;
        }
    }
}