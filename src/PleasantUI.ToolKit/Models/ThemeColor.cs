using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using PleasantUI.ToolKit.Messages;

namespace PleasantUI.ToolKit.Models;

/// <summary>
/// Represents a theme color with associated name and functionality for changing and copying the color.
/// </summary>
public class ThemeColor : ObservableObject
{
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

            OnPropertyChanged(nameof(Brush));
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
    public ThemeColor(string name, Color color)
    {
        Name = name;
        Color = color;
    }

    /// <summary>
    /// Opens a color picker dialog to change the color.
    /// </summary>
    public async Task ChangeColorAsync()
    {
        Color? newColor = await WeakReferenceMessenger.Default.Send(new AsyncRequestColorMessage(Color));

        if (newColor is null)
            return;

        Color previousColor = Color;

        WeakReferenceMessenger.Default.Send(new ColorChangedMessage(newColor.Value, this, previousColor));
    }

    /// <summary>
    /// Copies the color value to the clipboard.
    /// </summary>
    public async Task CopyColorAsync()
    {
        await WeakReferenceMessenger.Default.Send(new AsyncClipboardSetColorMessage(Color));
    }

    /// <summary>
    /// Pastes the color value from the clipboard.
    /// </summary>
    public async Task PasteColorAsync()
    {
        Color? newColor = await WeakReferenceMessenger.Default.Send(new AsyncRequestClipboardColorMessage());
        
        if (newColor is null)
            return;

        Color previousColor = Color;
        
        WeakReferenceMessenger.Default.Send(new ColorChangedMessage(newColor.Value, this, previousColor));
    }
}