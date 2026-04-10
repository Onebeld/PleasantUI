using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace PleasantUI.ToolKit.Controls;

/// <summary>
/// A slide-in panel that displays a filterable, searchable log of <see cref="LogEntry"/> items.
/// Supports auto-scroll, level filtering, source filtering, copy, and clear operations.
/// </summary>
[TemplatePart(PART_CloseButton,    typeof(Button))]
[TemplatePart(PART_ClearButton,    typeof(Button))]
[TemplatePart(PART_CopyAllButton,  typeof(Button))]
[TemplatePart(PART_SearchBox,      typeof(TextBox))]
[TemplatePart(PART_LogList,        typeof(ListBox))]
[TemplatePart(PART_LevelFilter,    typeof(ComboBox))]
[TemplatePart(PART_SourceFilter,   typeof(ComboBox))]
[TemplatePart(PART_DebugToggle,    typeof(ToggleButton))]
[TemplatePart(PART_AutoScrollToggle, typeof(ToggleButton))]
[PseudoClasses(PC_Open, PC_HasEntries, PC_HasFilter)]
public class LogViewerPanel : TemplatedControl
{
    // ── Template part names ───────────────────────────────────────────────────

    internal const string PART_CloseButton      = "PART_CloseButton";
    internal const string PART_ClearButton       = "PART_ClearButton";
    internal const string PART_CopyAllButton     = "PART_CopyAllButton";
    internal const string PART_SearchBox         = "PART_SearchBox";
    internal const string PART_LogList           = "PART_LogList";
    internal const string PART_LevelFilter       = "PART_LevelFilter";
    internal const string PART_SourceFilter      = "PART_SourceFilter";
    internal const string PART_DebugToggle       = "PART_DebugToggle";
    internal const string PART_AutoScrollToggle  = "PART_AutoScrollToggle";

    // ── Pseudo-class names ────────────────────────────────────────────────────

    private const string PC_Open       = ":open";
    private const string PC_HasEntries = ":hasEntries";
    private const string PC_HasFilter  = ":hasFilter";

    // ── Styled properties ─────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="IsOpen"/> property.</summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<LogViewerPanel, bool>(nameof(IsOpen));

    /// <summary>Defines the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<LogViewerPanel, string>(nameof(Title), defaultValue: "Activity Log");

    /// <summary>Defines the <see cref="AutoScroll"/> property.</summary>
    public static readonly StyledProperty<bool> AutoScrollProperty =
        AvaloniaProperty.Register<LogViewerPanel, bool>(nameof(AutoScroll), defaultValue: true);

    /// <summary>Defines the <see cref="ShowDebugEntries"/> property.</summary>
    public static readonly StyledProperty<bool> ShowDebugEntriesProperty =
        AvaloniaProperty.Register<LogViewerPanel, bool>(nameof(ShowDebugEntries), defaultValue: false);

    /// <summary>Defines the <see cref="SearchText"/> property.</summary>
    public static readonly StyledProperty<string?> SearchTextProperty =
        AvaloniaProperty.Register<LogViewerPanel, string?>(nameof(SearchText));

    /// <summary>Defines the <see cref="SelectedLevelFilter"/> property.</summary>
    public static readonly StyledProperty<LogLevel?> SelectedLevelFilterProperty =
        AvaloniaProperty.Register<LogViewerPanel, LogLevel?>(nameof(SelectedLevelFilter));

    /// <summary>Defines the <see cref="SelectedSourceFilter"/> property.</summary>
    public static readonly StyledProperty<string?> SelectedSourceFilterProperty =
        AvaloniaProperty.Register<LogViewerPanel, string?>(nameof(SelectedSourceFilter));

    /// <summary>Defines the <see cref="MaxEntries"/> property.</summary>
    public static readonly StyledProperty<int> MaxEntriesProperty =
        AvaloniaProperty.Register<LogViewerPanel, int>(nameof(MaxEntries), defaultValue: 5000);

    /// <summary>Defines the <see cref="PanelWidth"/> property.</summary>
    public static readonly StyledProperty<double> PanelWidthProperty =
        AvaloniaProperty.Register<LogViewerPanel, double>(nameof(PanelWidth), defaultValue: 400);

    /// <summary>Defines the <see cref="AllFilterText"/> property.</summary>
    public static readonly StyledProperty<string> AllFilterTextProperty =
        AvaloniaProperty.Register<LogViewerPanel, string>(nameof(AllFilterText), defaultValue: "All");

    /// <summary>Defines the <see cref="AutoScrollTooltip"/> property.</summary>
    public static readonly StyledProperty<string> AutoScrollTooltipProperty =
        AvaloniaProperty.Register<LogViewerPanel, string>(nameof(AutoScrollTooltip), defaultValue: "Auto-scroll");

    /// <summary>Defines the <see cref="CopyAllTooltip"/> property.</summary>
    public static readonly StyledProperty<string> CopyAllTooltipProperty =
        AvaloniaProperty.Register<LogViewerPanel, string>(nameof(CopyAllTooltip), defaultValue: "Copy all");

    /// <summary>Defines the <see cref="ClearTooltip"/> property.</summary>
    public static readonly StyledProperty<string> ClearTooltipProperty =
        AvaloniaProperty.Register<LogViewerPanel, string>(nameof(ClearTooltip), defaultValue: "Clear");

    /// <summary>Defines the <see cref="CloseTooltip"/> property.</summary>
    public static readonly StyledProperty<string> CloseTooltipProperty =
        AvaloniaProperty.Register<LogViewerPanel, string>(nameof(CloseTooltip), defaultValue: "Close");

    /// <summary>Defines the <see cref="DebugTooltip"/> property.</summary>
    public static readonly StyledProperty<string> DebugTooltipProperty =
        AvaloniaProperty.Register<LogViewerPanel, string>(nameof(DebugTooltip), defaultValue: "Show debug entries");

    // ── Direct properties ─────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="Entries"/> direct property.</summary>
    public static readonly DirectProperty<LogViewerPanel, ObservableCollection<LogEntry>> EntriesProperty =
        AvaloniaProperty.RegisterDirect<LogViewerPanel, ObservableCollection<LogEntry>>(
            nameof(Entries), o => o.Entries);

    /// <summary>Defines the <see cref="FilteredEntries"/> direct property.</summary>
    public static readonly DirectProperty<LogViewerPanel, AvaloniaList<LogEntry>> FilteredEntriesProperty =
        AvaloniaProperty.RegisterDirect<LogViewerPanel, AvaloniaList<LogEntry>>(
            nameof(FilteredEntries), o => o.FilteredEntries);

    // ── CLR accessors ─────────────────────────────────────────────────────────

    /// <summary>Gets or sets whether the panel is visible.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Gets or sets the panel title.</summary>
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets or sets whether the list auto-scrolls to new entries.</summary>
    public bool AutoScroll
    {
        get => GetValue(AutoScrollProperty);
        set => SetValue(AutoScrollProperty, value);
    }

    /// <summary>Gets or sets whether Debug-level entries are shown.</summary>
    public bool ShowDebugEntries
    {
        get => GetValue(ShowDebugEntriesProperty);
        set => SetValue(ShowDebugEntriesProperty, value);
    }

    /// <summary>Gets or sets the text used to filter entries by message content.</summary>
    public string? SearchText
    {
        get => GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    /// <summary>Gets or sets the level filter. Null means show all levels.</summary>
    public LogLevel? SelectedLevelFilter
    {
        get => GetValue(SelectedLevelFilterProperty);
        set => SetValue(SelectedLevelFilterProperty, value);
    }

    /// <summary>Gets or sets the source filter. Null means show all sources.</summary>
    public string? SelectedSourceFilter
    {
        get => GetValue(SelectedSourceFilterProperty);
        set => SetValue(SelectedSourceFilterProperty, value);
    }

    /// <summary>Gets or sets the maximum number of entries to retain.</summary>
    public int MaxEntries
    {
        get => GetValue(MaxEntriesProperty);
        set => SetValue(MaxEntriesProperty, value);
    }

    /// <summary>Gets or sets the width of the slide-in panel.</summary>
    public double PanelWidth
    {
        get => GetValue(PanelWidthProperty);
        set => SetValue(PanelWidthProperty, value);
    }

    /// <summary>Gets or sets the text displayed for the "All" filter option.</summary>
    public string AllFilterText
    {
        get => GetValue(AllFilterTextProperty);
        set => SetValue(AllFilterTextProperty, value);
    }

    /// <summary>Gets or sets the tooltip text for the auto-scroll toggle.</summary>
    public string AutoScrollTooltip
    {
        get => GetValue(AutoScrollTooltipProperty);
        set => SetValue(AutoScrollTooltipProperty, value);
    }

    /// <summary>Gets or sets the tooltip text for the copy all button.</summary>
    public string CopyAllTooltip
    {
        get => GetValue(CopyAllTooltipProperty);
        set => SetValue(CopyAllTooltipProperty, value);
    }

    /// <summary>Gets or sets the tooltip text for the clear button.</summary>
    public string ClearTooltip
    {
        get => GetValue(ClearTooltipProperty);
        set => SetValue(ClearTooltipProperty, value);
    }

    /// <summary>Gets or sets the tooltip text for the close button.</summary>
    public string CloseTooltip
    {
        get => GetValue(CloseTooltipProperty);
        set => SetValue(CloseTooltipProperty, value);
    }

    /// <summary>Gets or sets the tooltip text for the debug toggle.</summary>
    public string DebugTooltip
    {
        get => GetValue(DebugTooltipProperty);
        set => SetValue(DebugTooltipProperty, value);
    }

    /// <summary>Gets the full collection of log entries.</summary>
    public ObservableCollection<LogEntry> Entries { get; } = new();

    /// <summary>Gets the filtered view of entries shown in the list.</summary>
    public AvaloniaList<LogEntry> FilteredEntries { get; } = new();

    /// <summary>Gets the available log level filter options (null = All).</summary>
    public AvaloniaList<LogLevel?> LevelFilterOptions { get; } = new()
    {
        null,
        LogLevel.Debug,
        LogLevel.Information,
        LogLevel.Warning,
        LogLevel.Error
    };

    /// <summary>Gets the available source filter options (null = All).</summary>
    public AvaloniaList<string?> SourceFilterOptions { get; } = new() { null };

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Raised when the panel is closed.</summary>
    public event EventHandler? Closed;

    /// <summary>Raised when the user requests all entries to be copied.</summary>
    public event EventHandler? CopyAllRequested;

    /// <summary>Raised when the user requests a single entry to be copied.</summary>
    public event EventHandler<LogEntry>? CopyEntryRequested;

    // ── Private state ─────────────────────────────────────────────────────────

    private Button?       _closeButton;
    private Button?       _clearButton;
    private Button?       _copyAllButton;
    private TextBox?      _searchBox;
    private ListBox?      _logList;
    private ComboBox?     _levelFilter;
    private ComboBox?     _sourceFilter;
    private ToggleButton? _debugToggle;
    private ToggleButton? _autoScrollToggle;

    // ── Constructor ───────────────────────────────────────────────────────────

    public LogViewerPanel()
    {
        Entries.CollectionChanged += OnEntriesChanged;
    }

    // ── Template ──────────────────────────────────────────────────────────────

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        DetachHandlers();

        _closeButton     = e.NameScope.Find<Button>(PART_CloseButton);
        _clearButton     = e.NameScope.Find<Button>(PART_ClearButton);
        _copyAllButton   = e.NameScope.Find<Button>(PART_CopyAllButton);
        _searchBox       = e.NameScope.Find<TextBox>(PART_SearchBox);
        _logList         = e.NameScope.Find<ListBox>(PART_LogList);
        _levelFilter     = e.NameScope.Find<ComboBox>(PART_LevelFilter);
        _sourceFilter    = e.NameScope.Find<ComboBox>(PART_SourceFilter);
        _debugToggle     = e.NameScope.Find<ToggleButton>(PART_DebugToggle);
        _autoScrollToggle = e.NameScope.Find<ToggleButton>(PART_AutoScrollToggle);

        AttachHandlers();

        if (_logList is not null)
            _logList.ItemsSource = FilteredEntries;

        if (_levelFilter is not null)
        {
            _levelFilter.ItemsSource = LevelFilterOptions;
            _levelFilter.SelectedIndex = 0;

            // Display "All" for null values
            _levelFilter.ItemTemplate = new FuncDataTemplate<LogLevel?>((level, _) =>
                new TextBlock { Text = level is null ? AllFilterText : level.ToString() });
        }

        if (_sourceFilter is not null)
        {
            _sourceFilter.ItemsSource = SourceFilterOptions;
            _sourceFilter.SelectedIndex = 0;

            // Display "All" for null values
            _sourceFilter.ItemTemplate = new FuncDataTemplate<string?>((source, _) =>
                new TextBlock { Text = source is null ? AllFilterText : source });
        }

        RefreshFilter();
        UpdatePseudoClasses();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsOpenProperty)
            PseudoClasses.Set(PC_Open, change.GetNewValue<bool>());
        else if (change.Property == SearchTextProperty ||
                 change.Property == SelectedLevelFilterProperty ||
                 change.Property == SelectedSourceFilterProperty ||
                 change.Property == ShowDebugEntriesProperty)
        {
            RefreshFilter();
            UpdatePseudoClasses();
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Appends a new entry and optionally scrolls to it.</summary>
    public void Append(LogEntry entry)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => Append(entry));
            return;
        }

        // Trim if over limit.
        while (Entries.Count >= MaxEntries)
            Entries.RemoveAt(0);

        Entries.Add(entry);
    }

    /// <summary>Appends a simple message at the given level.</summary>
    public void Append(LogLevel level, string message, string? source = null, string? details = null)
        => Append(new LogEntry { Level = level, Message = message, Source = source, Details = details });

    /// <summary>Removes all entries.</summary>
    public void Clear()
    {
        Entries.Clear();
        FilteredEntries.Clear();
        UpdatePseudoClasses();
    }

    /// <summary>Opens the panel.</summary>
    public void Open() => IsOpen = true;

    /// <summary>Closes the panel.</summary>
    public void Close()
    {
        IsOpen = false;
        Closed?.Invoke(this, EventArgs.Empty);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void OnEntriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems is not null)
                    foreach (LogEntry entry in e.NewItems)
                    {
                        if (MatchesFilter(entry))
                        {
                            FilteredEntries.Add(entry);
                            UpdateSourceOptions(entry.Source);
                        }
                    }
                break;

            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems is not null)
                    foreach (LogEntry entry in e.OldItems)
                        FilteredEntries.Remove(entry);
                break;

            case NotifyCollectionChangedAction.Reset:
                FilteredEntries.Clear();
                SourceFilterOptions.Clear();
                SourceFilterOptions.Add(null);
                break;
        }

        UpdatePseudoClasses();

        if (AutoScroll && FilteredEntries.Count > 0 && _logList is not null)
            _logList.ScrollIntoView(FilteredEntries[FilteredEntries.Count - 1]);
    }

    private void RefreshFilter()
    {
        FilteredEntries.Clear();
        foreach (var entry in Entries)
            if (MatchesFilter(entry))
                FilteredEntries.Add(entry);
    }

    private bool MatchesFilter(LogEntry entry)
    {
        if (!ShowDebugEntries && entry.Level == LogLevel.Debug)
            return false;

        if (SelectedLevelFilter.HasValue && entry.Level != SelectedLevelFilter.Value)
            return false;

        if (SelectedSourceFilter is not null && entry.Source != SelectedSourceFilter)
            return false;

        if (!string.IsNullOrEmpty(SearchText) &&
            !entry.Message.Contains(SearchText, StringComparison.OrdinalIgnoreCase) &&
            !(entry.Source?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false))
            return false;

        return true;
    }

    private void UpdateSourceOptions(string? source)
    {
        if (source is not null && !SourceFilterOptions.Contains(source))
            SourceFilterOptions.Add(source);
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(PC_HasEntries, FilteredEntries.Count > 0);
        PseudoClasses.Set(PC_HasFilter,
            !string.IsNullOrEmpty(SearchText) ||
            SelectedLevelFilter.HasValue ||
            SelectedSourceFilter is not null);
    }

    private void AttachHandlers()
    {
        if (_closeButton   is not null) _closeButton.Click   += OnCloseClicked;
        if (_clearButton   is not null) _clearButton.Click   += OnClearClicked;
        if (_copyAllButton is not null) _copyAllButton.Click += OnCopyAllClicked;

        if (_searchBox is not null)
            _searchBox.TextChanged += (_, _) => SearchText = _searchBox.Text;

        if (_levelFilter is not null)
            _levelFilter.SelectionChanged += OnLevelFilterChanged;

        if (_sourceFilter is not null)
            _sourceFilter.SelectionChanged += OnSourceFilterChanged;

        if (_debugToggle is not null)
            _debugToggle.IsCheckedChanged += OnDebugToggleChanged;

        if (_autoScrollToggle is not null)
            _autoScrollToggle.IsCheckedChanged += OnAutoScrollToggleChanged;
    }

    private void DetachHandlers()
    {
        if (_closeButton   is not null) _closeButton.Click   -= OnCloseClicked;
        if (_clearButton   is not null) _clearButton.Click   -= OnClearClicked;
        if (_copyAllButton is not null) _copyAllButton.Click -= OnCopyAllClicked;
        if (_levelFilter   is not null) _levelFilter.SelectionChanged  -= OnLevelFilterChanged;
        if (_sourceFilter  is not null) _sourceFilter.SelectionChanged -= OnSourceFilterChanged;
        if (_debugToggle   is not null) _debugToggle.IsCheckedChanged  -= OnDebugToggleChanged;
        if (_autoScrollToggle is not null) _autoScrollToggle.IsCheckedChanged -= OnAutoScrollToggleChanged;
    }

    private void OnCloseClicked(object? s, RoutedEventArgs e)   => Close();
    private void OnClearClicked(object? s, RoutedEventArgs e)   => Clear();
    private void OnCopyAllClicked(object? s, RoutedEventArgs e) => CopyAllRequested?.Invoke(this, EventArgs.Empty);

    private void OnLevelFilterChanged(object? s, SelectionChangedEventArgs e)
    {
        if (_levelFilter?.SelectedItem is LogLevel level)
            SelectedLevelFilter = level;
        else
            SelectedLevelFilter = null;
    }

    private void OnSourceFilterChanged(object? s, SelectionChangedEventArgs e)
        => SelectedSourceFilter = _sourceFilter?.SelectedItem as string;

    private void OnDebugToggleChanged(object? s, RoutedEventArgs e)
        => ShowDebugEntries = _debugToggle?.IsChecked == true;

    private void OnAutoScrollToggleChanged(object? s, RoutedEventArgs e)
        => AutoScroll = _autoScrollToggle?.IsChecked == true;
}
