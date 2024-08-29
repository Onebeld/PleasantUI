using Avalonia.Controls;
using Avalonia.Styling;

namespace PleasantUI.Controls;

public class ThemePreviewVariantScope : ThemeVariantScope
{
	private static ThemeVariant _themeVariant = new("0x4C474E41PleasantTheme_Template", ThemeVariant.Light);
	
	public override void ApplyTemplate()
	{
		base.ApplyTemplate();
		
		// Workaround to solve the problem when the current theme is not displayed
		ThemeVariant? themeVariant = RequestedThemeVariant;
		
		RequestedThemeVariant = _themeVariant;
		RequestedThemeVariant = themeVariant;
	}
}