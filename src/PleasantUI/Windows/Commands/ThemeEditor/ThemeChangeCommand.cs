using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using PleasantUI.Core.Models;

namespace PleasantUI.Windows.Commands.ThemeEditor;

public class ThemeChangeCommand : IEditorCommand
{
	private readonly ResourceDictionary _resourceDictionary;
	private readonly AvaloniaList<ThemeColor> _themeColors;
	
	private readonly Dictionary<string, Color> _previousColors;
	private readonly Dictionary<string, Color> _newColors;
	
	public ThemeChangeCommand(AvaloniaList<ThemeColor> themeColors, ResourceDictionary resourceDictionary, Dictionary<string, Color> previousColors, Dictionary<string, Color> newColors)
	{
		_themeColors = themeColors;
		_resourceDictionary = resourceDictionary;
		_previousColors = previousColors;
		_newColors = newColors;
		
	}
	
	public void Undo()
	{
		foreach (KeyValuePair<string,Color> pair in _previousColors)
		{
			foreach (ThemeColor themeColor in _themeColors)
			{
				if (themeColor.Name != pair.Key) continue;
				
				themeColor.Color = pair.Value;
				break;
			}
		}
		
		ChangeColorsInResourceDictionary(_themeColors);
	}

	public void Redo()
	{
		foreach (KeyValuePair<string,Color> pair in _newColors)
		{
			foreach (ThemeColor themeColor in _themeColors)
			{
				if (themeColor.Name != pair.Key) continue;
				
				themeColor.Color = pair.Value;
				break;
			}
		}
		
		ChangeColorsInResourceDictionary(_themeColors);
	}
	
	private void ChangeColorsInResourceDictionary(AvaloniaList<ThemeColor> themeColors)
	{
		foreach (ThemeColor themeColor in themeColors)
		{
			if (_resourceDictionary.ContainsKey(themeColor.Name))
				_resourceDictionary[themeColor.Name] = themeColor.Color;
		}
	}
}