using Avalonia;
using Avalonia.Animation;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using System.Collections.Specialized;
using System.Diagnostics;

namespace PleasantUI.Controls;

/// <summary>
/// A panel that hosts a searchable, collapsible tree of <see cref="TreeViewSection"/> items.
/// Provides a filter text box, a collapse-all button, and raises a unified
/// <see cref="SelectionChanged"/> event when any section's selection changes.
/// </summary>
[TemplatePart(PART_SearchBox,    typeof(TextBox))]
[TemplatePart(PART_SectionsHost, typeof(ItemsControl))]
[TemplatePart(PART_CollapseButton, typeof(Button))]
[TemplatePart(PART_ExpandButton, typeof(Button))]
[TemplatePart(PART_ClearButton, typeof(Button))]
[PseudoClasses(PC_HasFilter)]
public class TreeViewPanel : TemplatedControl
{
    // ── Template part names ───────────────────────────────────────────────────

    internal const string PART_SearchBox    = "PART_SearchBox";
    internal const string PART_SectionsHost = "PART_SectionsHost";
    internal const string PART_CollapseButton = "PART_CollapseButton";
    internal const string PART_ExpandButton = "PART_ExpandButton";
    internal const string PART_ClearButton = "PART_ClearButton";
    internal const string PART_SearchBoxBorder = "PART_SearchBoxBorder";
    internal const string PART_HeaderGrid = "PART_HeaderGrid";

    // ── Pseudo-class names ────────────────────────────────────────────────────

    private const string PC_HasFilter = ":hasFilter";
    private const string PC_SearchFocused = ":searchFocused";

    // ── Styled properties ─────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="SectionIcon"/> property.</summary>
    public static readonly StyledProperty<Geometry> SectionIconProperty =
        AvaloniaProperty.Register<TreeViewPanel, Geometry>(nameof(SectionIcon));

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

    /// <summary>Defines the <see cref="ShowExpandButton"/> property.</summary>
    public static readonly StyledProperty<bool> ShowExpandButtonProperty =
        AvaloniaProperty.Register<TreeViewPanel, bool>(nameof(ShowExpandButton), defaultValue: true);

    /// <summary>Defines the <see cref="ShowCollapseButton"/> property.</summary>
    public static readonly StyledProperty<bool> ShowCollapseButtonProperty =
        AvaloniaProperty.Register<TreeViewPanel, bool>(nameof(ShowCollapseButton), defaultValue: true);

    /// <summary>Defines the <see cref="ExpandButtonColumnWidth"/> property.</summary>
    public static readonly StyledProperty<GridLength> ExpandButtonColumnWidthProperty =
        AvaloniaProperty.Register<TreeViewPanel, GridLength>(nameof(ExpandButtonColumnWidth), defaultValue: new GridLength(32));

    /// <summary>Defines the <see cref="CollapseButtonColumnWidth"/> property.</summary>
    public static readonly StyledProperty<GridLength> CollapseButtonColumnWidthProperty =
        AvaloniaProperty.Register<TreeViewPanel, GridLength>(nameof(CollapseButtonColumnWidth), defaultValue: new GridLength(32));

    /// <summary>Defines the <see cref="SearchBoxCornerRadius"/> property.</summary>
    public static readonly StyledProperty<CornerRadius> SearchBoxCornerRadiusProperty =
        AvaloniaProperty.Register<TreeViewPanel, CornerRadius>(nameof(SearchBoxCornerRadius), defaultValue: new CornerRadius(4));

    // ── Direct properties ─────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="Sections"/> direct property.</summary>
    public static readonly DirectProperty<TreeViewPanel, AvaloniaList<TreeViewSection>> SectionsProperty =
        AvaloniaProperty.RegisterDirect<TreeViewPanel, AvaloniaList<TreeViewSection>>(
            nameof(Sections), o => o.Sections);

    // ── CLR accessors ─────────────────────────────────────────────────────────

    /// <summary>Gets or sets the icon used for sections.</summary>
    public Geometry SectionIcon
    {
        get => GetValue(SectionIconProperty);
        set => SetValue(SectionIconProperty, value);
    }

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

    /// <summary>Gets or sets whether the expand-all button is visible.</summary>
    public bool ShowExpandButton
    {
        get => GetValue(ShowExpandButtonProperty);
        set => SetValue(ShowExpandButtonProperty, value);
    }

    /// <summary>Gets or sets whether the collapse-all button is visible.</summary>
    public bool ShowCollapseButton
    {
        get => GetValue(ShowCollapseButtonProperty);
        set => SetValue(ShowCollapseButtonProperty, value);
    }

    /// <summary>Gets or sets the width of the expand button column.</summary>
    public GridLength ExpandButtonColumnWidth
    {
        get => GetValue(ExpandButtonColumnWidthProperty);
        set => SetValue(ExpandButtonColumnWidthProperty, value);
    }

    /// <summary>Gets or sets the width of the collapse button column.</summary>
    public GridLength CollapseButtonColumnWidth
    {
        get => GetValue(CollapseButtonColumnWidthProperty);
        set => SetValue(CollapseButtonColumnWidthProperty, value);
    }

    /// <summary>Gets or sets the corner radius of the search box border.</summary>
    public CornerRadius SearchBoxCornerRadius
    {
        get => GetValue(SearchBoxCornerRadiusProperty);
        set => SetValue(SearchBoxCornerRadiusProperty, value);
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
    private Button?       _collapseButton;
    private Button?       _expandButton;
    private Button?       _clearButton;
    private Border?       _searchBoxBorder;
    private Grid?         _headerGrid;
    private bool          _isAnimatingColumns;

    // ── Constructor ───────────────────────────────────────────────────────────

    public TreeViewPanel()
    {
        Sections.CollectionChanged += OnSectionsChanged;
    }

    // ── Template ──────────────────────────────────────────────────────────────

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        Debug.WriteLine("[TreeViewPanel] OnApplyTemplate - Applying template");

        if (_searchBox is not null)
        {
            _searchBox.TextChanged -= OnSearchTextChanged;
            _searchBox.GotFocus -= OnSearchBoxGotFocus;
            _searchBox.LostFocus -= OnSearchBoxLostFocus;
            Debug.WriteLine("[TreeViewPanel] OnApplyTemplate - Detached from existing search box");
        }
        if (_collapseButton is not null)
        {
            _collapseButton.Click -= OnCollapseButtonClick;
            Debug.WriteLine("[TreeViewPanel] OnApplyTemplate - Detached from existing collapse button");
        }
        if (_expandButton is not null)
        {
            _expandButton.Click -= OnExpandButtonClick;
            Debug.WriteLine("[TreeViewPanel] OnApplyTemplate - Detached from existing expand button");
        }
        if (_clearButton is not null)
        {
            _clearButton.Click -= OnClearButtonClick;
            Debug.WriteLine("[TreeViewPanel] OnApplyTemplate - Detached from existing clear button");
        }

        _searchBox    = e.NameScope.Find<TextBox>(PART_SearchBox);
        _sectionsHost = e.NameScope.Find<ItemsControl>(PART_SectionsHost);
        _collapseButton = e.NameScope.Find<Button>(PART_CollapseButton);
        _expandButton = e.NameScope.Find<Button>(PART_ExpandButton);
        _clearButton = e.NameScope.Find<Button>(PART_ClearButton);
        _searchBoxBorder = e.NameScope.Find<Border>(PART_SearchBoxBorder);
        _headerGrid = e.NameScope.Find<Grid>(PART_HeaderGrid);

        Debug.WriteLine($"[TreeViewPanel] OnApplyTemplate - Found template parts: SearchBox={_searchBox != null}, SectionsHost={_sectionsHost != null}, CollapseButton={_collapseButton != null}, ExpandButton={_expandButton != null}, ClearButton={_clearButton != null}, SearchBoxBorder={_searchBoxBorder != null}, HeaderGrid={_headerGrid != null}");

        if (_searchBox is not null)
        {
            _searchBox.TextChanged += OnSearchTextChanged;
            _searchBox.GotFocus += OnSearchBoxGotFocus;
            _searchBox.LostFocus += OnSearchBoxLostFocus;
            Debug.WriteLine("[TreeViewPanel] OnApplyTemplate - Attached to search box events");
        }
        if (_collapseButton is not null)
        {
            _collapseButton.Click += OnCollapseButtonClick;
            Debug.WriteLine("[TreeViewPanel] OnApplyTemplate - Attached to collapse button");
        }
        if (_expandButton is not null)
        {
            _expandButton.Click += OnExpandButtonClick;
            Debug.WriteLine("[TreeViewPanel] OnApplyTemplate - Attached to expand button");
        }
        if (_clearButton is not null)
        {
            _clearButton.Click += OnClearButtonClick;
            Debug.WriteLine("[TreeViewPanel] OnApplyTemplate - Attached to clear button");
        }

        if (_sectionsHost is not null)
        {
            _sectionsHost.ItemsSource = Sections;
            Debug.WriteLine("[TreeViewPanel] OnApplyTemplate - Set SectionsHost ItemsSource");
        }

        // Wire existing sections.
        foreach (var section in Sections)
        {
            WireSection(section);
            Debug.WriteLine($"[TreeViewPanel] OnApplyTemplate - Wired section: {section.Header}");
        }

        PseudoClasses.Set(PC_HasFilter, !string.IsNullOrEmpty(FilterText));
        Debug.WriteLine($"[TreeViewPanel] OnApplyTemplate - Set HasFilter pseudo-class: {!string.IsNullOrEmpty(FilterText)}");
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        Debug.WriteLine($"[TreeViewPanel] OnPropertyChanged - Property changed: {change.Property.Name}");

        if (change.Property == FilterTextProperty)
        {
            var newFilterText = change.GetNewValue<string?>();
            PseudoClasses.Set(PC_HasFilter, !string.IsNullOrEmpty(newFilterText));
            FilterChanged?.Invoke(this, newFilterText);

            Debug.WriteLine($"[TreeViewPanel] OnPropertyChanged - FilterText changed to: {newFilterText}");

            // Propagate filter text to all sections
            foreach (var section in Sections)
                section.FilterText = newFilterText;
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Expands all sections.</summary>
    public void ExpandAll()
    {
        Debug.WriteLine("[TreeViewPanel] ExpandAll - Expanding all sections");
        foreach (var s in Sections) s.IsExpanded = true;
    }

    /// <summary>Collapses all sections.</summary>
    public void CollapseAll()
    {
        Debug.WriteLine("[TreeViewPanel] CollapseAll - Collapsing all sections");
        foreach (var s in Sections) s.IsExpanded = false;
    }

    /// <summary>Clears the filter text.</summary>
    public void ClearFilter()
    {
        Debug.WriteLine("[TreeViewPanel] ClearFilter - Clearing filter text");
        FilterText = null;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void OnSectionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Debug.WriteLine($"[TreeViewPanel] OnSectionsChanged - Action: {e.Action}, NewItems: {e.NewItems?.Count ?? 0}, OldItems: {e.OldItems?.Count ?? 0}");

        if (e.NewItems is not null)
            foreach (TreeViewSection s in e.NewItems)
            {
                WireSection(s);
                Debug.WriteLine($"[TreeViewPanel] OnSectionsChanged - Wired new section: {s.Header}");
            }

        if (e.OldItems is not null)
            foreach (TreeViewSection s in e.OldItems)
            {
                UnwireSection(s);
                Debug.WriteLine($"[TreeViewPanel] OnSectionsChanged - Unwired old section: {s.Header}");
            }

        if (_sectionsHost is not null)
            _sectionsHost.ItemsSource = Sections;
    }

    private void WireSection(TreeViewSection section)
    {
        section.SelectionChanged += OnSectionSelectionChanged;
        Debug.WriteLine($"[TreeViewPanel] WireSection - Wired section: {section.Header}");
    }

    private void UnwireSection(TreeViewSection section)
    {
        section.SelectionChanged -= OnSectionSelectionChanged;
        Debug.WriteLine($"[TreeViewPanel] UnwireSection - Unwired section: {section.Header}");
    }

    private void OnSectionSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not TreeViewSection section) return;

        Debug.WriteLine($"[TreeViewPanel] OnSectionSelectionChanged - Section: {section.Header}, SelectedItem: {section.SelectedItem}");

        // Clear selection in all other sections.
        foreach (var s in Sections)
            if (!ReferenceEquals(s, section))
                s.SelectedItem = null;

        SelectedItem    = section.SelectedItem;
        SelectedSection = section.SelectedItem is not null ? section : null;

        SelectionChanged?.Invoke(this, e);
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        var text = _searchBox?.Text;
        Debug.WriteLine($"[TreeViewPanel] OnSearchTextChanged - Text: {text}");
        FilterText = text;
    }

    private Task AnimateGridLengthAsync(
        Action<double> setter, double from, double to,
        TimeSpan duration, CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource();
        var sw = System.Diagnostics.Stopwatch.StartNew();
        double totalMs = duration.TotalMilliseconds;

        Avalonia.Threading.DispatcherTimer.Run(() =>
        {
            if (ct.IsCancellationRequested) { tcs.TrySetCanceled(); return false; }
            double t = Math.Clamp(sw.Elapsed.TotalMilliseconds / totalMs, 0, 1);
            double ease = t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2;
            _isAnimatingColumns = true;
            setter(from + (to - from) * ease);
            _isAnimatingColumns = false;
            if (t >= 1.0) { tcs.TrySetResult(); return false; }
            return true;
        }, TimeSpan.FromMilliseconds(16));

        return tcs.Task;
    }

    private void OnSearchBoxGotFocus(object? sender, FocusChangedEventArgs e)
    {
        Debug.WriteLine("[TreeViewPanel] OnSearchBoxGotFocus - Search box got focus");

        PseudoClasses.Set(PC_SearchFocused, true);

        if (_expandButton is null || _collapseButton is null)
        {
            Debug.WriteLine("[TreeViewPanel] OnSearchBoxGotFocus - Template parts not found, skipping animation");
            return;
        }

        var buttonAnimation = new Animation
        {
            Duration = TimeSpan.FromSeconds(0.2),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0.0),
                    Setters =
                    {
                        new Setter { Property = Button.OpacityProperty, Value = 1.0 },
                        new Setter { Property = Button.WidthProperty, Value = 32.0 }
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1.0),
                    Setters =
                    {
                        new Setter { Property = Button.OpacityProperty, Value = 0.0 },
                        new Setter { Property = Button.WidthProperty, Value = 0.0 }
                    }
                }
            }
        };

        buttonAnimation.RunAsync(_expandButton);
        buttonAnimation.RunAsync(_collapseButton);

        double fromExpand  = ExpandButtonColumnWidth.Value;
        double fromCollapse = CollapseButtonColumnWidth.Value;

        Task.Delay(200).ContinueWith(_ =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                AnimateGridLengthAsync(
                    v => ExpandButtonColumnWidth = new GridLength(v),
                    fromExpand, 0, TimeSpan.FromSeconds(0.25));
                AnimateGridLengthAsync(
                    v => CollapseButtonColumnWidth = new GridLength(v),
                    fromCollapse, 0, TimeSpan.FromSeconds(0.25));
            });
        });
    }

    private void OnSearchBoxLostFocus(object? sender, FocusChangedEventArgs e)
    {
        Debug.WriteLine("[TreeViewPanel] OnSearchBoxLostFocus - Search box lost focus");

        PseudoClasses.Set(PC_SearchFocused, false);

        if (_expandButton is null || _collapseButton is null)
        {
            Debug.WriteLine("[TreeViewPanel] OnSearchBoxLostFocus - Template parts not found, skipping animation");
            return;
        }

        double fromExpand  = ExpandButtonColumnWidth.Value;
        double fromCollapse = CollapseButtonColumnWidth.Value;

        AnimateGridLengthAsync(
            v => ExpandButtonColumnWidth = new GridLength(v),
            fromExpand, 32, TimeSpan.FromSeconds(0.25));
        AnimateGridLengthAsync(
            v => CollapseButtonColumnWidth = new GridLength(v),
            fromCollapse, 32, TimeSpan.FromSeconds(0.25));

        Task.Delay(250).ContinueWith(_ =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                if (_expandButton is null || _collapseButton is null) return;

                var buttonAnimation = new Animation
                {
                    Duration = TimeSpan.FromSeconds(0.2),
                    FillMode = FillMode.Forward,
                    Children =
                    {
                        new KeyFrame
                        {
                            Cue = new Cue(0.0),
                            Setters =
                            {
                                new Setter { Property = Button.OpacityProperty, Value = 0.0 },
                                new Setter { Property = Button.WidthProperty, Value = 0.0 }
                            }
                        },
                        new KeyFrame
                        {
                            Cue = new Cue(1.0),
                            Setters =
                            {
                                new Setter { Property = Button.OpacityProperty, Value = 1.0 },
                                new Setter { Property = Button.WidthProperty, Value = 32.0 }
                            }
                        }
                    }
                };

                buttonAnimation.RunAsync(_expandButton);
                buttonAnimation.RunAsync(_collapseButton);
            });
        });
    }

    private void OnCollapseButtonClick(object? sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[TreeViewPanel] OnCollapseButtonClick - Collapse button clicked");
        CollapseAll();
    }

    private void OnExpandButtonClick(object? sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[TreeViewPanel] OnExpandButtonClick - Expand button clicked");
        ExpandAll();
    }

    private void OnClearButtonClick(object? sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[TreeViewPanel] OnClearButtonClick - Clear button clicked");
        FilterText = null;
        if (_searchBox is not null)
            _searchBox.Text = string.Empty;
    }
}
