using Avalonia.Controls;
using Avalonia.Media;
using PleasantUI.ToolKit.Models;

namespace PleasantUI.ToolKit.Commands.ThemeEditor;

internal class ColorChangeCommand(
    ThemeColor themeColor,
    ResourceDictionary resourceDictionary,
    Color previousColor,
    Color newColor)
    : IEditorCommand
{
    public void Undo()
    {
        themeColor.Color = previousColor;

        ChangeColorInResourceDictionary(themeColor);
    }

    public void Redo()
    {
        themeColor.Color = newColor;

        ChangeColorInResourceDictionary(themeColor);
    }

    private void ChangeColorInResourceDictionary(ThemeColor themeColorForChange)
    {
        resourceDictionary[themeColorForChange.Name] = themeColorForChange.Color;
    }
}