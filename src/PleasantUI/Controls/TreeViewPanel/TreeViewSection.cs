using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;

namespace PleasantUI.Controls;

/// <summary>
/// A collapsible section inside a <see cref="TreeViewPanel"/>.
/// Displays a header with an icon, title, and item count badge, and hosts a virtualized list of items.
/// </summary>
[TemplatePart(PART_ItemsList, typeof(ListBox))]
[PseudoClasses(PC_Loading, PC_Empty)]
public class TreeViewSection : HeaderedItemsControl
{
    // ── Template part names ───────────────────────────────────────────────────

    internal const string PART_ItemsList = "PART_ItemsList";

    // ── Pseudo-class names ────────────────────────────────────────────────────

    private const string PC_Loading = ":loading";
    private const string PC_Empty   = ":empty";

    // ── Styled properties ─────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="SectionIcon"/> property.</summary>
    public static readonly StyledProperty<Geometry?> SectionIconProperty =
        AvaloniaProperty.Register<TreeViewSection, Geometry?>(nameof(SectionIcon));

    /// <summary>Defines the <see cref="IconBackground"/> property.</summary>
    public static readonly StyledProperty<IBrush?> IconBackgroundProperty =
        AvaloniaProperty.Register<TreeViewSection, IBrush?>(nameof(IconBackground));

    /// <summary>Defines the <see cref="IconForeground"/> property.</summary>
    public static readonly StyledProperty<IBrush?> IconForegroundProperty =
        AvaloniaProperty.Register<TreeViewSection, IBrush?>(nameof(IconForeground));

    /// <summary>Defines the <see cref="IsExpanded"/> property.</summary>
    public static readonly StyledProperty<bool> IsExpandedProperty =
        Expander.IsExpandedProperty.AddOwner<TreeViewSection>(
            new StyledPropertyMetadata<bool>(defaultValue: true));

    /// <summary>Defines the <see cref="IsLoading"/> property.</summary>
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<TreeViewSection, bool>(nameof(IsLoading));

    /// <summary>Defines the <see cref="LoadingText"/> property.</summary>
    public static readonly StyledProperty<string> LoadingTextProperty =
        AvaloniaProperty.Register<TreeViewSection, string>(nameof(LoadingText), defaultValue: "Loading...");

    /// <summary>Defines the <see cref="SelectedItem"/> property.</summary>
    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<TreeViewSection, object?>(nameof(SelectedItem));

    /// <summary>Defines the <see cref="MaxListHeight"/> property.</summary>
    public static readonly StyledProperty<double> MaxListHeightProperty =
        AvaloniaProperty.Register<TreeViewSection, double>(nameof(MaxListHeight), defaultValue: double.PositiveInfinity);

    // ── CLR accessors ─────────────────────────────────────────────────────────

    /// <summary>Gets or sets the icon geometry shown in the section header.</summary>
    public Geometry? SectionIcon
    {
        get => GetValue(SectionIconProperty);
        set => SetValue(SectionIconProperty, value);
    }

    /// <summary>Gets or sets the background of the icon badge.</summary>
    public IBrush? IconBackground
    {
        get => GetValue(IconBackgroundProperty);
        set => SetValue(IconBackgroundProperty, value);
    }

    /// <summary>Gets or sets the foreground of the icon badge.</summary>
    public IBrush? IconForeground
    {
        get => GetValue(IconForegroundProperty);
        set => SetValue(IconForegroundProperty, value);
    }

    /// <summary>Gets or sets whether the section is expanded.</summary>
    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>Gets or sets whether the section is in a loading state.</summary>
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    /// <summary>Gets or sets the text shown while loading.</summary>
    public string LoadingText
    {
        get => GetValue(LoadingTextProperty);
        set => SetValue(LoadingTextProperty, value);
    }

    /// <summary>Gets or sets the currently selected item in this section's list.</summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>Gets or sets the maximum height of the items list.</summary>
    public double MaxListHeight
    {
        get => GetValue(MaxListHeightProperty);
        set => SetValue(MaxListHeightProperty, value);
    }

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Raised when the user selects an item in this section.</summary>
    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

    // ── Private state ─────────────────────────────────────────────────────────

    private ListBox? _listBox;

    // ── Template ──────────────────────────────────────────────────────────────

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_listBox is not null)
            _listBox.SelectionChanged -= OnListSelectionChanged;

        _listBox = e.NameScope.Find<ListBox>(PART_ItemsList);

        if (_listBox is not null)
        {
            _listBox.SelectionChanged += OnListSelectionChanged;
            SyncListSource();
        }

        UpdatePseudoClasses();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsLoadingProperty)
            UpdatePseudoClasses();
        else if (change.Property == ItemsSourceProperty || change.Property == ItemCountProperty)
        {
            SyncListSource();
            UpdatePseudoClasses();
        }
        else if (change.Property == SelectedItemProperty && _listBox is not null)
        {
            _listBox.SelectedItem = change.NewValue;
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void SyncListSource()
    {
        if (_listBox is null) return;
        _listBox.ItemsSource = ItemsSource as IEnumerable ?? Items;
        _listBox.ItemTemplate = ItemTemplate;
    }

    private void OnListSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_listBox is not null)
            SelectedItem = _listBox.SelectedItem;
        SelectionChanged?.Invoke(this, e);
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(PC_Loading, IsLoading);
        PseudoClasses.Set(PC_Empty,   !IsLoading && ItemCount == 0);
    }
}
