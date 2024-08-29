using Avalonia.Controls;
using Avalonia.Media;
using PleasantUI.Core.Models;

namespace PleasantUI.Windows.Commands.ThemeEditor;

public class ColorChangeCommand : IEditorCommand
{
	private readonly ResourceDictionary _resourceDictionary;
	private readonly ThemeColor _themeColor;
	
	private readonly Color _previousColor;
	private readonly Color _newColor;
	
	public ColorChangeCommand(ThemeColor themeColor, ResourceDictionary resourceDictionary, Color previousColor, Color newColor)
	{
		_themeColor = themeColor;
		_resourceDictionary = resourceDictionary;
		_previousColor = previousColor;
		_newColor = newColor;
	}
	
	public void Undo()
	{
		_themeColor.Color = _previousColor;
		
		ChangeColorInResourceDictionary(_themeColor);
	}

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