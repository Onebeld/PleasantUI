namespace PleasantUI.Core.Internal.Reactive;

internal class EmptyDisposable : IDisposable
{
    public static readonly EmptyDisposable Instance = new();
    
    private EmptyDisposable() { }
    
    public void Dispose() { }
}