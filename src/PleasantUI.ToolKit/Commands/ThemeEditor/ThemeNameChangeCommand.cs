using PleasantUI.ToolKit.Services.Interfaces;

namespace PleasantUI.ToolKit.Commands.ThemeEditor;

internal class ThemeNameChangeCommand(IThemeService themeService, string previousName, string newName)
    : IEditorCommand
{
    public void Undo()
    {
        themeService.ChangeThemeName(previousName);
    }

    public void Redo()
    {
        themeService.ChangeThemeName(newName);
    }
}