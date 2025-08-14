using Avalonia.Media;
using PleasantUI.Core;
using PleasantUI.ToolKit.Messages;
using PleasantUI.ToolKit.Services.Interfaces;

namespace PleasantUI.ToolKit.Models;

/// <summary>
/// Represents a theme color with associated name and functionality for changing and copying the color.
/// </summary>
public class ThemeColor : ViewModelBase
{
    private readonly IEventAggregator _eventAggregator;
        
    private Color _color;
    private string _name;

    /// <summary>
    /// Gets or sets the name of the color.
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    /// <summary>
    /// Gets or sets the color value.
    /// </summary>
    public Color Color
    {
        get => _color;
        set
        {
            SetProperty(ref _color, value);

            RaisePropertyChanged(nameof(Brush));
        }
    }

    /// <summary>
    /// Gets a <see cref="Avalonia.Media.SolidColorBrush" /> representation of the color.
    /// </summary>
    public SolidColorBrush Brush => new(Color);

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeColor" /> class.
    /// </summary>
    /// <param name="name">The name of the color.</param>
    /// <param name="color">The color value.</param>
    public ThemeColor(string name, Color color, IEventAggregator eventAggregator)
    {
        _name = name;
        _color = color;

        _eventAggregator = eventAggregator;
    }

    /// <summary>
    /// Opens a color picker dialog to change the color.
    /// </summary>
    public async Task ChangeColorAsync()
    {
        TaskCompletionSource<Color?> taskCompletionSource = new();

        await _eventAggregator.PublishAsync(new ChangeColorMessage(Color, taskCompletionSource));

        Color? newColor = await taskCompletionSource.Task.ConfigureAwait(false);
            
        if (newColor is null)
            return;

        await _eventAggregator.PublishAsync(new ChangedColorMessage(this, newColor.Value, Color));
    }

    /// <summary>
    /// Copies the color value to the clipboard.
    /// </summary>
    public async Task CopyColorAsync()
    {
        await _eventAggregator.PublishAsync(new ColorCopyMessage(Color));
    }

    /// <summary>
    /// Pastes the color value from the clipboard.
    /// </summary>
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