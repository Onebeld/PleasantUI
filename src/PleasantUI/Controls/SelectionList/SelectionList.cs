using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Metadata;
using Avalonia.Styling;

namespace PleasantUI.Controls;

/// <summary>
/// A multi-select list control with image, title, subtitle and timestamp support per item.
/// Supports horizontal and vertical orientations, a selection-count header, and an empty-state message.
/// </summary>
public class SelectionList : ListBox
{
    private static readonly FuncTemplate<Panel?> DefaultVerticalPanel =
        new(() => new StackPanel { Orientation = Orientation.Vertical });

    // ── Properties ───────────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="ImageMemberBinding"/> property.</summary>
    public static readonly StyledProperty<BindingBase?> ImageMemberBindingProperty =
        AvaloniaProperty.Register<SelectionList, BindingBase?>(nameof(ImageMemberBinding));

    /// <summary>Defines the <see cref="TitleMemberBinding"/> property.</summary>
    public static readonly StyledProperty<BindingBase?> TitleMemberBindingProperty =
        AvaloniaProperty.Register<SelectionList, BindingBase?>(nameof(TitleMemberBinding));

    /// <summary>Defines the <see cref="SubtitleMemberBinding"/> property.</summary>
    public static readonly StyledProperty<BindingBase?> SubtitleMemberBindingProperty =
        AvaloniaProperty.Register<SelectionList, BindingBase?>(nameof(SubtitleMemberBinding));

    /// <summary>Defines the <see cref="TimestampMemberBinding"/> property.</summary>
    public static readonly StyledProperty<BindingBase?> TimestampMemberBindingProperty =
        AvaloniaProperty.Register<SelectionList, BindingBase?>(nameof(TimestampMemberBinding));

    /// <summary>Defines the <see cref="EmptyMessage"/> property.</summary>
    public static readonly StyledProperty<string> EmptyMessageProperty =
        AvaloniaProperty.Register<SelectionList, string>(nameof(EmptyMessage), defaultValue: "No items found");

    /// <summary>Defines the <see cref="Orientation"/> property.</summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<SelectionList, Orientation>(nameof(Orientation),
            defaultValue: Orientation.Vertical);

    /// <summary>Defines the <see cref="ImageTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> ImageTemplateProperty =
        AvaloniaProperty.Register<SelectionList, IDataTemplate?>(nameof(ImageTemplate));

    // ── CLR accessors ────────────────────────────────────────────────────────

    /// <summary>Binding path for the image of each item when using <see cref="ItemsControl.ItemsSource"/>.</summary>
    [AssignBinding]
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public BindingBase? ImageMemberBinding
    {
        get => GetValue(ImageMemberBindingProperty);
        set => SetValue(ImageMemberBindingProperty, value);
    }

    /// <summary>Binding path for the title of each item.</summary>
    [AssignBinding]
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public BindingBase? TitleMemberBinding
    {
        get => GetValue(TitleMemberBindingProperty);
        set => SetValue(TitleMemberBindingProperty, value);
    }

    /// <summary>Binding path for the subtitle of each item.</summary>
    [AssignBinding]
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public BindingBase? SubtitleMemberBinding
    {
        get => GetValue(SubtitleMemberBindingProperty);
        set => SetValue(SubtitleMemberBindingProperty, value);
    }

    /// <summary>Binding path for the timestamp of each item.</summary>
    [AssignBinding]
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public BindingBase? TimestampMemberBinding
    {
        get => GetValue(TimestampMemberBindingProperty);
        set => SetValue(TimestampMemberBindingProperty, value);
    }

    /// <summary>Gets or sets the message shown when the list has no items.</summary>
    public string EmptyMessage
    {
        get => GetValue(EmptyMessageProperty);
        set => SetValue(EmptyMessageProperty, value);
    }

    /// <summary>Gets or sets the orientation of the items panel.</summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>Gets or sets a data template used to render the image area of each item.</summary>
    public IDataTemplate? ImageTemplate
    {
        get => GetValue(ImageTemplateProperty);
        set => SetValue(ImageTemplateProperty, value);
    }

    // ── Template parts ───────────────────────────────────────────────────────

    private TextBlock? _selectionCountText;
    private TextBlock? _emptyMessageText;

    // ── Static constructor ───────────────────────────────────────────────────

    static SelectionList()
    {
        ItemsPanelProperty.OverrideDefaultValue<SelectionList>(DefaultVerticalPanel);
        SelectionModeProperty.OverrideDefaultValue<SelectionList>(
            SelectionMode.Multiple | SelectionMode.Toggle);

        OrientationProperty.Changed.AddClassHandler<SelectionList, Orientation>(
            (s, e) => s.OnOrientationChanged(e.NewValue.Value));
    }

    /// <summary>
    /// Overrides the style key so Avalonia's theme lookup resolves
    /// <see cref="SelectionList"/> instead of the base <see cref="ListBox"/> type.
    /// This ensures our custom ControlTheme is applied rather than the ListBox theme.
    /// </summary>
    protected override Type StyleKeyOverride => typeof(SelectionList);

    /// <summary>
    /// Initializes a new instance of <see cref="SelectionList"/>.
    /// Explicitly resolves and applies the SelectionList and SelectionListItem control themes
    /// from application resources, since Avalonia's theme lookup uses the exact type key
    /// and would otherwise fall back to the base ListBox theme.
    /// </summary>
    public SelectionList()
    {
        // Apply themes once resources are available (after the control attaches to the tree)
        AttachedToVisualTree += OnAttachedToVisualTree;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        AttachedToVisualTree -= OnAttachedToVisualTree;

        // Resolve and apply SelectionList control theme
        if (!IsSet(ThemeProperty) &&
            this.TryFindResource(typeof(SelectionList), ActualThemeVariant, out var listTheme) &&
            listTheme is ControlTheme ct)
        {
            SetCurrentValue(ThemeProperty, ct);
        }

        // Resolve and apply SelectionListItem container theme
        if (!IsSet(ItemContainerThemeProperty) &&
            this.TryFindResource(typeof(SelectionListItem), ActualThemeVariant, out var itemTheme) &&
            itemTheme is ControlTheme itemCt)
        {
            SetCurrentValue(ItemContainerThemeProperty, itemCt);
        }
    }

    /// <inheritdoc />
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)    {
        recycleKey = null;
        return item is not SelectionListItem;
    }

    /// <inheritdoc />
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new SelectionListItem();

    /// <inheritdoc />
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);

        if (container is not SelectionListItem sli) return;

        if (ImageMemberBinding is not null)
            sli.Bind(SelectionListItem.ImageProperty, ImageMemberBinding);
        if (TitleMemberBinding is not null)
            sli.Bind(SelectionListItem.TitleProperty, TitleMemberBinding);
        if (SubtitleMemberBinding is not null)
            sli.Bind(SelectionListItem.SubtitleProperty, SubtitleMemberBinding);
        if (TimestampMemberBinding is not null)
            sli.Bind(SelectionListItem.TimestampProperty, TimestampMemberBinding);
        if (ImageTemplate is not null && !sli.IsSet(SelectionListItem.ImageTemplateProperty))
            sli.SetCurrentValue(SelectionListItem.ImageTemplateProperty, ImageTemplate);
    }

    // ── Template ─────────────────────────────────────────────────────────────

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _selectionCountText = e.NameScope.Find<TextBlock>("PART_SelectionCount");
        _emptyMessageText   = e.NameScope.Find<TextBlock>("PART_EmptyMessage");

        SelectionChanged -= OnSelectionChanged;
        SelectionChanged += OnSelectionChanged;

        UpdateEmptyState();
        UpdateSelectionCount();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsSourceProperty)
        {
            if (change.OldValue is INotifyCollectionChanged oldNcc)
                oldNcc.CollectionChanged -= OnItemsCollectionChanged;
            if (change.NewValue is INotifyCollectionChanged newNcc)
                newNcc.CollectionChanged += OnItemsCollectionChanged;

            UpdateEmptyState();
        }
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => UpdateEmptyState();

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        => UpdateSelectionCount();

    private void UpdateEmptyState()
    {
        if (_emptyMessageText is null) return;
        _emptyMessageText.IsVisible = !HasItems();
    }

    private void UpdateSelectionCount()
    {
        if (_selectionCountText is null) return;
        int count = SelectedItems?.Count ?? 0;
        _selectionCountText.IsVisible = count > 0;
        _selectionCountText.Text = $"Selected: {count}";
    }

    private bool HasItems()
    {
        if (ItemsSource is null) return ItemCount > 0;
        var e = ItemsSource.GetEnumerator();
        bool has = e.MoveNext();
        (e as IDisposable)?.Dispose();
        return has;
    }

    private void OnOrientationChanged(Orientation orientation)
    {
        SetCurrentValue(ItemsPanelProperty, new FuncTemplate<Panel?>(
            () => new StackPanel { Orientation = orientation }));
    }
}
