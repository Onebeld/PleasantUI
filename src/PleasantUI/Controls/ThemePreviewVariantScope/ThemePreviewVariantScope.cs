using Avalonia.Controls;
using Avalonia.Styling;

namespace PleasantUI.Controls;

/// <summary>
/// A specialized <see cref="ThemeVariantScope"/> that ensures the correct display of the current theme.
/// </summary>
/// <remarks>
/// This class provides a workaround for a potential issue where the initially selected theme is not displayed correctly.
/// It achieves this by briefly switching to a default theme variant and then back to the requested one during template application.
/// </remarks>
public class ThemePreviewVariantScope : ThemeVariantScope
{
	private static readonly ThemeVariant ThemeVariant = new("0x4C474E41PleasantTheme_Template", ThemeVariant.Light);
	
	/// <summary>
	/// Applies the template for the control.
	/// </summary>
	/// <remarks>
	/// Overrides the base implementation to include a workaround that ensures the correct display of the current theme.
	/// </remarks>
	public override void ApplyTemplate()
	{
		base.ApplyTemplate();
		
		// Workaround to solve the problem when the current theme is not displayed
		ThemeVariant? themeVariant = RequestedThemeVariant;
		
		RequestedThemeVariant = ThemeVariant;
		RequestedThemeVariant = themeVariant;
	}
}