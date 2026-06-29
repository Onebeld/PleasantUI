using Avalonia.Media;

namespace PleasantUI.Core.Helpers;

/// <summary>
/// A static class to simplify working with <see cref="FontManager"/>
/// </summary>
public static class FontManagerHelper
{
    /// <summary>
    /// Gets a list of fonts installed on the system in alphabetical order.
    /// </summary>
    /// <returns>List of <see cref="FontFamily"/> in alphabetical order</returns>
    public static List<FontFamily> GetFontsAlphabetically() => FontManager.Current.SystemFonts.OrderBy(f => f.Name).ToList();
}