using Avalonia.Styling;
using PleasantUI.Core.Models.Interfaces;

namespace PleasantUI.Core.Models;

/// <summary>
/// Represents a theme with a name and a variant.
/// </summary>
public class Theme : ITheme
{
    /// <summary>
    /// Gets or sets the name of the theme.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the variant of the theme.
    /// </summary>
    public ThemeVariant? ThemeVariant { get; private set; }

    /// <summary>
    /// Sets the theme variant for theme
    /// </summary>
    /// <param name="themeVariant">Theme variant</param>
    /// <exception cref="ArgumentNullException">Called if theme variant is null</exception>
    public void SetThemeVariant(ThemeVariant? themeVariant)
    {
        ThemeVariant = themeVariant;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Theme" /> class.
    /// </summary>
    /// <param name="name">The name of the theme.</param>
    /// <param name="themeVariant">The variant of the theme.</param>
    public Theme(string name, ThemeVariant? themeVariant)
    {
        Name = name;
        ThemeVariant = themeVariant;
    }
}