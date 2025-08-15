namespace PleasantUI.ToolKit.Commands;

internal interface IEditorCommand
{
    void Undo();

    void Redo();
}