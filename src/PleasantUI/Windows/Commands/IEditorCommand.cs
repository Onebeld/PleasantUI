namespace PleasantUI.Windows.Commands;

public interface IEditorCommand
{
	void Undo();
	void Redo();
}