using PleasantUI.ToolKit.ViewModels;

namespace PleasantUI.ToolKit.Commands.ThemeEditor;

public class ThemeNameChangeCommand : IEditorCommand
{
	private readonly ThemeEditorViewModel _viewModel;

	private readonly string _previousName;
	private readonly string _newName;

	public ThemeNameChangeCommand(ThemeEditorViewModel viewModel, string previousName, string newName)
	{
		_viewModel = viewModel;
		_previousName = previousName;
		_newName = newName;
	}
	
	public void Undo()
	{
		_viewModel.ChangeThemeName(_previousName);
	}

	public void Redo()
	{
		_viewModel.ChangeThemeName(_newName);
	}
}