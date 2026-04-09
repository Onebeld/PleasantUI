using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Metadata;

namespace PleasantUI.Controls;

/// <summary>
/// A panel that hosts a searchable, collapsible tree of <see cref="TreeViewSection"/> items.
/// Provides a filter text box, a collapse-all button, and raises a unified
/// <see cref="SelectionChanged"/> event when any section's selection changes.
/// </summary>
[TemplatePart(PART_SearchBox,    typeof(TextBox))]
[TemplatePart(PART_SectionsHost, typeof(ItemsControl))]
[PseudoClasses(PC_HasFilter)]
public class TreeViewPanel : TemplatedControl
{
    // ── Template part names ───────────────────────────────────────────────────

    internal const string PART_SearchBox    = "PART_SearchBox";
    internal const string PART_SectionsHost = "PART_SectionsHost";

    // ── Pseudo-class names ────────────────────────────────────────────────────

    private const string PC_HasFilter = ":hasFilter";

    // ── Styled properties ─────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="FilterText"/> property.</summary>
    public static readonly StyledProperty<string?> FilterTextProperty =
        AvaloniaProperty.Register<TreeViewPanel, string?>(nameof(FilterText));

    /// <summary>Defines the <see cref="FilterWatermark"/> property.</summary>
    public static readonly StyledProperty<string> FilterWatermarkProperty =
        AvaloniaProperty.Register<TreeViewPanel, string>(nameof(FilterWatermark), defaultValue: "Filter...");

    /// <summary>Defines the <see cref="SelectedItem"/> property.</summary>
    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<TreeViewPanel, object?>(nameof(SelectedItem));

    /// <summary>Defines the <see cref="SelectedSection"/> property.</summary>
    public static readonly StyledProperty<TreeViewSection?> SelectedSectionProperty =
        AvaloniaProperty.Register<TreeViewPanel, TreeViewSection?>(nameof(SelectedSection));

    // ── Direct properties ─────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="Sections"/> direct property.</summary>
    public static readonly DirectProperty<TreeViewPanel, AvaloniaList<TreeViewSection>> SectionsProperty =
        AvaloniaProperty.RegisterDirect<TreeViewPanel, AvaloniaList<TreeViewSection>>(
            nameof(Sections), o => o.Sections);

    // ── CLR accessors ─────────────────────────────────────────────────────────

    /// <summary>Gets or sets the text used to filter items across all sections.</summary>
    public string? FilterText
    {
        get => GetValue(FilterTextProperty);
        set => SetValue(FilterTextProperty, value);
    }

    /// <summary>Gets or sets the watermark shown in the filter text box.</summary>
    public string FilterWatermark
    {
        get => GetValue(FilterWatermarkProperty);
        set => SetValue(FilterWatermarkProperty, value);
    }

    /// <summary>Gets or sets the currently selected item (across all sections).</summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>Gets or sets the section that owns the currently selected item.</summary>
    public TreeViewSection? SelectedSection
    {
        get => GetValue(SelectedSectionProperty);
        set => SetValue(SelectedSectionProperty, value);
    }

    /// <summary>Gets the collection of sections displayed in the panel.</summary>
    [Content]
    public AvaloniaList<TreeViewSection> Sections { get; } = new();

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Raised when the selected item changes in any section.</summary>
    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

    /// <summary>Raised when the filter text changes.</summary>
    public event EventHandler<string?>? FilterChanged;

    // ── Private state ─────────────────────────────────────────────────────────

    private TextBox?      _searchBox;
    private ItemsControl? _sectionsHost;

    // ── Constructor ───────────────────────────────────────────────────────────

    public TreeViewPanel()
    {
        Sections.CollectionChanged += OnSectionsChanged;
    }

    // ── Template ──────────────────────────────────────────────────────────────

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_searchBox is not null)
            _searchBox.TextChanged -= OnSearchTextChanged;

        _searchBox    = e.NameScope.Find<TextBox>(PART_SearchBox);
        _sectionsHost = e.NameScope.Find<ItemsControl>(PART_SectionsHost);

        if (_searchBox is not null)
            _searchBox.TextChanged += OnSearchTextChanged;

        if (_sectionsHost is not null)
            _sectionsHost.ItemsSource = Sections;

        // Wire existing sections.
        foreach (var section in Sections)
            WireSection(section);

        PseudoClasses.Set(PC_HasFilter, !string.IsNullOrEmpty(FilterText));
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == FilterTextProperty)
        {
            PseudoClasses.Set(PC_HasFilter, !string.IsNullOrEmpty(change.GetNewValue<string?>()));
            FilterChanged?.Invoke(this, change.GetNewValue<string?>());
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Expands all sections.</summary>
    public void ExpandAll()
    {
        foreach (var s in Sections) s.IsExpanded = true;
    }

    /// <summary>Collapses all sections.</summary>
    public void CollapseAll()
    {
        foreach (var s in Sections) s.IsExpanded = false;
    }

    /// <summary>Clears the filter text.</summary>
    public void ClearFilter() => FilterText = null;

    // ── Private helpers ───────────────────────────────────────────────────────

    private void OnSectionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
            foreach (TreeViewSection s in e.NewItems)
                WireSection(s);

        if (e.OldItems is not null)
            foreach (TreeViewSection s in e.OldItems)
                UnwireSection(s);

        if (_sectionsHost is not null)
            _sectionsHost.ItemsSource = Sections;
    }

    private void WireSection(TreeViewSection section)
        => section.SelectionChanged += OnSectionSelectionChanged;

    private void UnwireSection(TreeViewSection section)
        => section.SelectionChanged -= OnSectionSelectionChanged;

    private void OnSectionSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not TreeViewSection section) return;

        // Clear selection in all other sections.
        foreach (var s in Sections)
            if (!ReferenceEquals(s, section))
                s.SelectedItem = null;

        SelectedItem    = section.SelectedItem;
        SelectedSection = section.SelectedItem is not null ? section : null;

        SelectionChanged?.Invoke(this, e);
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        => FilterText = _searchBox?.Text;
}
