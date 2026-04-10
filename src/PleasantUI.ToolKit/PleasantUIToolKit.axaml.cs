using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;

namespace PleasantUI.ToolKit;

/// <summary>
/// PleasantUI ToolKit theme resource dictionary with dynamic VGUI theme loading
/// </summary>
public class PleasantUIToolKit : ResourceDictionary
{
    private static PleasantUIToolKit? _instance;
    private StyleInclude? _vguiStyleInclude;

    /// <summary>
    /// Gets the singleton instance of PleasantUIToolKit
    /// </summary>
    public static PleasantUIToolKit? Instance => _instance;

    /// <summary>
    /// Initializes a new instance of the PleasantUIToolKit class
    /// </summary>
    public PleasantUIToolKit()
    {
        _instance = this;
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Updates the VGUI style by adding or removing the VGUI control themes
    /// </summary>
    /// <param name="isVGUI">Whether VGUI mode is active</param>
    public void UpdateVGUIStyle(bool isVGUI)
    {
        System.Diagnostics.Debug.WriteLine($"[PleasantUIToolKit] UpdateVGUIStyle called with isVGUI={isVGUI}");

        if (isVGUI)
        {
            if (_vguiStyleInclude is null)
            {
                System.Diagnostics.Debug.WriteLine("[PleasantUIToolKit] Adding VGUIControlThemes.axaml");
                try
                {
                    _vguiStyleInclude = new StyleInclude(new Uri("avares://PleasantUI.ToolKit/"))
                    {
                        Source = new Uri("avares://PleasantUI.ToolKit/Styling/VGUIControlThemes.axaml")
                    };
                    MergedDictionaries.Add(_vguiStyleInclude);
                    System.Diagnostics.Debug.WriteLine("[PleasantUIToolKit] VGUIControlThemes.axaml added successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[PleasantUIToolKit] Error adding VGUIControlThemes.axaml: {ex.Message}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[PleasantUIToolKit] VGUIControlThemes.axaml already loaded");
            }
        }
        else
        {
            if (_vguiStyleInclude is not null)
            {
                System.Diagnostics.Debug.WriteLine("[PleasantUIToolKit] Removing VGUIControlThemes.axaml");
                try
                {
                    MergedDictionaries.Remove(_vguiStyleInclude);
                    _vguiStyleInclude = null;
                    System.Diagnostics.Debug.WriteLine("[PleasantUIToolKit] VGUIControlThemes.axaml removed successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[PleasantUIToolKit] Error removing VGUIControlThemes.axaml: {ex.Message}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[PleasantUIToolKit] VGUIControlThemes.axaml not loaded");
            }
        }
    }
}
