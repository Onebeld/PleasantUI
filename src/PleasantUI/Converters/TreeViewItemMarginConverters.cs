using Avalonia.Controls.Converters;

namespace PleasantUI.Converters;

/// <summary>
/// Provides static methods and converters for working with tree view items.
/// </summary>
public class TreeViewItemMarginConverters
{
    /// <summary>
    /// Gets the left margin converter.
    /// </summary>
    public static readonly MarginMultiplierConverter Left = new()
    {
        Indent = 16,
        Left = true
    };
}