using Avalonia.Styling;

namespace PleasantUI.Core.Models.Interfaces;

public interface ITheme
{
    public string Name { get; set; }
    
    public ThemeVariant? ThemeVariant { get; }

    void SetThemeVariant(ThemeVariant? themeVariant);
}