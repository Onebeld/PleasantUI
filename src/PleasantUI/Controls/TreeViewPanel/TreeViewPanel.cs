using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.Styling;

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

        if (_searchBox is not null)
        {
            _searchBox.TextChanged -= OnSearchTextChanged;
            _searchBox.GotFocus -= OnSearchBoxGotFocus;
            _searchBox.LostFocus -= OnSearchBoxLostFocus;
        }
        if (_collapseButton is not null)
        {
            _collapseButton.Click -= OnCollapseButtonClick;
        }
        if (_expandButton is not null)
        {
            _expandButton.Click -= OnExpandButtonClick;
        }
        if (_clearButton is not null)
        {
            _clearButton.Click -= OnClearButtonClick;
        }

        _searchBox    = e.NameScope.Find<TextBox>(PART_SearchBox);
        _sectionsHost = e.NameScope.Find<ItemsControl>(PART_SectionsHost);
        _collapseButton = e.NameScope.Find<Button>(PART_CollapseButton);
        _expandButton = e.NameScope.Find<Button>(PART_ExpandButton);
        _clearButton = e.NameScope.Find<Button>(PART_ClearButton);
        _searchBoxBorder = e.NameScope.Find<Border>(PART_SearchBoxBorder);
        _headerGrid = e.NameScope.Find<Grid>(PART_HeaderGrid);

        if (_searchBox is not null)
        {
            _searchBox.TextChanged += OnSearchTextChanged;
            _searchBox.GotFocus += OnSearchBoxGotFocus;
            _searchBox.LostFocus += OnSearchBoxLostFocus;
        }
        if (_collapseButton is not null)
        {
            _collapseButton.Click += OnCollapseButtonClick;
        }
        if (_expandButton is not null)
        {
            _expandButton.Click += OnExpandButtonClick;
        }
        if (_clearButton is not null)
        {
            _clearButton.Click += OnClearButtonClick;
        }

        _sectionsHost?.ItemsSource = Sections;

        // Wire existing sections.
        foreach (TreeViewSection? section in Sections)
        {
            WireSection(section);
        }

        PseudoClasses.Set(PC_HasFilter, !string.IsNullOrEmpty(FilterText));
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == FilterTextProperty)
        {
            string? newFilterText = change.GetNewValue<string?>();
            PseudoClasses.Set(PC_HasFilter, !string.IsNullOrEmpty(newFilterText));
            FilterChanged?.Invoke(this, newFilterText);

            // Propagate filter text to all sections
            foreach (TreeViewSection? section in Sections)
                section.FilterText = newFilterText;
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Expands all sections.</summary>
    public void ExpandAll()
    {
        foreach (TreeViewSection? s in Sections) s.IsExpanded = true;
    }

    /// <summary>Collapses all sections.</summary>
    public void CollapseAll()
    {
        foreach (TreeViewSection? s in Sections) s.IsExpanded = false;
    }

    /// <summary>Clears the filter text.</summary>
    public void ClearFilter()
    {
        FilterText = null;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void OnSectionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
            foreach (TreeViewSection s in e.NewItems)
            {
                WireSection(s);
            }

        if (e.OldItems is not null)
            foreach (TreeViewSection s in e.OldItems)
            {
                UnwireSection(s);
            }

        _sectionsHost?.ItemsSource = Sections;
    }

    private void WireSection(TreeViewSection section)
    {
        section.SelectionChanged += OnSectionSelectionChanged;
    }

    private void UnwireSection(TreeViewSection section)
    {
        section.SelectionChanged -= OnSectionSelectionChanged;
    }

    private void OnSectionSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not TreeViewSection section) return;

        // Clear selection in all other sections.
        foreach (TreeViewSection? s in Sections)
            if (!ReferenceEquals(s, section))
                s.SelectedItem = null;

        SelectedItem    = section.SelectedItem;
        SelectedSection = section.SelectedItem is not null ? section : null;

        SelectionChanged?.Invoke(this, e);
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        string? text = _searchBox?.Text;
        FilterText = text;
    }

    private Task AnimateGridLengthAsync(
        Action<double> setter, double from, double to,
        TimeSpan duration, CancellationToken ct = default)
    {
        TaskCompletionSource tcs = new();
        Stopwatch sw = Stopwatch.StartNew();
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
        PseudoClasses.Set(PC_SearchFocused, true);

        if (_expandButton is null || _collapseButton is null)
            return;

        Animation buttonAnimation = new()
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
                        new Setter { Property = OpacityProperty, Value = 1.0 },
                        new Setter { Property = WidthProperty, Value = 32.0 }
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1.0),
                    Setters =
                    {
                        new Setter { Property = OpacityProperty, Value = 0.0 },
                        new Setter { Property = WidthProperty, Value = 0.0 }
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
        PseudoClasses.Set(PC_SearchFocused, false);

        if (_expandButton is null || _collapseButton is null)
        {
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

                Animation buttonAnimation = new()
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
                                new Setter { Property = OpacityProperty, Value = 0.0 },
                                new Setter { Property = WidthProperty, Value = 0.0 }
                            }
                        },
                        new KeyFrame
                        {
                            Cue = new Cue(1.0),
                            Setters =
                            {
                                new Setter { Property = OpacityProperty, Value = 1.0 },
                                new Setter { Property = WidthProperty, Value = 32.0 }
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
        CollapseAll();
    }

    private void OnExpandButtonClick(object? sender, RoutedEventArgs e)
    {
        ExpandAll();
    }

    private void OnClearButtonClick(object? sender, RoutedEventArgs e)
    {
        FilterText = null;
        _searchBox?.Text = string.Empty;
    }
}
