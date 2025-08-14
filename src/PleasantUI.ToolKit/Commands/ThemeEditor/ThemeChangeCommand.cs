using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using PleasantUI.ToolKit.Models;
using PleasantUI.ToolKit.Services.Interfaces;

namespace PleasantUI.ToolKit.Commands.ThemeEditor;

/// <summary>
/// Represents a command that changes the colors of the theme.
/// </summary>
public class ThemeChangeCommand : IEditorCommand
{
    private readonly Dictionary<string, Color> _newColors;
    private readonly string? _newName;

    private readonly Dictionary<string, Color> _previousColors;

    private readonly string _previousName;

    private readonly IThemeService _themeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeChangeCommand" /> class.
    /// </summary>
    /// <param name="viewModel">The view model.</param>
    /// <param name="previousColors">The previous colors.</param>
    /// <param name="newColors">The new colors.</param>
    /// <param name="previousName">The previous name of the theme.</param>
    /// <param name="newName">The new name of the theme (optional).</param>
    public ThemeChangeCommand(IThemeService themeService, Dictionary<string, Color> previousColors,
        Dictionary<string, Color> newColors, string previousName, string? newName)
    {
        _themeService = themeService;
        _previousColors = previousColors;
        _newColors = newColors;
        _previousName = previousName;
        _newName = newName;
    }

    /// <inheritdoc />
    public void Undo()
    {
        AvaloniaList<ThemeColor> themeColors = _themeService.ThemeColors;

        foreach (KeyValuePair<string, Color> pair in _previousColors)
            foreach (ThemeColor themeColor in themeColors)
            {
                if (themeColor.Name != pair.Key) continue;

                themeColor.Color = pair.Value;
                break;
            }

        if (!string.IsNullOrWhiteSpace(_newName))
            _themeService.ChangeThemeName(_previousName);

        ChangeColorsInResourceDictionary(themeColors);
    }

    /// <inheritdoc />
    public void Redo()
    {
        AvaloniaList<ThemeColor> themeColors = _themeService.ThemeColors;

        foreach (KeyValuePair<string, Color> pair in _newColors)
            foreach (ThemeColor themeColor in themeColors.Where(themeColor => themeColor.Name == pair.Key))
            {
                themeColor.Color = pair.Value;
                break;
            }

        if (!string.IsNullOrWhiteSpace(_newName))
            _themeService.ChangeThemeName(_newName);


        ChangeColorsInResourceDictionary(themeColors);
    }

    private void ChangeColorsInResourceDictionary(AvaloniaList<ThemeColor> themeColors)
    {
        ResourceDictionary resourceDictionary = _themeService.ResourceDictionary;

        foreach (ThemeColor themeColor in themeColors)
            if (resourceDictionary.ContainsKey(themeColor.Name))
                resourceDictionary[themeColor.Name] = themeColor.Color;
    }
}