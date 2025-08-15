using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using PleasantUI.ToolKit.Models;
using PleasantUI.ToolKit.Services.Interfaces;

namespace PleasantUI.ToolKit.Commands.ThemeEditor;

internal class ThemeChangeCommand(
    IThemeService themeService,
    Dictionary<string, Color> previousColors,
    Dictionary<string, Color> newColors,
    string previousName,
    string? newName)
    : IEditorCommand
{
    public void Undo()
    {
        AvaloniaList<ThemeColor> themeColors = themeService.ThemeColors;

        foreach (KeyValuePair<string, Color> pair in previousColors)
            foreach (ThemeColor themeColor in themeColors)
            {
                if (themeColor.Name != pair.Key) continue;

                themeColor.Color = pair.Value;
                break;
            }

        if (!string.IsNullOrWhiteSpace(newName))
            themeService.ChangeThemeName(previousName);

        ChangeColorsInResourceDictionary(themeColors);
    }

    public void Redo()
    {
        AvaloniaList<ThemeColor> themeColors = themeService.ThemeColors;

        foreach (KeyValuePair<string, Color> pair in newColors)
            foreach (ThemeColor themeColor in themeColors.Where(themeColor => themeColor.Name == pair.Key))
            {
                themeColor.Color = pair.Value;
                break;
            }

        if (!string.IsNullOrWhiteSpace(newName))
            themeService.ChangeThemeName(newName);


        ChangeColorsInResourceDictionary(themeColors);
    }

    private void ChangeColorsInResourceDictionary(AvaloniaList<ThemeColor> themeColors)
    {
        ResourceDictionary resourceDictionary = themeService.ResourceDictionary;

        foreach (ThemeColor themeColor in themeColors)
            if (resourceDictionary.ContainsKey(themeColor.Name))
                resourceDictionary[themeColor.Name] = themeColor.Color;
    }
}