namespace PleasantUI.Core.Internal.Reactive;

internal class EmptyDisposable : IDisposable
{
    public static EmptyDisposable Instance { get; } = new();

    public void Dispose() { }
}