namespace PleasantUI.Core.Internal.Reactive;

internal sealed class AnonymousDisposable(Action dispose) : IDisposable
{
    private volatile Action? _dispose = dispose;
    public bool IsDisposed => _dispose == null;
    public void Dispose()
    {
        Interlocked.Exchange(ref _dispose, null)?.Invoke();
    }
}