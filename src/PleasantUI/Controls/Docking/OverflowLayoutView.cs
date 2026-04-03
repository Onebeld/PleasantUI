using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Metadata;

namespace PleasantUI.Controls.Docking;

/// <summary>
/// Represents a layout view that handles overflow of items.
/// </summary>
public class OverflowLayoutView : TemplatedControl
{
    /// <summary>Defines the <see cref="ItemsSource"/> property.</summary>
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<OverflowLayoutView, IEnumerable?>(nameof(ItemsSource));

    /// <summary>Defines the <see cref="ItemTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<OverflowLayoutView, IDataTemplate?>(nameof(ItemTemplate));

    /// <summary>Defines the <see cref="MenuItemTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> MenuItemTemplateProperty =
        AvaloniaProperty.Register<OverflowLayoutView, IDataTemplate?>(nameof(MenuItemTemplate));

    /// <summary>Defines the <see cref="Orientation"/> property.</summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<OverflowLayoutView, Orientation>(nameof(Orientation), Orientation.Vertical);

    /// <summary>Defines the <see cref="Spacing"/> property.</summary>
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<OverflowLayoutView, double>(nameof(Spacing));

    /// <summary>Defines the <see cref="Button"/> property.</summary>
    public static readonly StyledProperty<Button?> ButtonProperty =
        AvaloniaProperty.Register<OverflowLayoutView, Button?>(nameof(Button));

    private readonly AvaloniaList<object> _items = [];
    private readonly AvaloniaList<object> _ellipsisItems = [];
    private readonly List<(object item, Size size)> _sizeCache = [];
    private ItemsControl? _itemsControl;
    private ContentPresenter? _buttonPresenter;
    private Size _buttonSize;

    /// <summary>Gets or sets the items source.</summary>
    [Content]
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>Gets or sets the item template.</summary>
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>Gets or sets the menu item template.</summary>
    public IDataTemplate? MenuItemTemplate
    {
        get => GetValue(MenuItemTemplateProperty);
        set => SetValue(MenuItemTemplateProperty, value);
    }

    /// <summary>Gets or sets the orientation of the layout.</summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>Gets or sets the spacing between items.</summary>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <summary>Gets or sets the overflow button.</summary>
    public Button? Button
    {
        get => GetValue(ButtonProperty);
        set => SetValue(ButtonProperty, value);
    }

    internal ItemsControl? ItemsControl => _itemsControl;

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _itemsControl = e.NameScope.Get<ItemsControl>("PART_ItemsControl");
        _buttonPresenter = e.NameScope.Get<ContentPresenter>("PART_ButtonPresenter");

        _itemsControl.ItemsSource = _items;
        _itemsControl.ItemTemplate = ItemTemplate;
        _itemsControl.ItemsPanel = new FuncTemplate<Panel?>(() => new StackPanel
        {
            Spacing = Spacing,
            Orientation = Orientation
        });
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsSourceProperty)
            UpdateItemsSource(change.OldValue as IEnumerable, change.NewValue as IEnumerable);

        if (_itemsControl != null)
        {
            if (change.Property == ItemTemplateProperty)
                _itemsControl.ItemTemplate = ItemTemplate;
            else if (_itemsControl.ItemsPanelRoot is StackPanel panel &&
                     (change.Property == SpacingProperty || change.Property == OrientationProperty))
            {
                panel.Spacing = Spacing;
                panel.Orientation = Orientation;
            }
        }
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (_buttonSize.Width == 0 || _buttonSize.Height == 0)
        {
            if (_buttonPresenter != null)
            {
                _buttonPresenter.IsVisible = true;
                _buttonPresenter.Measure(Size.Infinity);
                _buttonSize = _buttonPresenter.DesiredSize;
                _buttonPresenter.IsVisible = _ellipsisItems.Count > 0;
            }
        }

        if (double.IsInfinity(availableSize.Width) || double.IsInfinity(availableSize.Height))
            ResetItems();
        else
            UpdateLayout(availableSize);

        return base.MeasureOverride(availableSize);
    }

    private void UpdateSizeCache()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            var obj = _items[i];
            var index = _sizeCache.FindIndex(x => x.item.Equals(obj));
            var control = _itemsControl?.ContainerFromIndex(i);

            if (control == null)
            {
                if (index >= 0) _sizeCache.RemoveAt(index);
                continue;
            }

            var size = control.DesiredSize;
            if (index >= 0)
                _sizeCache[index] = (obj, size);
            else
                _sizeCache.Add((obj, size));
        }
    }

    private void UpdateItemsSource(IEnumerable? oldValue, IEnumerable? newValue)
    {
        if (oldValue is INotifyCollectionChanged oldCollection)
            oldCollection.CollectionChanged -= OnItemsSourceCollectionChanged;
        if (newValue is INotifyCollectionChanged newCollection)
            newCollection.CollectionChanged += OnItemsSourceCollectionChanged;

        _sizeCache.Clear();
        ResetItems();
        InvalidateMeasure();
    }

    private void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            if (e.NewStartingIndex <= _items.Count)
            {
                _items.AddRange(e.NewItems!.Cast<object>());
                InvalidateMeasure();
                return;
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            _sizeCache.RemoveAll(o => e.OldItems!.Contains(o.item));
            if (e.OldStartingIndex >= _items.Count)
            {
                _ellipsisItems.RemoveRange(e.OldStartingIndex - _items.Count, e.OldItems!.Count);
                InvalidateMeasure();
                return;
            }
        }

        ResetItems();
        InvalidateMeasure();
    }

    private void ResetItems()
    {
        _items.Clear();
        _ellipsisItems.Clear();

        if (ItemsSource != null)
            _items.AddRange(ItemsSource.Cast<object>());

        if (_buttonPresenter != null)
            _buttonPresenter.IsVisible = false;
    }

    private double GetSize(Size size)
        => Orientation == Orientation.Horizontal ? size.Width : size.Height;

    private double GetItemSize(object item)
        => GetSize(_sizeCache.FirstOrDefault(x => x.item.Equals(item)).size);

    private double GetButtonSize()
        => GetSize(_buttonSize);

    private void UpdateLayout(Size availableSize)
    {
        var available = GetSize(availableSize);
        var buttonSize = GetButtonSize();
        available -= buttonSize + Spacing;

        var size = GetSize(LayoutHelper.MeasureChild(_itemsControl, Size.Infinity, default));
        UpdateSizeCache();

        if (size >= available)
        {
            var overflow = size - available;
            int i = _items.Count - 1;

            for (; i >= 0; i--)
            {
                overflow -= GetItemSize(_items[i]);
                overflow -= Spacing;

                if (overflow < 0)
                {
                    if (overflow - (buttonSize + Spacing) < 0) i--;
                    break;
                }
            }

            if (i < _items.Count - 1)
            {
                var newEllipsisItems = _items.Skip(i + 1).ToArray();
                _items.RemoveRange(i + 1, _items.Count - i - 1);
                _ellipsisItems.InsertRange(0, newEllipsisItems);
                if (_buttonPresenter != null)
                    _buttonPresenter.IsVisible = _ellipsisItems.Count > 0;
            }
        }
        else
        {
            var space = available - size;
            int i = 0;

            for (; i < _ellipsisItems.Count; i++)
            {
                space -= GetItemSize(_ellipsisItems[i]);
                space -= Spacing;
                if (space < 0) break;
            }

            if (i > 0)
            {
                var newItems = _ellipsisItems.Take(i).ToArray();
                _ellipsisItems.RemoveRange(0, i);
                _items.AddRange(newItems);
                if (_buttonPresenter != null)
                    _buttonPresenter.IsVisible = _ellipsisItems.Count > 0;
            }
        }
    }
}
