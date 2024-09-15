using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using PleasantUI.Core.Models;
using PleasantUI.ToolKit.ViewModels;

namespace PleasantUI.ToolKit.Commands.ThemeEditor;

public class ThemeChangeCommand : IEditorCommand
{
	private readonly ThemeEditorViewModel _viewModel;
	
	private readonly Dictionary<string, Color> _previousColors;
	private readonly Dictionary<string, Color> _newColors;
	
	private readonly string _previousName;
	private readonly string? _newName;
	
	public ThemeChangeCommand(ThemeEditorViewModel viewModel, Dictionary<string, Color> previousColors, Dictionary<string, Color> newColors, string previousName, string? newName)
	{
		_viewModel = viewModel;
		_previousColors = previousColors;
		_newColors = newColors;
		_previousName = previousName;
		_newName = newName;
	}
	
	public void Undo()
	{
		AvaloniaList<ThemeColor> themeColors = _viewModel.ThemeColors;
		
		foreach (KeyValuePair<string,Color> pair in _previousColors)
		{
			foreach (ThemeColor themeColor in themeColors)
			{
				if (themeColor.Name != pair.Key) continue;
				
				themeColor.Color = pair.Value;
				break;
			}
		}

		if (!string.IsNullOrWhiteSpace(_newName))
			_viewModel.ChangeThemeName(_previousName);
		
		ChangeColorsInResourceDictionary(themeColors);
	}

	public void Redo()
	{
		AvaloniaList<ThemeColor> themeColors = _viewModel.ThemeColors;
		
		foreach (KeyValuePair<string,Color> pair in _newColors)
		{
			foreach (ThemeColor themeColor in themeColors)
			{
				if (themeColor.Name != pair.Key) continue;
				
				themeColor.Color = pair.Value;
				break;
			}
		}

		if (!string.IsNullOrWhiteSpace(_newName))
			_viewModel.ChangeThemeName(_newName);
		
		ChangeColorsInResourceDictionary(themeColors);
	}
	
	private void ChangeColorsInResourceDictionary(AvaloniaList<ThemeColor> themeColors)
	{
		ResourceDictionary resourceDictionary = _viewModel.ResourceDictionary;
		
		foreach (ThemeColor themeColor in themeColors)
		{
			if (resourceDictionary.ContainsKey(themeColor.Name))
				resourceDictionary[themeColor.Name] = themeColor.Color;
		}
	}
}