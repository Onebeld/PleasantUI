using Avalonia.Controls;
using Avalonia.Input;

namespace PleasantUI.Controls;

/// <summary>
/// Internal <see cref="ItemsControl"/> that hosts <see cref="PinCodeItem"/> cells.
/// Suppresses left/right arrow key routing so <see cref="PinCode"/> can handle navigation.
/// </summary>
public class PinCodeItemsControl : ItemsControl
{
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        => NeedsContainer<PinCodeItem>(item, out recycleKey);

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new PinCodeItem();

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);

        // Sync the string value from the Digits list into the cell's Text property
        if (container is PinCodeItem cell)
            cell.Text = item as string ?? string.Empty;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key is Key.Left or Key.Right or Key.FnLeftArrow or Key.FnRightArrow)
        {
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }
}
