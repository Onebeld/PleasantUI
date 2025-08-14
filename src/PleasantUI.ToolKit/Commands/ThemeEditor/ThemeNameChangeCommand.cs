using PleasantUI.ToolKit.Services.Interfaces;

namespace PleasantUI.ToolKit.Commands.ThemeEditor;

/// <summary>
/// Represents a command that changes the name of the theme.
/// </summary>
/// <seealso cref="IEditorCommand" />
public class ThemeNameChangeCommand : IEditorCommand
{
    private readonly IThemeService _themeService;
        
    private readonly string _newName;

    private readonly string _previousName;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeNameChangeCommand" /> class.
    /// </summary>
    /// <param name="viewModel">The view model.</param>
    /// <param name="previousName">The previous name of the theme.</param>
    /// <param name="newName">The new name of the theme.</param>
    public ThemeNameChangeCommand(IThemeService themeService, string previousName, string newName)
    {
        _themeService = themeService;
        _previousName = previousName;
        _newName = newName;
    }

    /// <inheritdoc />
    public void Undo()
    {
        _themeService.ChangeThemeName(_previousName);
    }

    /// <inheritdoc />
    public void Redo()
    {
        _themeService.ChangeThemeName(_newName);
    }
}