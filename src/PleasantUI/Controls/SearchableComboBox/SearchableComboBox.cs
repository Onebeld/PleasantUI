using System.Collections;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;
using Size = Avalonia.Size;

namespace PleasantUI.Controls;

/// <summary>
/// A drop-down list control with search functionality.
/// </summary>
[TemplatePart("PART_Popup", typeof(Popup), IsRequired = true)]
[TemplatePart("PART_EditableTextBox", typeof(TextBox), IsRequired = true)]
public class SearchableComboBox : ItemsControl
{
    private Popup? _popup;
    private TextBox? _editableTextBox;

    private bool _isUpdatingText;
    private bool _isFiltering;

    private object? _selectedItem;
    private int _selectedIndex = -1;

    private static readonly FuncTemplate<Panel?> DefaultPanel = new(() => new StackPanel());
    
    public static readonly DirectProperty<SearchableComboBox, int> SelectedIndexProperty =
        AvaloniaProperty.RegisterDirect<SearchableComboBox, int>(
            nameof(SelectedIndex),
            o => o.SelectedIndex,
            (o, v) => o.SelectedIndex = v,
            -1);

    public static readonly DirectProperty<SearchableComboBox, object?> SelectedItemProperty =
        AvaloniaProperty.RegisterDirect<SearchableComboBox, object?>(
            nameof(SelectedItem),
            o => o.SelectedItem,
            (o, v) => o.SelectedItem = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    public static readonly DirectProperty<SearchableComboBox, object?> SelectionBoxItemProperty =
        AvaloniaProperty.RegisterDirect<SearchableComboBox, object>(nameof(SelectionBoxItem),
            o => o.SelectionBoxItem);
    
    public static readonly DirectProperty<SearchableComboBox, IEnumerable?> AllItemsProperty =
        AvaloniaProperty.RegisterDirect<SearchableComboBox, IEnumerable?>(
            nameof(AllItems), o => o.AllItems, (o, v) => o.AllItems = v);

    public static readonly DirectProperty<SearchableComboBox, int> FilteredItemCountProperty =
        AvaloniaProperty.RegisterDirect<SearchableComboBox, int>(nameof(ItemCount), o => o.ItemCount);

    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<SearchableComboBox, bool>(nameof(IsDropDownOpen));

    public static readonly StyledProperty<double> MaxDropDownHeightProperty =
        AvaloniaProperty.Register<SearchableComboBox, double>(nameof(MaxDropDownHeight), 200);

    public static readonly StyledProperty<string?> FilterTextProperty =
        AvaloniaProperty.Register<SearchableComboBox, string?>(nameof(FilterText), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<SearchableComboBox, string?>(nameof(PlaceholderText));

    public static readonly StyledProperty<IBrush?> PlaceholderForegroundProperty =
        AvaloniaProperty.Register<SearchableComboBox, IBrush?>(nameof(PlaceholderForeground));

    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        ContentControl.HorizontalContentAlignmentProperty.AddOwner<SearchableComboBox>();

    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        ContentControl.VerticalContentAlignmentProperty.AddOwner<SearchableComboBox>();

    public static readonly StyledProperty<Func<object?, string?, bool>?> FilterFunctionProperty =
        AvaloniaProperty.Register<SearchableComboBox, Func<object?, string?, bool>?>(nameof(FilterFunction));

    public static readonly StyledProperty<IDataTemplate?> SelectionBoxItemTemplateProperty =
        AvaloniaProperty.Register<SearchableComboBox, IDataTemplate?>(nameof(SelectionBoxItemTemplate),
            defaultBindingMode: BindingMode.TwoWay,
            coerce: CoerceSelectionBoxItemTemplate);
    
    public event EventHandler? DropDownClosed;
    
    public event EventHandler? DropDownOpened;

    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public double MaxDropDownHeight
    {
        get => GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    public string? FilterText
    {
        get => GetValue(FilterTextProperty);
        set => SetValue(FilterTextProperty, value);
    }

    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public IBrush? PlaceholderForeground
    {
        get => GetValue(PlaceholderForegroundProperty);
        set => SetValue(PlaceholderForegroundProperty, value);
    }

    public HorizontalAlignment HorizontalContentAlignment
    {
        get => GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }

    public VerticalAlignment VerticalContentAlignment
    {
        get => GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }

    public Func<object?, string?, bool>? FilterFunction
    {
        get => GetValue(FilterFunctionProperty);
        set => SetValue(FilterFunctionProperty, value);
    }

    public IEnumerable? AllItems
    {
        get;
        set => SetAndRaise(AllItemsProperty, ref field, value);
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex == value) return;
            _selectedIndex = value;
            RaisePropertyChanged(SelectedIndexProperty, _selectedIndex, value);
            UpdateSelectedItemFromIndex();
        }
    }

    public object? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (Equals(_selectedItem, value))
                return;
            
            var oldItem = _selectedItem;
            _selectedItem = value;
            RaisePropertyChanged(SelectedItemProperty, oldItem, value);
            UpdateSelectedIndexFromItem();
        }
    }
    
    public int FilteredItemCount
    {
        get;
        private set => SetAndRaise(FilteredItemCountProperty, ref field, value);
    }

    /// <summary>
    /// Gets or sets the item to display as the control's content.
    /// </summary>
    public object? SelectionBoxItem
    {
        get;
        protected set => SetAndRaise(SelectionBoxItemProperty, ref field, value);
    }

    /// <summary>
    /// Gets or sets the DataTemplate used to display the selected item. This has a higher priority than <see cref="ItemsControl.ItemTemplate"/> if set.
    /// </summary>
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? SelectionBoxItemTemplate
    {
        get => GetValue(SelectionBoxItemTemplateProperty);
        set => SetValue(SelectionBoxItemTemplateProperty, value);
    }
    
    static SearchableComboBox()
    {
        ItemsPanelProperty.OverrideDefaultValue<SearchableComboBox>(DefaultPanel);
        FocusableProperty.OverrideDefaultValue<SearchableComboBox>(true);
    }
    
    private static IDataTemplate? CoerceSelectionBoxItemTemplate(AvaloniaObject obj, IDataTemplate? template)
    {
        if (template is not null)
            return template;
        
        if (obj is SearchableComboBox comboBox && template is null)
            return comboBox.ItemTemplate;
        return template;
    }
    
    public void Clear()
    {
        SelectedItem = null;
        SelectedIndex = -1;
        SetCurrentValue(FilterTextProperty, string.Empty);
        EnsureAllItemsVisible();
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_popup != null)
        {
            _popup.Opened -= PopupOpened;
            _popup.Closed -= PopupClosed;
        }

        _popup = e.NameScope.Get<Popup>("PART_Popup");
        _editableTextBox = e.NameScope.Get<TextBox>("PART_EditableTextBox");

        if (_popup != null)
        {
            _popup.Opened += PopupOpened;
            _popup.Closed += PopupClosed;
        }
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedItemProperty)
        {
            UpdateSelectionBoxItem(SelectedItem);
            TryFocusSelectedItem();
        }
        else if (change.Property == FilterTextProperty && !_isUpdatingText)
        {
            FilterItems();
        }
        else if (change.Property == ItemsSourceProperty)
        {
            IEnumerable newItems = change.GetNewValue<IEnumerable>();
            if (!_isFiltering && AllItems == null)
                AllItems = newItems;
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (AllItems == null) 
            AllItems = ItemsSource ?? Items.Cast<object>().ToList();
    }
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.Handled) return;

        if (e.Source is Visual visual && _popup?.IsInsidePopup(visual) == true)
            return;

        if (!IsDropDownOpen)
        {
            EnsureAllItemsVisible();
            SetCurrentValue(FilterTextProperty, string.Empty);
            SetCurrentValue(IsDropDownOpenProperty, true);
        }
        else
            SetCurrentValue(IsDropDownOpenProperty, false);

        e.Handled = true;

        base.OnPointerPressed(e);
    }
    
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (e.Handled) return;

        if (e.Source is Visual visual && _popup?.IsInsidePopup(visual) == true)
        {
            if (UpdateSelectionFromEventSource(e.Source))
            {
                SetCurrentValue(IsDropDownOpenProperty, false);
                
                e.Handled = true;
            }
        }

        base.OnPointerReleased(e);
    }
    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled) return;

        if (IsDropDownOpen && e.Key == Key.Escape)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
            return;
        }

        if ((e.Key == Key.F4 && !e.KeyModifiers.HasFlag(KeyModifiers.Alt)) ||
            ((e.Key == Key.Down || e.Key == Key.Up) && e.KeyModifiers.HasFlag(KeyModifiers.Alt)))
        {
            SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
            e.Handled = true;
        }
        else if (!IsDropDownOpen && (e.Key == Key.Enter || e.Key == Key.Space))
        {
            SetCurrentValue(IsDropDownOpenProperty, true);
            e.Handled = true;
        }
        else if (IsDropDownOpen && e.Key == Key.Enter)
        {
            SelectFocusedItem();
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
        }
        else if (IsDropDownOpen && ItemCount > 0 && (e.Key == Key.Down || e.Key == Key.Up))
        {
            var direction = e.Key == Key.Down ? 1 : -1;
            MoveFocus(direction);
            e.Handled = true;
        }
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new SearchableComboBoxItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<SearchableComboBoxItem>(item, out recycleKey);
    }
    
    internal void ItemFocused(SearchableComboBoxItem dropDownItem)
    {
        if (IsDropDownOpen && dropDownItem.IsFocused && dropDownItem.IsArrangeValid)
        {
            dropDownItem.BringIntoView();
        }
    }
    
    protected int IndexFromItem(object? item)
    {
        if (item == null) return -1;
        return Items.IndexOf(item);
    }
    
    private bool UpdateSelectionFromEventSource(object? eventSource)
    {
        var container = GetContainerFromEventSource(eventSource);
        if (container == null) return false;

        int index = IndexFromContainer(container);
        if (index < 0) return false;

        var item = Items.ElementAtOrDefault(index);
        if (item != null)
        {
            SelectedItem = item;
            return true;
        }

        return false;
    }
    
    private Control? GetContainerFromEventSource(object? eventSource)
    {
        for (Visual? visual = eventSource as Visual; visual != null; visual = visual.Parent as Visual)
        {
            if (visual is Control control && control.Parent == this && IndexFromContainer(control) != -1)
                return control;
        }
        return null;
    }
    
    private void PopupOpened(object? sender, EventArgs e)
    {
        EnsureAllItemsVisible();
        
        _isUpdatingText = true;
        SetCurrentValue(FilterTextProperty, string.Empty);
        _isUpdatingText = false;

        UpdateContainerSelection();
        _editableTextBox?.Focus();
        
        if (SelectedItem != null)
            ScrollIntoView(SelectedItem);

        DropDownOpened?.Invoke(this, EventArgs.Empty);
    }

    private void PopupClosed(object? sender, EventArgs e)
    {
        SetCurrentValue(FilterTextProperty, string.Empty);
        EnsureAllItemsVisible();

        DropDownClosed?.Invoke(this, EventArgs.Empty);
    }
    
    private void EnsureAllItemsVisible()
    {
        if (AllItems == null)
        {
            AllItems = ItemsSource ?? Items.Cast<object>().ToList();
        }

        if (!Equals(ItemsSource, AllItems))
            SetCurrentValue(ItemsSourceProperty, AllItems);
    }

    private void FilterItems()
    {
        if (_isFiltering) return;

        _isFiltering = true;
        try
        {
            if (AllItems == null)
            {
                AllItems = ItemsSource ?? Items.Cast<object>().ToList();
                if (AllItems == null) return;
            }

            string? filterText = FilterText;

            if (string.IsNullOrEmpty(filterText))
            {
                if (!Equals(ItemsSource, AllItems))
                    SetCurrentValue(ItemsSourceProperty, AllItems);
                
                UpdateContainerSelection();
                return;
            }

            Func<object?, string?, bool> filterFunc = FilterFunction ?? DefaultFilterFunction;
            
            List<object> filtered = AllItems.Cast<object>()
                .Where(item => filterFunc(item, filterText))
                .ToList();

            SetCurrentValue(ItemsSourceProperty, new ObservableCollection<object>(filtered));
            FilteredItemCount = filtered.Count;
            
            UpdateContainerSelection();
        }
        finally
        {
            _isFiltering = false;
        }
    }

    private static bool DefaultFilterFunction(object? item, string? filterText)
    {
        if (item == null || string.IsNullOrEmpty(filterText)) return true;
        string itemText = item is SearchableComboBoxItem cbi
            ? cbi.Content?.ToString() ?? ""
            : item.ToString() ?? "";
        return itemText.Contains(filterText, StringComparison.OrdinalIgnoreCase);
    }

    private void UpdateSelectionBoxItem(object? item)
    {
        if (item is ContentControl contentControl)
            item = contentControl.Content;

        if (item is Control control)
        {
            if (VisualRoot != null)
            {
                control.Measure(Size.Infinity);

                SelectionBoxItem = new Rectangle
                {
                    Width = control.DesiredSize.Width,
                    Height = control.DesiredSize.Height,
                    Fill = new VisualBrush
                    {
                        Visual = control,
                        Stretch = Stretch.None,
                        AlignmentX = AlignmentX.Left,
                    }
                };
            }

            UpdateFlowDirection();
        }
        else
        {
            if (item is not null && ItemTemplate is null && SelectionBoxItemTemplate is null && DisplayMemberBinding is { } binding)
            {
                FuncDataTemplate<object?> template = new((_, _) =>
                    new TextBlock
                    {
                        [TextBlock.DataContextProperty] = item,
                        [!TextBlock.TextProperty] = binding,
                    });
                Control? text = template.Build(item);
                SelectionBoxItem = text;
            }
            else
            {
                SelectionBoxItem = item;
            }
                
        }
    }
    
    private void UpdateFlowDirection()
    {
        if (SelectionBoxItem is not Rectangle rectangle)
            return;

        if ((rectangle.Fill as VisualBrush)?.Visual is not { } content)
            return;
        
        FlowDirection flowDirection = content.GetVisualParent()?.FlowDirection ?? FlowDirection.LeftToRight;
        rectangle.FlowDirection = flowDirection;
    }

    private void UpdateSelectedItemFromIndex()
    {
        if (_selectedIndex >= 0 && _selectedIndex < ItemCount)
        {
            var item = ContainerFromIndex(_selectedIndex)?.DataContext ?? Items.ElementAtOrDefault(_selectedIndex);
            if (item != null && !Equals(_selectedItem, item))
                SelectedItem = item;
        }
    }

    private void UpdateSelectedIndexFromItem()
    {
        if (SelectedItem == null)
        {
            _selectedIndex = -1;
            return;
        }

        var index = IndexFromItem(SelectedItem);
        if (index >= 0)
        {
            _selectedIndex = index;
            return;
        }

        if (AllItems != null)
        {
            var allList = AllItems.Cast<object>().ToList();
            _selectedIndex = allList.FindIndex(x => Equals(x, SelectedItem));
        }
    }

    private void SelectFocusedItem()
    {
        foreach (var container in GetRealizedContainers())
        {
            if (container.IsFocused)
            {
                var index = IndexFromContainer(container);
                if (index >= 0)
                {
                    var item = Items.ElementAtOrDefault(index);
                    SelectedItem = item;           // This preserves the real item
                    UpdateContainerSelection();
                    SetCurrentValue(FilterTextProperty, string.Empty);
                    EnsureAllItemsVisible();
                }
                break;
            }
        }
    }

    private void TryFocusSelectedItem()
    {
        if (!IsDropDownOpen || SelectedIndex == -1) return;

        var container = ContainerFromIndex(SelectedIndex);
        if (container == null)
        {
            ScrollIntoView(SelectedIndex);
            container = ContainerFromIndex(SelectedIndex);
        }

        container?.Focus();
    }

    private void MoveFocus(int direction)
    {
        var containers = GetRealizedContainers().ToList();
        if (containers.Count == 0) return;

        var focusedIndex = -1;
        for (int i = 0; i < containers.Count; i++)
        {
            if (containers[i].IsFocused)
            {
                focusedIndex = i;
                break;
            }
        }

        var newIndex = focusedIndex + direction;
        if (newIndex < 0) newIndex = containers.Count - 1;
        if (newIndex >= containers.Count) newIndex = 0;

        if (newIndex >= 0 && newIndex < containers.Count)
        {
            containers[newIndex].Focus();
        }
    }
    
    private void UpdateContainerSelection()
    {
        if (Presenter?.Panel == null) return;

        object? selectedItem = SelectedItem;
        if (selectedItem == null) return;

        foreach (Control child in Presenter.Panel.Children)
        {
            if (child is not SearchableComboBoxItem item)
                continue;
            
            bool isSelected = Equals(GetItemFromContainer(item), selectedItem);
            item.SetCurrentValue(SearchableComboBoxItem.IsSelectedProperty, isSelected);
        }
    }
    
    private object? GetItemFromContainer(Control container)
    {
        return container.DataContext ?? (container as ContentControl)?.Content;
    }
}