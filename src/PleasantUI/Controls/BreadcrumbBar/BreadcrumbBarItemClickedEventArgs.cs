using Avalonia.Interactivity;

namespace PleasantUI.Controls;

/// <summary>
/// Provides event data for the <see cref="BreadcrumbBar.ItemClicked"/> event.
/// </summary>
public class BreadcrumbBarItemClickedEventArgs : RoutedEventArgs
{
    /// <summary>Gets the zero-based index of the item that was clicked.</summary>
    public int Index { get; }

    /// <summary>Gets the content/data of the item that was clicked.</summary>
    public object? Item { get; }

    internal BreadcrumbBarItemClickedEventArgs(int index, object? item)
    {
        Index = index;
        Item  = item;
    }
}
