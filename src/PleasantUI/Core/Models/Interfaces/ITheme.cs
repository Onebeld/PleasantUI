using Avalonia.Styling;

namespace PleasantUI.Core.Models.Interfaces;

/// <summary>
/// An interface that describes the design theme for an application
/// </summary>
public interface ITheme
{
    /// <summary>
    /// Theme name
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Variant of the theme.
    /// </summary>
    public ThemeVariant? ThemeVariant { get; }

    /// <summary>
    /// Sets the theme variant for theme
    /// </summary>
    /// <param name="themeVariant">Theme variant</param>
    void SetThemeVariant(ThemeVariant? themeVariant);
}