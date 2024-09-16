namespace PleasantUI.ToolKit.Commands;

/// <summary>
/// Represents a command that can be executed and undone/redone in an editor context.
/// </summary>
public interface IEditorCommand
{
    /// <summary>
    /// Undoes the command.
    /// </summary>
    void Undo();

    /// <summary>
    /// Redoes the command.
    /// </summary>
    void Redo();
}