using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using PleasantUI.Core;

namespace PleasantUI.ToolKit;

/// <summary>
/// PleasantUI ToolKit theme resource dictionary with dynamic VGUI theme loading
/// </summary>
public class PleasantUIToolKit : ResourceDictionary
{
    private static PleasantUIToolKit? _instance;
    private StyleInclude? _vguiStyleInclude;
    private bool _listenerAdded = false;

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
        
        TryAddListener();
    }

    private void TryAddListener()
    {
        if (_listenerAdded) return;
        
        if (PleasantSettings.Current is not null)
        {
            PleasantSettings.Current.PropertyChanged += OnPleasantSettingsChanged;
            _listenerAdded = true;
            // Check if VGUI is already active
            UpdateVGUIStyle(PleasantSettings.Current.Theme == "VGUI");
            System.Diagnostics.Debug.WriteLine("[PleasantUIToolKit] Listener added, checking VGUI status");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[PleasantUIToolKit] PleasantSettings.Current is null, will retry later");
            // Retry after a delay
            System.Threading.Tasks.Task.Delay(100).ContinueWith(_ =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(TryAddListener);
            });
        }
    }

    private void OnPleasantSettingsChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PleasantSettings.Current.Theme))
        {
            UpdateVGUIStyle(PleasantSettings.Current?.Theme == "VGUI");
        }
    }

    /// <summary>
    /// Manually triggers VGUI style update (called from PleasantTheme via reflection)
    /// </summary>
    public static void UpdateVGUIStyleManually(bool isVGUI)
    {
        System.Diagnostics.Debug.WriteLine($"[PleasantUIToolKit] UpdateVGUIStyleManually called with isVGUI={isVGUI}");
        
        if (Avalonia.Application.Current?.Resources is null)
        {
            System.Diagnostics.Debug.WriteLine("[PleasantUIToolKit] Application.Current.Resources is null");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"[PleasantUIToolKit] Application.Current.Resources.MergedDictionaries count: {Avalonia.Application.Current.Resources.MergedDictionaries.Count}");
        
        // Log all resources to debug
        int index = 0;
        foreach (var resource in Avalonia.Application.Current.Resources.MergedDictionaries)
        {
            string resourceInfo = resource?.GetType().FullName ?? "null";
            if (resource is Avalonia.Markup.Xaml.Styling.ResourceInclude ri)
            {
                resourceInfo += $" (Source: {ri.Source})";
            }
            System.Diagnostics.Debug.WriteLine($"[PleasantUIToolKit] Resource [{index}]: {resourceInfo}");
            index++;
        }

        // Find PleasantUIToolKit in MergedDictionaries
        PleasantUIToolKit? toolKit = null;
        foreach (var resource in Avalonia.Application.Current.Resources.MergedDictionaries)
        {
            if (resource is PleasantUIToolKit tk)
            {
                toolKit = tk;
                break;
            }
        }

        if (toolKit is not null)
        {
            toolKit.UpdateVGUIStyle(isVGUI);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[PleasantUIToolKit] PleasantUIToolKit not found in Application.Resources.MergedDictionaries");
            // Try to find it recursively in nested MergedDictionaries
            FindRecursive(Avalonia.Application.Current.Resources, 0);
        }
    }

    private static void FindRecursive(IResourceDictionary dict, int depth)
    {
        string indent = new string(' ', depth * 2);
        System.Diagnostics.Debug.WriteLine($"{indent}Checking dictionary: {dict?.GetType().FullName}");
        
        if (dict is PleasantUIToolKit)
        {
            System.Diagnostics.Debug.WriteLine($"{indent}FOUND PleasantUIToolKit!");
        }
        
        foreach (var resource in dict.MergedDictionaries)
        {
            if (resource is IResourceDictionary rd)
            {
                FindRecursive(rd, depth + 1);
            }
        }
    }

    /// <summary>
    /// Updates the VGUI style by adding or removing the VGUI control themes
    /// </summary>
    /// <param name="isVGUI">Whether VGUI mode is active</param>
    public void UpdateVGUIStyle(bool isVGUI)
    {
        System.Diagnostics.Debug.WriteLine($"[PleasantUIToolKit] UpdateVGUIStyle called with isVGUI={isVGUI}");
        System.Diagnostics.Debug.WriteLine($"[PleasantUIToolKit] MergedDictionaries count before: {MergedDictionaries.Count}");

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
                    System.Diagnostics.Debug.WriteLine($"[PleasantUIToolKit] VGUIControlThemes.axaml added successfully, MergedDictionaries count after: {MergedDictionaries.Count}");
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
