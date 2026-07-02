using System.Collections;
using Avalonia;
using Avalonia.Collections;
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

    private bool _isFiltering;

    private object? _selectedItem;
    private int _selectedIndex = -1;
    
    private readonly AvaloniaList<object> _filteredItems = [];

    private static readonly FuncTemplate<Panel?> DefaultPanel = new(() => new VirtualizingStackPanel());
    
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
        AvaloniaProperty.RegisterDirect<SearchableComboBox, object?>(nameof(SelectionBoxItem),
            o => o.SelectionBoxItem);
    
    public static readonly DirectProperty<SearchableComboBox, IEnumerable?> AllItemsProperty =
        AvaloniaProperty.RegisterDirect<SearchableComboBox, IEnumerable?>(
            nameof(AllItems), o => o.AllItems, (o, v) => o.AllItems = v);

    public static readonly DirectProperty<SearchableComboBox, int> FilteredItemCountProperty =
        AvaloniaProperty.RegisterDirect<SearchableComboBox, int>(nameof(FilteredItemCount), o => o.FilteredItemCount);

    /// <summary>
    /// Defines the <see cref="IsDropDownOpen"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<SearchableComboBox, bool>(nameof(IsDropDownOpen));

    /// <summary>
    /// Defines the <see cref="MaxDropDownHeight"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MaxDropDownHeightProperty =
        AvaloniaProperty.Register<SearchableComboBox, double>(nameof(MaxDropDownHeight), 200);

    /// <summary>
    /// Defines the <see cref="FilterText"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> FilterTextProperty =
        AvaloniaProperty.Register<SearchableComboBox, string?>(nameof(FilterText), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Defines the <see cref="PlaceholderText"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<SearchableComboBox, string?>(nameof(PlaceholderText));
    
    /// <summary>
    /// Defines the <see cref="FilterPlaceholderText"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> FilterPlaceholderTextProperty =
        AvaloniaProperty.Register<SearchableComboBox, string?>(nameof(FilterPlaceholderText));

    /// <summary>
    /// Defines the <see cref="PlaceholderForeground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> PlaceholderForegroundProperty =
        AvaloniaProperty.Register<SearchableComboBox, IBrush?>(nameof(PlaceholderForeground));

    /// <summary>
    /// Defines the <see cref="HorizontalContentAlignment"/> property.
    /// </summary>
    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        ContentControl.HorizontalContentAlignmentProperty.AddOwner<SearchableComboBox>();

    /// <summary>
    /// Defines the <see cref="VerticalContentAlignment"/> property.
    /// </summary>
    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        ContentControl.VerticalContentAlignmentProperty.AddOwner<SearchableComboBox>();

    /// <summary>
    /// Defines the <see cref="FilterFunction"/> property.
    /// </summary>
    public static readonly StyledProperty<Func<object?, string?, bool>?> FilterFunctionProperty =
        AvaloniaProperty.Register<SearchableComboBox, Func<object?, string?, bool>?>(nameof(FilterFunction));

    /// <summary>
    /// Defines the <see cref="SelectionBoxItemTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> SelectionBoxItemTemplateProperty =
        AvaloniaProperty.Register<SearchableComboBox, IDataTemplate?>(nameof(SelectionBoxItemTemplate),
            defaultBindingMode: BindingMode.TwoWay,
            coerce: CoerceSelectionBoxItemTemplate);
    
    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;
    
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
    
    public string? FilterPlaceholderText
    {
        get => GetValue(FilterPlaceholderTextProperty);
        set => SetValue(FilterPlaceholderTextProperty, value);
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
            
            object? oldItem = _selectedItem;
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
        return template ?? (obj as SearchableComboBox)?.ItemTemplate;
    }
    
    public void Clear()
    {
        SelectedItem = null;
        SelectedIndex = -1;
        SetCurrentValue(FilterTextProperty, string.Empty);
        EnsureAllItems();
    }
    
    /// <inheritdoc />
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
    
    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedItemProperty)
        {
            UpdateSelectionBoxItem(SelectedItem);
            TryFocusSelectedItem();
        }
        else if (change.Property == FilterTextProperty)
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

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        EnsureAllItems();
    }
    
    /// <inheritdoc />
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.Handled) return;

        if (e.Source is Visual visual && _popup?.IsInsidePopup(visual) == true)
            return;

        SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);

        e.Handled = true;
        base.OnPointerPressed(e);
    }
    
    /// <inheritdoc />
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
    
    /// <inheritdoc />
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
            (e.Key is Key.Down or Key.Up && e.KeyModifiers.HasFlag(KeyModifiers.Alt)))
        {
            SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
            e.Handled = true;
        }
        else if (!IsDropDownOpen && e.Key is Key.Enter or Key.Space)
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
        else if (IsDropDownOpen && ItemCount > 0 && e.Key is Key.Down or Key.Up)
        {
            int direction = e.Key == Key.Down ? 1 : -1;
            MoveFocus(direction);
            e.Handled = true;
        }
    }
    
    /// <inheritdoc />
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);

        if (container is SearchableComboBoxItem comboItem)
        {
            bool isSelected = Equals(item, SelectedItem);
            comboItem.SetCurrentValue(ListBoxItem.IsSelectedProperty, isSelected);
        }
    }
    
    /// <inheritdoc />
    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is SearchableComboBoxItem comboItem)
            comboItem.SetCurrentValue(ListBoxItem.IsSelectedProperty, false);

        base.ClearContainerForItemOverride(container);
    }

    /// <inheritdoc />
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new SearchableComboBoxItem();
    }

    /// <inheritdoc />
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<SearchableComboBoxItem>(item, out recycleKey);
    }
    
    internal void ItemFocused(SearchableComboBoxItem dropDownItem)
    {
        if (IsDropDownOpen && dropDownItem is { IsFocused: true, IsArrangeValid: true })
        {
            dropDownItem.BringIntoView();
        }
    }
    
    private int IndexFromItem(object? item)
    {
        if (item == null) return -1;
        return Items.IndexOf(item);
    }
    
    private bool UpdateSelectionFromEventSource(object? eventSource)
    {
        Control? container = GetContainerFromEventSource(eventSource);
        if (container == null) return false;

        int index = IndexFromContainer(container);
        if (index < 0) return false;

        object? item = index < Items.Count ? Items[index] : null;
        
        if (item == null)
            return false;
        
        DeselectContainerForItem(SelectedItem);

        SelectedItem = item;
        if (container is  SearchableComboBoxItem comboItem)
            comboItem.IsSelected = true;
        
        // TODO: SelectionChanged
        //SelectionChanged?.Invoke(this, new SelectionChangedEventArgs());
            
        return true;
    }

    private void DeselectContainerForItem(object? item)
    {
        if (item == null)
            return;
        
        Control? oldContainer = ContainerFromItem(item);
        if (oldContainer is SearchableComboBoxItem comboItem)
            comboItem.IsSelected = false;
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
        EnsureAllItems();
        SetCurrentValue(FilterTextProperty, string.Empty);
        
        _editableTextBox?.Focus();
        
        if (SelectedItem != null)
            ScrollIntoView(SelectedItem);

        DropDownOpened?.Invoke(this, EventArgs.Empty);
    }

    private void PopupClosed(object? sender, EventArgs e)
    {
        SetCurrentValue(FilterTextProperty, string.Empty);
        EnsureAllItems();
        
        DropDownClosed?.Invoke(this, EventArgs.Empty);
    }
    
    private void EnsureAllItems()
    {
        AllItems ??= ItemsSource ?? Items.Cast<object>().ToList();

        if (!Equals(ItemsSource, AllItems))
            SetCurrentValue(ItemsSourceProperty, AllItems);
    }

    private void FilterItems()
    {
        if (_isFiltering) return;

        _isFiltering = true;
        try
        {
            EnsureAllItems();
            
            if (AllItems == null) return;

            string? filterText = FilterText;

            if (string.IsNullOrEmpty(filterText))
            {
                if (!Equals(ItemsSource, AllItems))
                    SetCurrentValue(ItemsSourceProperty, AllItems);

                if (AllItems is ICollection collection)
                    FilteredItemCount = collection.Count;
            }
            else
            {
                Func<object?, string?, bool> filterFunc = FilterFunction ?? DefaultFilterFunction;
                
                _filteredItems.Clear();

                foreach (object? item in AllItems)
                {
                    if (filterFunc(item, filterText))
                        _filteredItems.Add(item);
                }
                
                if (!Equals(ItemsSource, _filteredItems))
                    SetCurrentValue(ItemsSourceProperty, _filteredItems);
                
                FilteredItemCount = _filteredItems.Count;
            }
        }
        finally
        {
            _isFiltering = false;
        }
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
            SelectionBoxItem = item;
        }
    }

    private void UpdateSelectedItemFromIndex()
    {
        if (_selectedIndex < 0 || _selectedIndex >= ItemCount)
            return;
        
        object? item = ContainerFromIndex(_selectedIndex)?.DataContext ?? Items.ElementAtOrDefault(_selectedIndex);
        if (item != null && !Equals(_selectedItem, item))
            SelectedItem = item;
    }

    private void UpdateSelectedIndexFromItem()
    {
        if (SelectedItem == null)
        {
            _selectedIndex = -1;
            return;
        }

        _selectedIndex = IndexFromItem(SelectedItem);

        if (_selectedIndex >= 0)
            return;

        if (AllItems == null)
            return;
        
        List<object> allList = AllItems.Cast<object>().ToList();
        _selectedIndex = allList.FindIndex(x => Equals(x, SelectedItem));
    }
    
    private static bool DefaultFilterFunction(object? item, string? filterText)
    {
        if (item == null || string.IsNullOrEmpty(filterText)) return true;
        
        string itemText = item is SearchableComboBoxItem cbi
            ? cbi.Content?.ToString() ?? ""
            : item.ToString() ?? "";
        
        return itemText.Contains(filterText, StringComparison.OrdinalIgnoreCase);
    }
    
    private void UpdateFlowDirection()
    {
        if (SelectionBoxItem is not Rectangle { Fill: VisualBrush brush } rectangle)
            return;
        
        FlowDirection flowDirection = brush.Visual?.GetVisualParent()?.FlowDirection ?? FlowDirection.LeftToRight;
        rectangle.FlowDirection = flowDirection;
    }

    private void SelectFocusedItem()
    {
        foreach (Control container in GetRealizedContainers())
        {
            if (!container.IsFocused || container is not SearchableComboBoxItem)
                continue;
            
            int index = IndexFromContainer(container);
            if (index >= 0)
            {
                SelectedItem = Items.ElementAtOrDefault(index);
                SetCurrentValue(FilterTextProperty, string.Empty);
                EnsureAllItems();
            }
            break;
        }
    }

    private void TryFocusSelectedItem()
    {
        if (!IsDropDownOpen || SelectedIndex == -1) return;

        Control? container = ContainerFromIndex(SelectedIndex);
        if (container == null)
        {
            ScrollIntoView(SelectedIndex);
            container = ContainerFromIndex(SelectedIndex);
        }

        container?.Focus();
    }

    private void MoveFocus(int direction)
    {
        List<Control> containers = GetRealizedContainers().ToList();
        if (containers.Count == 0) return;

        int focusedIndex = containers.FindIndex(c => c.IsFocused);
        int newIndex = focusedIndex + direction;

        if (newIndex < 0) newIndex = containers.Count - 1;
        if (newIndex >= containers.Count) newIndex = 0;

        containers[newIndex].Focus();
    }
}