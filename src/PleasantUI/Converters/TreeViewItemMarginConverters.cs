using Avalonia.Controls.Converters;

namespace PleasantUI.Converters;

public class TreeViewItemMarginConverters
{
    public static readonly MarginMultiplierConverter Left = new()
    {
        Indent = 16,
        Left = true
    };
}