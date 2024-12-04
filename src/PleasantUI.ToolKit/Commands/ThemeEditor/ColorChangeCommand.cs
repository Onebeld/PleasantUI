using Avalonia.Controls;
using Avalonia.Media;
using PleasantUI.ToolKit.Models;

namespace PleasantUI.ToolKit.Commands.ThemeEditor;

/// <summary>
/// Represents a command that changes the color of a <see cref="ThemeColor" />.
/// </summary>
public class ColorChangeCommand : IEditorCommand
{
    private readonly Color _newColor;

    private readonly Color _previousColor;
    private readonly ResourceDictionary _resourceDictionary;
    private readonly ThemeColor _themeColor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorChangeCommand" /> class.
    /// </summary>
    /// <param name="themeColor">The theme color that was changed.</param>
    /// <param name="resourceDictionary">The resource dictionary that contains the theme color.</param>
    /// <param name="previousColor">The previous color of the theme color.</param>
    /// <param name="newColor">The new color of the theme color.</param>
    public ColorChangeCommand(ThemeColor themeColor, ResourceDictionary resourceDictionary, Color previousColor,
        Color newColor)
    {
        _themeColor = themeColor;
        _resourceDictionary = resourceDictionary;
        _previousColor = previousColor;
        _newColor = newColor;
    }

    /// <inheritdoc />
    public void Undo()
    {
        _themeColor.Color = _previousColor;

        ChangeColorInResourceDictionary(_themeColor);
    }

    /// <inheritdoc />
    public void Redo()
    {
        _themeColor.Color = _newColor;

        ChangeColorInResourceDictionary(_themeColor);
    }

    private void ChangeColorInResourceDictionary(ThemeColor themeColor)
    {
        _resourceDictionary[themeColor.Name] = themeColor.Color;
    }
}