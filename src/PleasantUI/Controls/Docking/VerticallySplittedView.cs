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
using PleasantUI.Core.Internal.Reactive;

namespace PleasantUI.Controls.Docking;

/// <summary>
/// Represents a view that splits content vertically with a resizable divider.
/// </summary>
[TemplatePart("PART_TopContentPresenter", typeof(ContentPresenter))]
[TemplatePart("PART_BottomContentPresenter", typeof(ContentPresenter))]
[TemplatePart("PART_Thumb", typeof(Thumb))]
[TemplatePart("PART_Root", typeof(Panel))]
public class VerticallySplittedView : TemplatedControl, IDockAreaView
{
    /// <summary>Defines the <see cref="TopContent"/> property.</summary>
    public static readonly StyledProperty<object?> TopContentProperty =
        AvaloniaProperty.Register<VerticallySplittedView, object?>(nameof(TopContent));

    /// <summary>Defines the <see cref="TopContentTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> TopContentTemplateProperty =
        AvaloniaProperty.Register<VerticallySplittedView, IDataTemplate?>(nameof(TopContentTemplate));

    /// <summary>Defines the <see cref="TopHeightProportion"/> property.</summary>
    public static readonly StyledProperty<double> TopHeightProportionProperty =
        AvaloniaProperty.Register<VerticallySplittedView, double>(nameof(TopHeightProportion), defaultValue: 1);

    /// <summary>Defines the <see cref="BottomContent"/> property.</summary>
    public static readonly StyledProperty<object?> BottomContentProperty =
        AvaloniaProperty.Register<VerticallySplittedView, object?>(nameof(BottomContent));

    /// <summary>Defines the <see cref="BottomContentTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> BottomContentTemplateProperty =
        AvaloniaProperty.Register<VerticallySplittedView, IDataTemplate?>(nameof(BottomContentTemplate));

    /// <summary>Defines the <see cref="BottomHeightProportion"/> property.</summary>
    public static readonly StyledProperty<double> BottomHeightProportionProperty =
        AvaloniaProperty.Register<VerticallySplittedView, double>(nameof(BottomHeightProportion), defaultValue: 1);

    private DockArea? _topDockArea;
    private DockArea? _bottomDockArea;
    private ContentPresenter? _topPresenter;
    private ContentPresenter? _bottomPresenter;
    private Thumb? _thumb;
    private Panel? _root;
    private bool _dragEventSubscribed;
    private IDisposable? _visibilitySubscription;
    private const double ThumbPadding = 2;

    static VerticallySplittedView()
    {
        DockAreaDragDropBehavior.BehaviorTypeProperty.Changed.AddClassHandler<VerticallySplittedView, Type>(
            (s, e) => s.OnBehaviorTypeChanged(e));
    }

    /// <summary>Gets or sets the top content.</summary>
    [DependsOn(nameof(TopContentTemplate))]
    public object? TopContent
    {
        get => GetValue(TopContentProperty);
        set => SetValue(TopContentProperty, value);
    }

    /// <summary>Gets or sets the top content template.</summary>
    public IDataTemplate? TopContentTemplate
    {
        get => GetValue(TopContentTemplateProperty);
        set => SetValue(TopContentTemplateProperty, value);
    }

    /// <summary>Gets or sets the top height proportion.</summary>
    public double TopHeightProportion
    {
        get => GetValue(TopHeightProportionProperty);
        set => SetValue(TopHeightProportionProperty, value);
    }

    /// <summary>Gets or sets the bottom content.</summary>
    [DependsOn(nameof(BottomContentTemplate))]
    public object? BottomContent
    {
        get => GetValue(BottomContentProperty);
        set => SetValue(BottomContentProperty, value);
    }

    /// <summary>Gets or sets the bottom content template.</summary>
    public IDataTemplate? BottomContentTemplate
    {
        get => GetValue(BottomContentTemplateProperty);
        set => SetValue(BottomContentTemplateProperty, value);
    }

    /// <summary>Gets or sets the bottom height proportion.</summary>
    public double BottomHeightProportion
    {
        get => GetValue(BottomHeightProportionProperty);
        set => SetValue(BottomHeightProportionProperty, value);
    }

    (DockArea, Control)[] IDockAreaView.GetArea()
        => [(_topDockArea!, _topPresenter!), (_bottomDockArea!, _bottomPresenter!)];

    void IDockAreaView.OnAttachedToDockArea(DockArea dockArea)
    {
        if (dockArea.Target == nameof(TopContent))
            _topDockArea = dockArea;
        else if (dockArea.Target == nameof(BottomContent))
            _bottomDockArea = dockArea;

        dockArea.PropertyChanged += DockAreaOnPropertyChanged;
        UpdateIsDragDropEnabled();
    }

    void IDockAreaView.OnDetachedFromDockArea(DockArea dockArea)
    {
        if (dockArea.Target == nameof(TopContent))
            _topDockArea = null;
        else if (dockArea.Target == nameof(BottomContent))
            _bottomDockArea = null;

        dockArea.PropertyChanged -= DockAreaOnPropertyChanged;
        UpdateIsDragDropEnabled();
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _visibilitySubscription?.Dispose();

        _topPresenter = e.NameScope.Get<ContentPresenter>("PART_TopContentPresenter");
        _bottomPresenter = e.NameScope.Get<ContentPresenter>("PART_BottomContentPresenter");
        _root = e.NameScope.Get<Panel>("PART_Root");
        _thumb = e.NameScope.Get<Thumb>("PART_Thumb");

        var topObs = _topPresenter.IsChildVisibleObservable();
        var bottomObs = _bottomPresenter.IsChildVisibleObservable();

        _visibilitySubscription = Observable.CombineLatest(topObs, bottomObs, (t, b) => (t, b))
            .Subscribe(new AnonymousObserver<(bool top, bool bottom)>(v =>
            {
                _thumb!.IsVisible = v.top && v.bottom;
                IsVisible = v.top || v.bottom;
                UpdateSize(Bounds.Size);
            }));

        _thumb.DragDelta += OnThumbDragDelta;
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TopContentProperty || change.Property == BottomContentProperty)
            ContentChanged(change);
        else if (change.Property == TopHeightProportionProperty || change.Property == BottomHeightProportionProperty)
            UpdateSize(Bounds.Size);
    }

    /// <inheritdoc/>
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateSize(e.NewSize);
    }

    private void DockAreaOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != DockArea.TargetProperty || sender is not DockArea dockArea) return;

        switch (e.OldValue)
        {
            case nameof(TopContent): _topDockArea = null; break;
            case nameof(BottomContent): _bottomDockArea = null; break;
        }

        switch (dockArea.Target)
        {
            case nameof(TopContent): _topDockArea = dockArea; break;
            case nameof(BottomContent): _bottomDockArea = dockArea; break;
        }

        UpdateIsDragDropEnabled();
    }

    private void UpdateIsDragDropEnabled()
    {
        var list = Interaction.GetBehaviors(this);

        if (_topDockArea != null || _bottomDockArea != null)
        {
            if (_dragEventSubscribed) return;
            _dragEventSubscribed = true;
            var behavior = Activator.CreateInstance(DockAreaDragDropBehavior.GetBehaviorType(this)) as DockAreaDragDropBehavior;
            if (behavior != null) list.Add(behavior);
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
        if (!_dragEventSubscribed || (_topDockArea == null && _bottomDockArea == null)) return;

        var list = Interaction.GetBehaviors(this);
        foreach (var b in list.OfType<DockAreaDragDropBehavior>().ToList())
            list.Remove(b);

        var type = e.NewValue.GetValueOrDefault() ?? typeof(DockAreaDragDropBehavior);
        var newBehavior = Activator.CreateInstance(type) as DockAreaDragDropBehavior;
        if (newBehavior != null) list.Add(newBehavior);
    }

    private void ContentChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is ILogical oldChild) LogicalChildren.Remove(oldChild);
        if (e.NewValue is ILogical newChild) LogicalChildren.Add(newChild);
    }

    private void OnThumbDragDelta(object? sender, VectorEventArgs e)
    {
        if (_topPresenter == null || _bottomPresenter == null || _root == null || _thumb == null)
            return;

        var size = Bounds.Size;
        var newHeight = _topPresenter.Height + e.Vector.Y;

        if (newHeight + 5 >= size.Height || newHeight <= 5) return;

        TopHeightProportion = Math.Clamp(newHeight / size.Height, 0, 1);
        BottomHeightProportion = Math.Clamp(1 - TopHeightProportion, 0, 1);
    }

    private void UpdateSize(Size size)
    {
        if (_topPresenter == null || _bottomPresenter == null || _thumb == null) return;

        var (topHeight, bottomHeight) = GetAbsoluteHeights(size);

        if (_topPresenter.IsChildVisible() && _bottomPresenter.IsChildVisible())
        {
            _topPresenter.Margin = new Thickness(0);
            _topPresenter.Height = topHeight;
            _thumb.Margin = new Thickness(0, topHeight - ThumbPadding, 0, 0);
            _bottomPresenter.Margin = new Thickness(0, topHeight + ThumbPadding, 0, 0);
            _bottomPresenter.Height = Math.Max(bottomHeight - ThumbPadding, 0);
        }
        else if (_topPresenter.IsChildVisible())
        {
            _topPresenter.Margin = new Thickness(0);
            _topPresenter.Height = size.Height;
        }
        else if (_bottomPresenter.IsChildVisible())
        {
            _bottomPresenter.Margin = new Thickness(0);
            _bottomPresenter.Height = size.Height;
        }
    }

    private (double top, double bottom) GetAbsoluteHeights(Size availableSize)
    {
        var den = TopHeightProportion + BottomHeightProportion;
        if (den == 0) return (availableSize.Height / 2, availableSize.Height / 2);
        var factor = availableSize.Height / den;
        return (factor * TopHeightProportion, factor * BottomHeightProportion);
    }
}
