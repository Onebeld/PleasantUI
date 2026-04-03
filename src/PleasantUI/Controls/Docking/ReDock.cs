using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Metadata;
using Avalonia.Reactive;
using Avalonia.Xaml.Interactivity;
using PleasantUI.Core;

namespace PleasantUI.Controls.Docking;

/// <summary>
/// Represents a three-pane docking control with left, center, and right content areas.
/// Uses a Grid with <see cref="NamedColumnDefinition"/> columns so panels collapse
/// cleanly when hidden and resize correctly when dragged.
/// </summary>
[TemplatePart("PART_Root", typeof(Grid))]
[TemplatePart("PART_LeftContentPresenter", typeof(ContentPresenter))]
[TemplatePart("PART_ContentPresenter", typeof(ContentPresenter))]
[TemplatePart("PART_RightContentPresenter", typeof(ContentPresenter))]
[TemplatePart("PART_LeftThumb", typeof(Thumb))]
[TemplatePart("PART_RightThumb", typeof(Thumb))]
public class ReDock : TemplatedControl, IDockAreaView
{
    // Column name constants — must match x:Name in the AXAML template
    private const string LeftColumnName       = "PART_LeftColumn";
    private const string LeftThumbColumnName  = "PART_LeftThumbColumn";
    private const string CenterColumnName     = "PART_CenterColumn";
    private const string RightThumbColumnName = "PART_RightThumbColumn";
    private const string RightColumnName      = "PART_RightColumn";

    private const double SideMinPx      = 32;
    private const double CenterMinPx    = 80;
    private const double SideMaxRatio   = 0.75;

    /// <summary>Defines the <see cref="LeftContent"/> property.</summary>
    public static readonly StyledProperty<object?> LeftContentProperty =
        AvaloniaProperty.Register<ReDock, object?>(nameof(LeftContent));

    /// <summary>Defines the <see cref="LeftContentTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> LeftContentTemplateProperty =
        AvaloniaProperty.Register<ReDock, IDataTemplate?>(nameof(LeftContentTemplate));

    /// <summary>Defines the <see cref="LeftWidthProportion"/> property.</summary>
    public static readonly StyledProperty<double> LeftWidthProportionProperty =
        AvaloniaProperty.Register<ReDock, double>(nameof(LeftWidthProportion), defaultValue: 0.25);

    /// <summary>Defines the <see cref="Content"/> property.</summary>
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<ReDock, object?>(nameof(Content));

    /// <summary>Defines the <see cref="ContentTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        AvaloniaProperty.Register<ReDock, IDataTemplate?>(nameof(ContentTemplate));

    /// <summary>Defines the <see cref="WidthProportion"/> property.</summary>
    public static readonly StyledProperty<double> WidthProportionProperty =
        AvaloniaProperty.Register<ReDock, double>(nameof(WidthProportion), defaultValue: 0.5);

    /// <summary>Defines the <see cref="RightContent"/> property.</summary>
    public static readonly StyledProperty<object?> RightContentProperty =
        AvaloniaProperty.Register<ReDock, object?>(nameof(RightContent));

    /// <summary>Defines the <see cref="RightContentTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> RightContentTemplateProperty =
        AvaloniaProperty.Register<ReDock, IDataTemplate?>(nameof(RightContentTemplate));

    /// <summary>Defines the <see cref="RightWidthProportion"/> property.</summary>
    public static readonly StyledProperty<double> RightWidthProportionProperty =
        AvaloniaProperty.Register<ReDock, double>(nameof(RightWidthProportion), defaultValue: 0.25);

    private DockArea? _leftDockArea;
    private DockArea? _rightDockArea;

    private Grid?            _rootGrid;
    private NamedColumnDefinition? _leftColumn;
    private NamedColumnDefinition? _leftThumbColumn;
    private NamedColumnDefinition? _centerColumn;
    private NamedColumnDefinition? _rightThumbColumn;
    private NamedColumnDefinition? _rightColumn;

    private ContentPresenter? _leftPresenter;
    private ContentPresenter? _rightPresenter;
    private ContentPresenter? _presenter;
    private Thumb?            _leftThumb;
    private Thumb?            _rightThumb;

    private bool        _dragEventSubscribed;
    private IDisposable? _visibilitySubscription;

    static ReDock()
    {
        DockAreaDragDropBehavior.BehaviorTypeProperty.Changed
            .AddClassHandler<ReDock, Type>((s, e) => s.OnBehaviorTypeChanged(e));
    }

    /// <summary>Gets or sets the left content.</summary>
    [DependsOn(nameof(LeftContentTemplate))]
    public object? LeftContent
    {
        get => GetValue(LeftContentProperty);
        set => SetValue(LeftContentProperty, value);
    }

    /// <summary>Gets or sets the left content template.</summary>
    public IDataTemplate? LeftContentTemplate
    {
        get => GetValue(LeftContentTemplateProperty);
        set => SetValue(LeftContentTemplateProperty, value);
    }

    /// <summary>Gets or sets the left width proportion (0–1).</summary>
    public double LeftWidthProportion
    {
        get => GetValue(LeftWidthProportionProperty);
        set => SetValue(LeftWidthProportionProperty, value);
    }

    /// <summary>Gets or sets the center content.</summary>
    [Content]
    [DependsOn(nameof(ContentTemplate))]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>Gets or sets the center content template.</summary>
    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    /// <summary>Gets or sets the center width proportion (0–1).</summary>
    public double WidthProportion
    {
        get => GetValue(WidthProportionProperty);
        set => SetValue(WidthProportionProperty, value);
    }

    /// <summary>Gets or sets the right content.</summary>
    [DependsOn(nameof(RightContentTemplate))]
    public object? RightContent
    {
        get => GetValue(RightContentProperty);
        set => SetValue(RightContentProperty, value);
    }

    /// <summary>Gets or sets the right content template.</summary>
    public IDataTemplate? RightContentTemplate
    {
        get => GetValue(RightContentTemplateProperty);
        set => SetValue(RightContentTemplateProperty, value);
    }

    /// <summary>Gets or sets the right width proportion (0–1).</summary>
    public double RightWidthProportion
    {
        get => GetValue(RightWidthProportionProperty);
        set => SetValue(RightWidthProportionProperty, value);
    }

    // ── IDockAreaView ────────────────────────────────────────────────────────

    (DockArea, Control)[] IDockAreaView.GetArea()
        => [(_leftDockArea!, _leftPresenter!), (_rightDockArea!, _rightPresenter!)];

    void IDockAreaView.OnAttachedToDockArea(DockArea dockArea)
    {
        if (dockArea.Target == nameof(LeftContent))
            _leftDockArea = dockArea;
        else if (dockArea.Target == nameof(RightContent))
            _rightDockArea = dockArea;

        dockArea.PropertyChanged += DockAreaOnPropertyChanged;
        UpdateIsDragDropEnabled();
    }

    void IDockAreaView.OnDetachedFromDockArea(DockArea dockArea)
    {
        if (dockArea.Target == nameof(LeftContent))
            _leftDockArea = null;
        else if (dockArea.Target == nameof(RightContent))
            _rightDockArea = null;

        dockArea.PropertyChanged -= DockAreaOnPropertyChanged;
        UpdateIsDragDropEnabled();
    }

    // ── Template ─────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _visibilitySubscription?.Dispose();

        // Resolve the Grid by name from the name scope (it IS in the visual tree)
        _rootGrid = e.NameScope.Get<Grid>("PART_Root");

        // Resolve NamedColumnDefinition instances by scanning ColumnDefinitions
        // and matching the Name styled property — they are NOT in the name scope
        // because ColumnDefinition objects live outside the visual tree.
        _leftColumn       = FindNamedColumn(_rootGrid, LeftColumnName);
        _leftThumbColumn  = FindNamedColumn(_rootGrid, LeftThumbColumnName);
        _centerColumn     = FindNamedColumn(_rootGrid, CenterColumnName);
        _rightThumbColumn = FindNamedColumn(_rootGrid, RightThumbColumnName);
        _rightColumn      = FindNamedColumn(_rootGrid, RightColumnName);

        _leftPresenter  = e.NameScope.Get<ContentPresenter>("PART_LeftContentPresenter");
        _rightPresenter = e.NameScope.Get<ContentPresenter>("PART_RightContentPresenter");
        _presenter      = e.NameScope.Get<ContentPresenter>("PART_ContentPresenter");
        _leftThumb      = e.NameScope.Get<Thumb>("PART_LeftThumb");
        _rightThumb     = e.NameScope.Get<Thumb>("PART_RightThumb");

        // Ensure side presenters start hidden when they have no content so the
        // columns collapse immediately. HandleButtonToggle will set IsVisible=true
        // when content is injected dynamically.
        if (_leftPresenter.Content == null)
            _leftPresenter.IsVisible = false;
        if (_rightPresenter.Content == null)
            _rightPresenter.IsVisible = false;

        var leftObs  = _leftPresenter.IsChildVisibleObservable();
        var rightObs = _rightPresenter.IsChildVisibleObservable();

        _visibilitySubscription = Observable.CombineLatest(leftObs, rightObs, (l, r) => (l, r))
            .Subscribe(new AnonymousObserver<(bool left, bool right)>(_ => UpdateColumnWidths()));

        _leftThumb.DragDelta  += OnLeftThumbDragDelta;
        _rightThumb.DragDelta += OnRightThumbDragDelta;

        UpdateColumnWidths();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ContentProperty ||
            change.Property == LeftContentProperty ||
            change.Property == RightContentProperty)
        {
            ContentChanged(change);
        }
        else if (change.Property == LeftWidthProportionProperty ||
                 change.Property == WidthProportionProperty ||
                 change.Property == RightWidthProportionProperty)
        {
            UpdateColumnWidths();
        }
    }

    /// <inheritdoc/>
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateColumnWidths();
    }

    // ── Column management ────────────────────────────────────────────────────

    /// <summary>
    /// Scans <paramref name="grid"/>.ColumnDefinitions for a
    /// <see cref="NamedColumnDefinition"/> whose <see cref="NamedColumnDefinition.Name"/>
    /// equals <paramref name="name"/>.
    /// </summary>
    private static NamedColumnDefinition? FindNamedColumn(Grid grid, string name)
    {
        foreach (var col in grid.ColumnDefinitions)
        {
            if (col is NamedColumnDefinition ncd && ncd.Name == name)
                return ncd;
        }
        return null;
    }

    private void UpdateColumnWidths()
    {
        if (_leftColumn == null || _leftThumbColumn == null ||
            _centerColumn == null ||
            _rightThumbColumn == null || _rightColumn == null ||
            _leftPresenter == null || _rightPresenter == null ||
            _leftThumb == null || _rightThumb == null)
            return;

        var leftVisible  = _leftPresenter.IsChildVisible();
        var rightVisible = _rightPresenter.IsChildVisible();

        var totalWidth = Bounds.Width;

        if (leftVisible)
        {
            // Use pixel width derived from proportion so the side panel has a
            // fixed pixel size and the center column (Star) fills the remainder.
            var leftPx = totalWidth > 0
                ? Math.Max(totalWidth * LeftWidthProportion, SideMinPx)
                : SideMinPx;

            _leftColumn.MinWidth = SideMinPx;
            _leftColumn.Width    = new GridLength(leftPx, GridUnitType.Pixel);
            _leftThumbColumn.Width = GridLength.Auto;
            _leftThumb.IsVisible = true;
        }
        else
        {
            _leftColumn.MinWidth   = 0;
            _leftColumn.Width      = new GridLength(0, GridUnitType.Pixel);
            _leftThumbColumn.Width = new GridLength(0, GridUnitType.Pixel);
            _leftThumb.IsVisible   = false;
        }

        // Center always fills remaining space
        _centerColumn.Width = new GridLength(1, GridUnitType.Star);

        if (rightVisible)
        {
            var rightPx = totalWidth > 0
                ? Math.Max(totalWidth * RightWidthProportion, SideMinPx)
                : SideMinPx;

            _rightColumn.MinWidth = SideMinPx;
            _rightColumn.Width    = new GridLength(rightPx, GridUnitType.Pixel);
            _rightThumbColumn.Width = GridLength.Auto;
            _rightThumb.IsVisible = true;
        }
        else
        {
            _rightColumn.MinWidth   = 0;
            _rightColumn.Width      = new GridLength(0, GridUnitType.Pixel);
            _rightThumbColumn.Width = new GridLength(0, GridUnitType.Pixel);
            _rightThumb.IsVisible   = false;
        }
    }

    // ── Drag resize ──────────────────────────────────────────────────────────

    private void OnLeftThumbDragDelta(object? sender, VectorEventArgs e)
    {
        if (_leftPresenter?.IsChildVisible() != true) return;

        var totalWidth = Bounds.Width;
        if (totalWidth <= 0) return;

        var rightVisible = _rightPresenter?.IsChildVisible() == true;
        var rightPx      = rightVisible ? totalWidth * RightWidthProportion : 0;
        var maxLeft      = Math.Min(totalWidth * SideMaxRatio, totalWidth - rightPx - CenterMinPx);

        var currentLeftPx = totalWidth * LeftWidthProportion;
        var newLeftPx     = Math.Clamp(currentLeftPx + e.Vector.X, SideMinPx, Math.Max(maxLeft, SideMinPx));

        LeftWidthProportion = newLeftPx / totalWidth;
        UpdateColumnWidths();
    }

    private void OnRightThumbDragDelta(object? sender, VectorEventArgs e)
    {
        if (_rightPresenter?.IsChildVisible() != true) return;

        var totalWidth = Bounds.Width;
        if (totalWidth <= 0) return;

        var leftVisible = _leftPresenter?.IsChildVisible() == true;
        var leftPx      = leftVisible ? totalWidth * LeftWidthProportion : 0;
        var maxRight    = Math.Min(totalWidth * SideMaxRatio, totalWidth - leftPx - CenterMinPx);

        var currentRightPx = totalWidth * RightWidthProportion;
        var newRightPx     = Math.Clamp(currentRightPx - e.Vector.X, SideMinPx, Math.Max(maxRight, SideMinPx));

        RightWidthProportion = newRightPx / totalWidth;
        UpdateColumnWidths();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void DockAreaOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != DockArea.TargetProperty || sender is not DockArea dockArea) return;

        switch (e.OldValue)
        {
            case nameof(LeftContent):  _leftDockArea  = null; break;
            case nameof(RightContent): _rightDockArea = null; break;
        }

        switch (dockArea.Target)
        {
            case nameof(LeftContent):  _leftDockArea  = dockArea; break;
            case nameof(RightContent): _rightDockArea = dockArea; break;
        }

        UpdateIsDragDropEnabled();
    }

    private void UpdateIsDragDropEnabled()
    {
        var list = Interaction.GetBehaviors(this);

        if (_leftDockArea != null || _rightDockArea != null)
        {
            if (_dragEventSubscribed) return;
            _dragEventSubscribed = true;
            if (Activator.CreateInstance(DockAreaDragDropBehavior.GetBehaviorType(this))
                    is DockAreaDragDropBehavior behavior)
                list.Add(behavior);
        }
        else
        {
            if (!_dragEventSubscribed) return;
            _dragEventSubscribed = false;
            foreach (var b in list.OfType<DockAreaDragDropBehavior>().ToList())
                list.Remove(b);
        }
    }

    private void OnBehaviorTypeChanged(AvaloniaPropertyChangedEventArgs<Type> e)
    {
        if (!_dragEventSubscribed || (_leftDockArea == null && _rightDockArea == null)) return;

        var list = Interaction.GetBehaviors(this);
        foreach (var b in list.OfType<DockAreaDragDropBehavior>().ToList())
            list.Remove(b);

        var type = e.NewValue.GetValueOrDefault() ?? typeof(DockAreaDragDropBehavior);
        if (Activator.CreateInstance(type) is DockAreaDragDropBehavior newBehavior)
            list.Add(newBehavior);
    }

    private void ContentChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is ILogical oldChild) LogicalChildren.Remove(oldChild);
        if (e.NewValue is ILogical newChild) LogicalChildren.Add(newChild);
    }
}
