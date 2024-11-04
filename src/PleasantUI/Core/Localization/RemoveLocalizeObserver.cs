namespace PleasantUI.Core.Localization;

/// <summary>
/// Represents an observer that can be removed from a <see cref="LocalizeObservable"/>.
/// </summary>
public class RemoveLocalizeObserver : IDisposable
{
    private LocalizeObservable? _parent;
    
    private IObserver<string>? _observer;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveLocalizeObserver"/> class.
    /// </summary>
    /// <param name="parent">The <see cref="LocalizeObservable"/> instance that manages the observer.</param>
    /// <param name="observer">The observer to be removed when disposing.</param>
    public RemoveLocalizeObserver(LocalizeObservable parent, IObserver<string> observer)
    {
        _parent = parent;
        Volatile.Write(ref _observer, observer);
    }
    
    /// <inheritdoc/>
    public void Dispose()
    {
        IObserver<string>? observer = _observer;
        Interlocked.Exchange(ref _parent, null)?.Remove(observer!);
        _observer = null;
    }
}