using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PleasantUI.Controls;
using PleasantUI.Core.Interfaces;
using PleasantUI.Core.Localization;
using PleasantUI.Extensions;
using ColorPickerWindow = PleasantUI.ToolKit.ColorPickerWindow;

namespace PleasantUI.Core.Models;

/// <summary>
/// Represents a theme color with associated name and functionality for changing and copying the color.
/// </summary>
public class ThemeColor : ViewModelBase
{
    /// <summary>
    /// The parent window.
    /// </summary>
    internal static IPleasantWindow WindowParent;

    private Color _color;
    private string _name;

    /// <summary>
    /// Gets or sets the name of the color.
    /// </summary>
    public string Name
    {
        get => _name;
        set => RaiseAndSet(ref _name, value);
    }

    /// <summary>
    /// Gets or sets the color value.
    /// </summary>
    public Color Color
    {
        get => _color;
        set
        {
            RaiseAndSet(ref _color, value);

            RaisePropertyChanged(nameof(Brush));
        }
    }

    /// <summary>
    /// Gets a <see cref="SolidColorBrush" /> representation of the color.
    /// </summary>
    public SolidColorBrush Brush => new(Color);

    /// <summary>
    /// Event triggered when the color is changed.
    /// </summary>
    public event Action<ThemeColor, Color, Color>? ColorChanged;
    
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
    public async void ChangeColor()
    {
        Color? newColor = await ColorPickerWindow.SelectColor(WindowParent, Color.ToUInt32());

        if (newColor is null)
            return;

        Color previousColor = Color;

        ColorChanged?.Invoke(this, newColor.Value, previousColor);
    }

    /// <summary>
    /// Copies the color value to the clipboard.
    /// </summary>
    public async void CopyColor()
    {
        await TopLevel.GetTopLevel(WindowParent as Visual)?.Clipboard?.SetTextAsync(Color.ToString().ToUpper())!;

        Geometry? icon = ResourceExtensions.GetResource<Geometry>("CopyRegular");
        string text = Localizer.Instance["ColorCopiedToClipboard"];

        PleasantSnackbar.Show(WindowParent, text, icon: icon);
    }

    /// <summary>
    /// Pastes the color value from the clipboard.
    /// </summary>
    public async void PasteColor()
    {
        Color previousColor = Color;

        string? data = await TopLevel.GetTopLevel(WindowParent as Visual)?.Clipboard?.GetTextAsync()!;

        if (!Color.TryParse(data, out Color newColor))
            return;

        ColorChanged?.Invoke(this, newColor, previousColor);
    }
}