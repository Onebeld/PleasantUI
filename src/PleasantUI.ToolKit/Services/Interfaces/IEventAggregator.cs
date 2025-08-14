namespace PleasantUI.ToolKit.Services.Interfaces;

public interface IEventAggregator
{
    Task PublishAsync<TEvent>(TEvent ev);
    IDisposable Subscribe<TEvent>(Func<TEvent, Task> handler);
}