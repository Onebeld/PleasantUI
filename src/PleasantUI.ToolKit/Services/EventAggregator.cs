using System.Collections.Concurrent;
using Avalonia.Logging;
using PleasantUI.ToolKit.Services.Interfaces;

namespace PleasantUI.ToolKit.Services;

public class EventAggregator : IEventAggregator
{
    private readonly ConcurrentDictionary<Type, List<Func<object, Task>>?> _handlers = new();
    
    public async Task PublishAsync<TEvent>(TEvent ev)
    {
        if (ev is null)
            return;
        
        if (_handlers.TryGetValue(typeof(TEvent), out List<Func<object, Task>>? list))
        {
            Func<object, Task>[]? handlers = list?.ToArray();

            if (handlers != null)
                foreach (Func<object, Task> handler in handlers)
                {
                    try
                    {
                        await handler(ev).ConfigureAwait(false);
                    }
                    catch
                    { 
                        Logger.Sink?.Log(LogEventLevel.Error, "EventAggregator", this, "Error during event call");
                    }
                }
        }
    }

    public IDisposable Subscribe<TEvent>(Func<TEvent, Task> handler)
    {
        List<Func<object, Task>>? handlers = _handlers.GetOrAdd(typeof(TEvent), _ => []);
        Func<object, Task> wrapper = (obj) => handler((TEvent)obj);

        if (handlers == null)
            throw new NullReferenceException("handlers is null");
        
        lock (handlers)
            handlers.Add(wrapper);

        return new Subscription(() =>
        {
            lock (handlers)
                handlers.Remove(wrapper);
        });

    }
    
    private sealed class Subscription(Action action) : IDisposable
    {
        private bool _isDisposed;

        public void Dispose()
        {
            if (_isDisposed)
                return;

            action();
            _isDisposed = true;
        }
    }
}