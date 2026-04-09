using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace PleasantUI.Controls;

/// <summary>
/// Hosts the overflow (secondary + dynamically overflowed primary) items of a <see cref="CommandBar"/>.
/// Tracks whether any items have icons or are toggle buttons so items can align themselves consistently.
/// </summary>
[PseudoClasses(PC_Icons, PC_Toggle)]
public class CommandBarOverflowPresenter : ItemsControl
{
    private const string PC_Icons  = ":icons";
    private const string PC_Toggle = ":toggle";

    private int _iconCount;
    private int _toggleCount;

    public CommandBarOverflowPresenter()
    {
        ItemsView.CollectionChanged += OnItemsCollectionChanged;
    }

    protected override Type StyleKeyOverride => typeof(CommandBarOverflowPresenter);

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        recycleKey = null;
        return item is not ICommandBarElement;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsSourceProperty)
        {
            _iconCount   = 0;
            _toggleCount = 0;

            if (change.NewValue is IList list)
                RegisterItems(list);

            UpdateVisualState();
        }
    }

    // ── Collection tracking ───────────────────────────────────────────────────

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems is not null) RegisterItems(e.NewItems);
                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems is not null) UnregisterItems(e.OldItems);
                break;
            case NotifyCollectionChangedAction.Reset:
                if (e.OldItems is not null)
                    UnregisterItems(e.OldItems);
                else
                {
                    _iconCount   = 0;
                    _toggleCount = 0;
                }
                break;
            case NotifyCollectionChangedAction.Replace:
                if (e.OldItems is not null) UnregisterItems(e.OldItems);
                if (e.NewItems is not null) RegisterItems(e.NewItems);
                break;
        }

        UpdateVisualState();
    }

    private void RegisterItems(IList items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            switch (items[i])
            {
                case CommandBarButton btn:
                    if (btn.Icon is not null) _iconCount++;
                    btn.IsInOverflow = true;
                    break;
                case CommandBarToggleButton tb:
                    _toggleCount++;
                    if (tb.Icon is not null) _iconCount++;
                    tb.IsInOverflow = true;
                    break;
                case CommandBarSeparator sep:
                    sep.IsInOverflow = true;
                    break;
            }
        }
    }

    private void UnregisterItems(IList items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            switch (items[i])
            {
                case CommandBarButton btn:
                    if (btn.Icon is not null) _iconCount = Math.Max(0, _iconCount - 1);
                    btn.IsInOverflow = false;
                    ClearItemPseudoClasses(btn);
                    break;
                case CommandBarToggleButton tb:
                    _toggleCount = Math.Max(0, _toggleCount - 1);
                    if (tb.Icon is not null) _iconCount = Math.Max(0, _iconCount - 1);
                    tb.IsInOverflow = false;
                    ClearItemPseudoClasses(tb);
                    break;
                case CommandBarSeparator sep:
                    sep.IsInOverflow = false;
                    break;
            }
        }
    }

    private void UpdateVisualState()
    {
        bool hasIcons  = _iconCount  > 0;
        bool hasToggle = _toggleCount > 0;

        PseudoClasses.Set(PC_Icons,  hasIcons);
        PseudoClasses.Set(PC_Toggle, hasToggle);

        // Propagate to each item so they can align their content.
        var items = Items as IList ?? (IList)ItemsView;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] is Control c && c.Classes is IPseudoClasses pc)
            {
                pc.Set(PC_Icons,  hasIcons);
                pc.Set(PC_Toggle, hasToggle);
            }
        }
    }

    private static void ClearItemPseudoClasses(Control c)
    {
        if (c.Classes is not IPseudoClasses pc) return;
        pc.Set(PC_Icons,  false);
        pc.Set(PC_Toggle, false);
    }
}
