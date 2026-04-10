using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace PleasantUI.ToolKit.Controls;

/// <summary>
/// A panel that visualizes a multi-chunk download operation.
/// Provides a tab strip, a progress bar, a chunk-progress canvas,
/// a chunk details list, and action buttons (pause/resume, cancel).
/// </summary>
[TemplatePart(PART_TabStrip,        typeof(ListBox))]
[TemplatePart(PART_TabContent,      typeof(ContentPresenter))]
[TemplatePart(PART_ProgressBar,     typeof(ProgressBar))]
[TemplatePart(PART_ChunkCanvas,     typeof(Canvas))]
[TemplatePart(PART_ChunkList,       typeof(ListBox))]
[TemplatePart(PART_PauseButton,     typeof(Button))]
[TemplatePart(PART_CancelButton,    typeof(Button))]
[TemplatePart(PART_DetailsToggle,   typeof(Button))]
[PseudoClasses(PC_Paused, PC_Merging, PC_ShowDetails, PC_HasChunks)]
public class DownloadPanel : TemplatedControl
{
    // ── Template part names ───────────────────────────────────────────────────

    internal const string PART_TabStrip      = "PART_TabStrip";
    internal const string PART_TabContent    = "PART_TabContent";
    internal const string PART_ProgressBar   = "PART_ProgressBar";
    internal const string PART_ChunkCanvas   = "PART_ChunkCanvas";
    internal const string PART_ChunkList     = "PART_ChunkList";
    internal const string PART_PauseButton   = "PART_PauseButton";
    internal const string PART_CancelButton  = "PART_CancelButton";
    internal const string PART_DetailsToggle = "PART_DetailsToggle";

    // ── Pseudo-class names ────────────────────────────────────────────────────

    private const string PC_Paused      = ":paused";
    private const string PC_Merging     = ":merging";
    private const string PC_ShowDetails = ":showDetails";
    private const string PC_HasChunks   = ":hasChunks";

    // ── Styled properties ─────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="Progress"/> property.</summary>
    public static readonly StyledProperty<double> ProgressProperty =
        AvaloniaProperty.Register<DownloadPanel, double>(nameof(Progress));

    /// <summary>Defines the <see cref="MergeProgress"/> property.</summary>
    public static readonly StyledProperty<double> MergeProgressProperty =
        AvaloniaProperty.Register<DownloadPanel, double>(nameof(MergeProgress));

    /// <summary>Defines the <see cref="IsPaused"/> property.</summary>
    public static readonly StyledProperty<bool> IsPausedProperty =
        AvaloniaProperty.Register<DownloadPanel, bool>(nameof(IsPaused));

    /// <summary>Defines the <see cref="IsMerging"/> property.</summary>
    public static readonly StyledProperty<bool> IsMergingProperty =
        AvaloniaProperty.Register<DownloadPanel, bool>(nameof(IsMerging));

    /// <summary>Defines the <see cref="ShowDetails"/> property.</summary>
    public static readonly StyledProperty<bool> ShowDetailsProperty =
        AvaloniaProperty.Register<DownloadPanel, bool>(nameof(ShowDetails));

    /// <summary>Defines the <see cref="IsPauseResumeEnabled"/> property.</summary>
    public static readonly StyledProperty<bool> IsPauseResumeEnabledProperty =
        AvaloniaProperty.Register<DownloadPanel, bool>(nameof(IsPauseResumeEnabled), defaultValue: true);

    /// <summary>Defines the <see cref="PauseResumeCommand"/> property.</summary>
    public static readonly StyledProperty<ICommand?> PauseResumeCommandProperty =
        AvaloniaProperty.Register<DownloadPanel, ICommand?>(nameof(PauseResumeCommand));

    /// <summary>Defines the <see cref="CancelCommand"/> property.</summary>
    public static readonly StyledProperty<ICommand?> CancelCommandProperty =
        AvaloniaProperty.Register<DownloadPanel, ICommand?>(nameof(CancelCommand));

    /// <summary>Defines the <see cref="ChunkProgressBrush"/> property.</summary>
    public static readonly StyledProperty<IBrush?> ChunkProgressBrushProperty =
        AvaloniaProperty.Register<DownloadPanel, IBrush?>(nameof(ChunkProgressBrush));

    /// <summary>Defines the <see cref="ChunkBackgroundBrush"/> property.</summary>
    public static readonly StyledProperty<IBrush?> ChunkBackgroundBrushProperty =
        AvaloniaProperty.Register<DownloadPanel, IBrush?>(nameof(ChunkBackgroundBrush));

    /// <summary>Defines the <see cref="ChunkItemTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> ChunkItemTemplateProperty =
        AvaloniaProperty.Register<DownloadPanel, IDataTemplate?>(nameof(ChunkItemTemplate));

    /// <summary>Defines the <see cref="TabItemTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> TabItemTemplateProperty =
        AvaloniaProperty.Register<DownloadPanel, IDataTemplate?>(nameof(TabItemTemplate));

    /// <summary>Defines the <see cref="SelectedTab"/> property.</summary>
    public static readonly StyledProperty<object?> SelectedTabProperty =
        AvaloniaProperty.Register<DownloadPanel, object?>(nameof(SelectedTab));

    /// <summary>Defines the <see cref="TabContent"/> property.</summary>
    public static readonly StyledProperty<object?> TabContentProperty =
        AvaloniaProperty.Register<DownloadPanel, object?>(nameof(TabContent));

    /// <summary>Defines the <see cref="SizeLabel"/> property.</summary>
    public static readonly StyledProperty<string> SizeLabelProperty =
        AvaloniaProperty.Register<DownloadPanel, string>(nameof(SizeLabel), defaultValue: "SIZE");

    /// <summary>Defines the <see cref="SpeedLabel"/> property.</summary>
    public static readonly StyledProperty<string> SpeedLabelProperty =
        AvaloniaProperty.Register<DownloadPanel, string>(nameof(SpeedLabel), defaultValue: "SPEED");

    /// <summary>Defines the <see cref="RemainingLabel"/> property.</summary>
    public static readonly StyledProperty<string> RemainingLabelProperty =
        AvaloniaProperty.Register<DownloadPanel, string>(nameof(RemainingLabel), defaultValue: "REMAINING");

    /// <summary>Defines the <see cref="ShowDetailsLabel"/> property.</summary>
    public static readonly StyledProperty<string> ShowDetailsLabelProperty =
        AvaloniaProperty.Register<DownloadPanel, string>(nameof(ShowDetailsLabel), defaultValue: "Show details");

    /// <summary>Defines the <see cref="HideDetailsLabel"/> property.</summary>
    public static readonly StyledProperty<string> HideDetailsLabelProperty =
        AvaloniaProperty.Register<DownloadPanel, string>(nameof(HideDetailsLabel), defaultValue: "Hide details");

    /// <summary>Defines the <see cref="PauseLabel"/> property.</summary>
    public static readonly StyledProperty<string> PauseLabelProperty =
        AvaloniaProperty.Register<DownloadPanel, string>(nameof(PauseLabel), defaultValue: "Pause");

    /// <summary>Defines the <see cref="ResumeLabel"/> property.</summary>
    public static readonly StyledProperty<string> ResumeLabelProperty =
        AvaloniaProperty.Register<DownloadPanel, string>(nameof(ResumeLabel), defaultValue: "Resume");

    /// <summary>Defines the <see cref="CancelLabel"/> property.</summary>
    public static readonly StyledProperty<string> CancelLabelProperty =
        AvaloniaProperty.Register<DownloadPanel, string>(nameof(CancelLabel), defaultValue: "Cancel");

    // ── Direct properties ─────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="Tabs"/> direct property.</summary>
    public static readonly DirectProperty<DownloadPanel, AvaloniaList<object>> TabsProperty =
        AvaloniaProperty.RegisterDirect<DownloadPanel, AvaloniaList<object>>(
            nameof(Tabs), o => o.Tabs);

    /// <summary>Defines the <see cref="Chunks"/> direct property.</summary>
    public static readonly DirectProperty<DownloadPanel, AvaloniaList<ChunkInfo>> ChunksProperty =
        AvaloniaProperty.RegisterDirect<DownloadPanel, AvaloniaList<ChunkInfo>>(
            nameof(Chunks), o => o.Chunks);

    // ── CLR accessors ─────────────────────────────────────────────────────────

    /// <summary>Gets or sets the overall download progress [0..100].</summary>
    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    /// <summary>Gets or sets the merge/post-processing progress [0..100].</summary>
    public double MergeProgress
    {
        get => GetValue(MergeProgressProperty);
        set => SetValue(MergeProgressProperty, value);
    }

    /// <summary>Gets or sets whether the download is paused.</summary>
    public bool IsPaused
    {
        get => GetValue(IsPausedProperty);
        set => SetValue(IsPausedProperty, value);
    }

    /// <summary>Gets or sets whether the download is in the merge/finalize phase.</summary>
    public bool IsMerging
    {
        get => GetValue(IsMergingProperty);
        set => SetValue(IsMergingProperty, value);
    }

    /// <summary>Gets or sets whether the chunk details section is expanded.</summary>
    public bool ShowDetails
    {
        get => GetValue(ShowDetailsProperty);
        set => SetValue(ShowDetailsProperty, value);
    }

    /// <summary>Gets or sets whether the pause/resume button is enabled.</summary>
    public bool IsPauseResumeEnabled
    {
        get => GetValue(IsPauseResumeEnabledProperty);
        set => SetValue(IsPauseResumeEnabledProperty, value);
    }

    /// <summary>Gets or sets the command invoked when the pause/resume button is clicked.</summary>
    public ICommand? PauseResumeCommand
    {
        get => GetValue(PauseResumeCommandProperty);
        set => SetValue(PauseResumeCommandProperty, value);
    }

    /// <summary>Gets or sets the command invoked when the cancel button is clicked.</summary>
    public ICommand? CancelCommand
    {
        get => GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }

    /// <summary>Gets or sets the brush used to fill chunk progress rectangles.</summary>
    public IBrush? ChunkProgressBrush
    {
        get => GetValue(ChunkProgressBrushProperty);
        set => SetValue(ChunkProgressBrushProperty, value);
    }

    /// <summary>Gets or sets the background brush of the chunk canvas.</summary>
    public IBrush? ChunkBackgroundBrush
    {
        get => GetValue(ChunkBackgroundBrushProperty);
        set => SetValue(ChunkBackgroundBrushProperty, value);
    }

    /// <summary>Gets or sets the data template for chunk list items.</summary>
    public IDataTemplate? ChunkItemTemplate
    {
        get => GetValue(ChunkItemTemplateProperty);
        set => SetValue(ChunkItemTemplateProperty, value);
    }

    /// <summary>Gets or sets the data template for tab strip items.</summary>
    public IDataTemplate? TabItemTemplate
    {
        get => GetValue(TabItemTemplateProperty);
        set => SetValue(TabItemTemplateProperty, value);
    }

    /// <summary>Gets or sets the currently selected tab item.</summary>
    public object? SelectedTab
    {
        get => GetValue(SelectedTabProperty);
        set => SetValue(SelectedTabProperty, value);
    }

    /// <summary>Gets or sets the content shown in the tab content area.</summary>
    public object? TabContent
    {
        get => GetValue(TabContentProperty);
        set => SetValue(TabContentProperty, value);
    }

    /// <summary>Gets or sets the label for the size column in chunk info.</summary>
    public string SizeLabel
    {
        get => GetValue(SizeLabelProperty);
        set => SetValue(SizeLabelProperty, value);
    }

    /// <summary>Gets or sets the label for the speed column in chunk info.</summary>
    public string SpeedLabel
    {
        get => GetValue(SpeedLabelProperty);
        set => SetValue(SpeedLabelProperty, value);
    }

    /// <summary>Gets or sets the label for the remaining time column in chunk info.</summary>
    public string RemainingLabel
    {
        get => GetValue(RemainingLabelProperty);
        set => SetValue(RemainingLabelProperty, value);
    }

    /// <summary>Gets or sets the label for the show details button.</summary>
    public string ShowDetailsLabel
    {
        get => GetValue(ShowDetailsLabelProperty);
        set => SetValue(ShowDetailsLabelProperty, value);
    }

    /// <summary>Gets or sets the label for the hide details button.</summary>
    public string HideDetailsLabel
    {
        get => GetValue(HideDetailsLabelProperty);
        set => SetValue(HideDetailsLabelProperty, value);
    }

    /// <summary>Gets or sets the label for the pause button.</summary>
    public string PauseLabel
    {
        get => GetValue(PauseLabelProperty);
        set => SetValue(PauseLabelProperty, value);
    }

    /// <summary>Gets or sets the label for the resume button.</summary>
    public string ResumeLabel
    {
        get => GetValue(ResumeLabelProperty);
        set => SetValue(ResumeLabelProperty, value);
    }

    /// <summary>Gets or sets the label for the cancel button.</summary>
    public string CancelLabel
    {
        get => GetValue(CancelLabelProperty);
        set => SetValue(CancelLabelProperty, value);
    }

    /// <summary>Gets the collection of tab items shown in the tab strip.</summary>
    public AvaloniaList<object> Tabs { get; } = new();

    /// <summary>Gets the collection of chunk data used for canvas visualization and the details list.</summary>
    public AvaloniaList<ChunkInfo> Chunks { get; } = new();

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Raised when the selected tab changes.</summary>
    public event EventHandler<SelectionChangedEventArgs>? TabSelectionChanged;

    // ── Private state ─────────────────────────────────────────────────────────

    private ListBox?          _tabStrip;
    private ProgressBar?      _progressBar;
    private Canvas?           _chunkCanvas;
    private ListBox?          _chunkList;
    private Button?           _pauseButton;
    private Button?           _cancelButton;
    private Button?           _detailsToggle;

    // ── Constructor ───────────────────────────────────────────────────────────

    public DownloadPanel()
    {
        Chunks.CollectionChanged += OnChunksChanged;
    }

    // ── Template ──────────────────────────────────────────────────────────────

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        DetachHandlers();

        _tabStrip      = e.NameScope.Find<ListBox>(PART_TabStrip);
        _progressBar   = e.NameScope.Find<ProgressBar>(PART_ProgressBar);
        _chunkCanvas   = e.NameScope.Find<Canvas>(PART_ChunkCanvas);
        _chunkList     = e.NameScope.Find<ListBox>(PART_ChunkList);
        _pauseButton   = e.NameScope.Find<Button>(PART_PauseButton);
        _cancelButton  = e.NameScope.Find<Button>(PART_CancelButton);
        _detailsToggle = e.NameScope.Find<Button>(PART_DetailsToggle);

        AttachHandlers();

        if (_tabStrip is not null)
        {
            _tabStrip.ItemsSource  = Tabs;
            _tabStrip.ItemTemplate = TabItemTemplate;
            if (SelectedTab is not null)
                _tabStrip.SelectedItem = SelectedTab;
        }

        if (_chunkList is not null)
        {
            _chunkList.ItemsSource = Chunks;
        }

        UpdatePseudoClasses();
        RedrawChunkCanvas();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsPausedProperty)
            PseudoClasses.Set(PC_Paused, change.GetNewValue<bool>());
        else if (change.Property == IsMergingProperty)
            PseudoClasses.Set(PC_Merging, change.GetNewValue<bool>());
        else if (change.Property == ShowDetailsProperty)
            PseudoClasses.Set(PC_ShowDetails, change.GetNewValue<bool>());
        else if (change.Property == ProgressProperty && _progressBar is not null)
            _progressBar.Value = change.GetNewValue<double>();
        else if (change.Property == MergeProgressProperty && _progressBar is not null && IsMerging)
            _progressBar.Value = change.GetNewValue<double>();
        else if (change.Property == SelectedTabProperty && _tabStrip is not null)
            _tabStrip.SelectedItem = change.NewValue;
        else if (change.Property == TabItemTemplateProperty && _tabStrip is not null)
            _tabStrip.ItemTemplate = change.GetNewValue<IDataTemplate?>();
        else if (change.Property == ChunkItemTemplateProperty && _chunkList is not null)
            _chunkList.ItemTemplate = change.GetNewValue<IDataTemplate?>();
        else if (change.Property == ChunkProgressBrushProperty ||
                 change.Property == ChunkBackgroundBrushProperty)
            RedrawChunkCanvas();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Redraws the chunk progress canvas based on the current <see cref="Chunks"/> collection.
    /// Call this after updating chunk progress values.
    /// </summary>
    public void RedrawChunkCanvas()
    {
        if (_chunkCanvas is null || Chunks.Count == 0) return;

        Dispatcher.UIThread.Post(() =>
        {
            _chunkCanvas.Children.Clear();

            double canvasWidth  = _chunkCanvas.Bounds.Width;
            double canvasHeight = _chunkCanvas.Bounds.Height;

            if (canvasWidth <= 0 || canvasHeight <= 0) return;

            IBrush progressBrush = ChunkProgressBrush
                ?? new SolidColorBrush(Color.Parse("#4CAF50"));

            foreach (var chunk in Chunks)
            {
                double chunkStart  = chunk.Start  * canvasWidth;
                double chunkWidth  = (chunk.End - chunk.Start) * canvasWidth;
                double fillWidth   = chunkWidth * Math.Clamp(chunk.Progress, 0, 1);

                var rect = new Avalonia.Controls.Shapes.Rectangle
                {
                    Width  = fillWidth,
                    Height = canvasHeight,
                    Fill   = chunk.ProgressBrush ?? progressBrush
                };

                Canvas.SetLeft(rect, chunkStart);
                Canvas.SetTop(rect, 0);
                _chunkCanvas.Children.Add(rect);
            }
        }, DispatcherPriority.Render);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void AttachHandlers()
    {
        if (_tabStrip      is not null) _tabStrip.SelectionChanged      += OnTabSelectionChanged;
        if (_detailsToggle is not null) _detailsToggle.Click            += OnDetailsToggleClicked;
        if (_chunkCanvas   is not null) _chunkCanvas.SizeChanged        += OnCanvasSizeChanged;
    }

    private void DetachHandlers()
    {
        if (_tabStrip      is not null) _tabStrip.SelectionChanged      -= OnTabSelectionChanged;
        if (_detailsToggle is not null) _detailsToggle.Click            -= OnDetailsToggleClicked;
        if (_chunkCanvas   is not null) _chunkCanvas.SizeChanged        -= OnCanvasSizeChanged;
    }

    private void OnTabSelectionChanged(object? s, SelectionChangedEventArgs e)
    {
        SelectedTab = _tabStrip?.SelectedItem;
        TabSelectionChanged?.Invoke(this, e);
    }

    private void OnDetailsToggleClicked(object? s, RoutedEventArgs e)
        => ShowDetails = !ShowDetails;

    private void OnCanvasSizeChanged(object? s, SizeChangedEventArgs e)
        => RedrawChunkCanvas();

    private void OnChunksChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_chunkList is not null)
            _chunkList.ItemsSource = Chunks;

        PseudoClasses.Set(PC_HasChunks, Chunks.Count > 0);
        RedrawChunkCanvas();
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(PC_Paused,      IsPaused);
        PseudoClasses.Set(PC_Merging,     IsMerging);
        PseudoClasses.Set(PC_ShowDetails, ShowDetails);
        PseudoClasses.Set(PC_HasChunks,   Chunks.Count > 0);
    }
}
