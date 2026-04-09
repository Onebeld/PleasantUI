using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;

namespace PleasantUI.Controls;

/// <summary>
/// A panel that displays a searchable, filterable, multi-selectable list of items
/// with an optional loading overlay, bulk-action toolbar, and pagination footer slot.
/// </summary>
[TemplatePart(PART_SearchBox,       typeof(TextBox))]
[TemplatePart(PART_ItemsList,       typeof(ListBox))]
[TemplatePart(PART_LoadingOverlay,  typeof(Border))]
[TemplatePart(PART_BulkActionBar,   typeof(Border))]
[TemplatePart(PART_FooterPresenter, typeof(ContentPresenter))]
[PseudoClasses(PC_Loading, PC_MultiSelect, PC_HasFilter, PC_HasItems, PC_HasBulkActions)]
public class ItemListPanel : TemplatedControl
{
    // ── Template part names ───────────────────────────────────────────────────

    internal const string PART_SearchBox       = "PART_SearchBox";
    internal const string PART_ItemsList       = "PART_ItemsList";
    internal const string PART_LoadingOverlay  = "PART_LoadingOverlay";
    internal const string PART_BulkActionBar   = "PART_BulkActionBar";
    internal const string PART_FooterPresenter = "PART_FooterPresenter";

    // ── Pseudo-class names ────────────────────────────────────────────────────

    private const string PC_Loading       = ":loading";
    private const string PC_MultiSelect   = ":multiSelect";
    private const string PC_HasFilter     = ":hasFilter";
    private const string PC_HasItems      = ":hasItems";
    private const string PC_HasBulkActions = ":hasBulkActions";

    // ── Styled properties ─────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="IsLoading"/> property.</summary>
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<ItemListPanel, bool>(nameof(IsLoading));

    /// <summary>Defines the <see cref="LoadingTitle"/> property.</summary>
    public static readonly StyledProperty<string> LoadingTitleProperty =
        AvaloniaProperty.Register<ItemListPanel, string>(nameof(LoadingTitle), defaultValue: "Loading...");

    /// <summary>Defines the <see cref="LoadingSubtitle"/> property.</summary>
    public static readonly StyledProperty<string?> LoadingSubtitleProperty =
        AvaloniaProperty.Register<ItemListPanel, string?>(nameof(LoadingSubtitle));

    /// <summary>Defines the <see cref="IsMultiSelectMode"/> property.</summary>
    public static readonly StyledProperty<bool> IsMultiSelectModeProperty =
        AvaloniaProperty.Register<ItemListPanel, bool>(nameof(IsMultiSelectMode));

    /// <summary>Defines the <see cref="SearchText"/> property.</summary>
    public static readonly StyledProperty<string?> SearchTextProperty =
        AvaloniaProperty.Register<ItemListPanel, string?>(nameof(SearchText));

    /// <summary>Defines the <see cref="SearchWatermark"/> property.</summary>
    public static readonly StyledProperty<string> SearchWatermarkProperty =
        AvaloniaProperty.Register<ItemListPanel, string>(nameof(SearchWatermark), defaultValue: "Search...");

    /// <summary>Defines the <see cref="ItemsSource"/> property.</summary>
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        ItemsControl.ItemsSourceProperty.AddOwner<ItemListPanel>();

    /// <summary>Defines the <see cref="ItemTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        ItemsControl.ItemTemplateProperty.AddOwner<ItemListPanel>();

    /// <summary>Defines the <see cref="SelectedItem"/> property.</summary>
    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<ItemListPanel, object?>(nameof(SelectedItem));

    /// <summary>Defines the <see cref="BulkActionsContent"/> property.</summary>
    public static readonly StyledProperty<object?> BulkActionsContentProperty =
        AvaloniaProperty.Register<ItemListPanel, object?>(nameof(BulkActionsContent));

    /// <summary>Defines the <see cref="FooterContent"/> property.</summary>
    public static readonly StyledProperty<object?> FooterContentProperty =
        AvaloniaProperty.Register<ItemListPanel, object?>(nameof(FooterContent));

    /// <summary>Defines the <see cref="EmptyStateTitle"/> property.</summary>
    public static readonly StyledProperty<string> EmptyStateTitleProperty =
        AvaloniaProperty.Register<ItemListPanel, string>(nameof(EmptyStateTitle), defaultValue: "No items");

    /// <summary>Defines the <see cref="EmptyStateSubtitle"/> property.</summary>
    public static readonly StyledProperty<string?> EmptyStateSubtitleProperty =
        AvaloniaProperty.Register<ItemListPanel, string?>(nameof(EmptyStateSubtitle));

    /// <summary>Defines the <see cref="SelectedCount"/> direct property.</summary>
    public static readonly DirectProperty<ItemListPanel, int> SelectedCountProperty =
        AvaloniaProperty.RegisterDirect<ItemListPanel, int>(nameof(SelectedCount), o => o.SelectedCount);

    // ── CLR accessors ─────────────────────────────────────────────────────────

    /// <summary>Gets or sets whether the loading overlay is shown.</summary>
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    /// <summary>Gets or sets the title shown in the loading overlay.</summary>
    public string LoadingTitle
    {
        get => GetValue(LoadingTitleProperty);
        set => SetValue(LoadingTitleProperty, value);
    }

    /// <summary>Gets or sets the subtitle shown in the loading overlay.</summary>
    public string? LoadingSubtitle
    {
        get => GetValue(LoadingSubtitleProperty);
        set => SetValue(LoadingSubtitleProperty, value);
    }

    /// <summary>Gets or sets whether multi-select mode is active.</summary>
    public bool IsMultiSelectMode
    {
        get => GetValue(IsMultiSelectModeProperty);
        set => SetValue(IsMultiSelectModeProperty, value);
    }

    /// <summary>Gets or sets the search/filter text.</summary>
    public string? SearchText
    {
        get => GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    /// <summary>Gets or sets the watermark shown in the search box.</summary>
    public string SearchWatermark
    {
        get => GetValue(SearchWatermarkProperty);
        set => SetValue(SearchWatermarkProperty, value);
    }

    /// <summary>Gets or sets the items source for the list.</summary>
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>Gets or sets the data template for list items.</summary>
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>Gets or sets the currently selected item.</summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>Gets or sets the content shown in the bulk-actions bar (visible in multi-select mode).</summary>
    public object? BulkActionsContent
    {
        get => GetValue(BulkActionsContentProperty);
        set => SetValue(BulkActionsContentProperty, value);
    }

    /// <summary>Gets or sets the content shown in the footer slot (e.g. pagination).</summary>
    public object? FooterContent
    {
        get => GetValue(FooterContentProperty);
        set => SetValue(FooterContentProperty, value);
    }

    /// <summary>Gets or sets the title shown in the empty state.</summary>
    public string EmptyStateTitle
    {
        get => GetValue(EmptyStateTitleProperty);
        set => SetValue(EmptyStateTitleProperty, value);
    }

    /// <summary>Gets or sets the subtitle shown in the empty state.</summary>
    public string? EmptyStateSubtitle
    {
        get => GetValue(EmptyStateSubtitleProperty);
        set => SetValue(EmptyStateSubtitleProperty, value);
    }

    /// <summary>Gets the number of selected items in multi-select mode.</summary>
    public int SelectedCount
    {
        get => _selectedCount;
        private set => SetAndRaise(SelectedCountProperty, ref _selectedCount, value);
    }

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Raised when the selected item changes.</summary>
    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

    /// <summary>Raised when the search text changes.</summary>
    public event EventHandler<string?>? SearchChanged;

    // ── Private state ─────────────────────────────────────────────────────────

    private TextBox?          _searchBox;
    private ListBox?          _listBox;
    private int               _selectedCount;

    // ── Template ──────────────────────────────────────────────────────────────

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_searchBox is not null) _searchBox.TextChanged -= OnSearchTextChanged;
        if (_listBox   is not null) _listBox.SelectionChanged -= OnListSelectionChanged;

        _searchBox = e.NameScope.Find<TextBox>(PART_SearchBox);
        _listBox   = e.NameScope.Find<ListBox>(PART_ItemsList);

        if (_searchBox is not null) _searchBox.TextChanged += OnSearchTextChanged;
        if (_listBox   is not null)
        {
            _listBox.SelectionChanged += OnListSelectionChanged;
            SyncList();
        }

        UpdatePseudoClasses();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsLoadingProperty)
            PseudoClasses.Set(PC_Loading, change.GetNewValue<bool>());
        else if (change.Property == IsMultiSelectModeProperty)
        {
            bool multi = change.GetNewValue<bool>();
            PseudoClasses.Set(PC_MultiSelect, multi);
            if (_listBox is not null)
                _listBox.SelectionMode = multi
                    ? SelectionMode.Multiple | SelectionMode.Toggle
                    : SelectionMode.Single;
        }
        else if (change.Property == SearchTextProperty)
        {
            PseudoClasses.Set(PC_HasFilter, !string.IsNullOrEmpty(change.GetNewValue<string?>()));
            SearchChanged?.Invoke(this, change.GetNewValue<string?>());
        }
        else if (change.Property == ItemsSourceProperty || change.Property == ItemTemplateProperty)
        {
            SyncList();
            UpdatePseudoClasses();
        }
        else if (change.Property == SelectedItemProperty && _listBox is not null)
        {
            _listBox.SelectedItem = change.NewValue;
        }
        else if (change.Property == BulkActionsContentProperty)
        {
            PseudoClasses.Set(PC_HasBulkActions, change.NewValue is not null);
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Gets all currently selected items (multi-select mode).</summary>
    public IList? GetSelectedItems() => _listBox?.SelectedItems;

    /// <summary>Selects all items.</summary>
    public void SelectAll() => _listBox?.SelectAll();

    /// <summary>Clears all selections.</summary>
    public void UnselectAll() => _listBox?.UnselectAll();

    /// <summary>Clears the search text.</summary>
    public void ClearSearch() => SearchText = null;

    // ── Private helpers ───────────────────────────────────────────────────────

    private void SyncList()
    {
        if (_listBox is null) return;
        _listBox.ItemsSource = ItemsSource;
        _listBox.ItemTemplate = ItemTemplate;
    }

    private void OnListSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        SelectedItem  = _listBox?.SelectedItem;
        SelectedCount = _listBox?.SelectedItems?.Count ?? 0;
        SelectionChanged?.Invoke(this, e);
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        => SearchText = _searchBox?.Text;

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(PC_Loading,     IsLoading);
        PseudoClasses.Set(PC_MultiSelect, IsMultiSelectMode);
        PseudoClasses.Set(PC_HasFilter,   !string.IsNullOrEmpty(SearchText));
        PseudoClasses.Set(PC_HasBulkActions, BulkActionsContent is not null);

        bool hasItems = false;
        if (ItemsSource is ICollection c) hasItems = c.Count > 0;
        else if (ItemsSource is not null) hasItems = ItemsSource.GetEnumerator().MoveNext();
        PseudoClasses.Set(PC_HasItems, hasItems);
    }
}
