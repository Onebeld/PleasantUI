using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using PleasantUI.Core.Models;
using PleasantUI.Core.Models.Interfaces;
using PleasantUI.ToolKit.Models;

namespace PleasantUI.ToolKit.Services.Interfaces;

internal interface IThemeService
{
    public IEventAggregator EventAggregator { get; }
    
    public AvaloniaList<ThemeColor> ThemeColors { get; }
    
    public ResourceDictionary ResourceDictionary { get; }
    
    public string JsonColors { get; }
    
    public bool CanUndo { get; }
    
    public bool CanRedo { get; }
    
    public string ThemeName { get; set; }

    void CreateThemeColors(CustomTheme customTheme);

    void ChangeColor(ThemeColor themeColor, Color newColor, Color previousColor);

    Task CopyColorAsync(Color color);

    Task<Color?> PasteColorAsync();

    TopLevel GetTopLevel();
    
    Task CopyThemeAsync();

    Task<bool> PasteThemeAsync();
    
    Task ExportThemeAsync(string path);

    /// <summary>
    /// Undoes the last change.
    /// </summary>
    bool Undo();

    /// <summary>
    /// Redoes the last undone change.
    /// </summary>
    bool Redo();

    bool ImportJson(string? json);

    void GetColorsFromTheme(ITheme theme);

    void ChangeThemeName(string name);
}