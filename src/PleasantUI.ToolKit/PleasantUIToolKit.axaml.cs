using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PleasantUI.ToolKit;

/// <summary>
/// PleasantUI ToolKit theme resource dictionary
/// </summary>
public class PleasantUIToolKit : ResourceDictionary
{
    /// <summary>
    /// Gets the singleton instance of PleasantUIToolKit
    /// </summary>
    public static PleasantUIToolKit? Instance { get; private set; }

    /// <summary>
    /// Initializes a new instance of the PleasantUIToolKit class
    /// </summary>
    public PleasantUIToolKit()
    {
        Instance = this;
        AvaloniaXamlLoader.Load(this);
    }
}
