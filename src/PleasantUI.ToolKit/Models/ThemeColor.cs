using Avalonia.Media;
using PleasantUI.Core;
using PleasantUI.ToolKit.Messages;
using PleasantUI.ToolKit.Services.Interfaces;

namespace PleasantUI.ToolKit.Models;

internal class ThemeColor : ViewModelBase
{
    private readonly IEventAggregator _eventAggregator;
        
    private Color _color;
    private string _name;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public Color Color
    {
        get => _color;
        set
        {
            SetProperty(ref _color, value);

            RaisePropertyChanged(nameof(Brush));
        }
    }

    public SolidColorBrush Brush => new(Color);

    public ThemeColor(string name, Color color, IEventAggregator eventAggregator)
    {
        _name = name;
        _color = color;

        _eventAggregator = eventAggregator;
    }

    public async Task ChangeColorAsync()
    {
        TaskCompletionSource<Color?> taskCompletionSource = new();

        await _eventAggregator.PublishAsync(new ChangeColorMessage(Color, taskCompletionSource));

        Color? newColor = await taskCompletionSource.Task.ConfigureAwait(false);
            
        if (newColor is null)
            return;

        await _eventAggregator.PublishAsync(new ChangedColorMessage(this, newColor.Value, Color));
    }

    public async Task CopyColorAsync()
    {
        await _eventAggregator.PublishAsync(new ColorCopyMessage(Color));
    }

    public async Task PasteColorAsync()
    {
        TaskCompletionSource<Color?> taskCompletionSource = new();

        await _eventAggregator.PublishAsync(new PasteColorMessage(taskCompletionSource));

        Color? newColor = await taskCompletionSource.Task.ConfigureAwait(false);
        
        if (newColor is null)
            return;

        await _eventAggregator.PublishAsync(new ChangedColorMessage(this, newColor.Value, Color));
    }
}