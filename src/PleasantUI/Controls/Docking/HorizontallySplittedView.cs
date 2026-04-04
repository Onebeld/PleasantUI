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
/// Represents a view that splits content horizontally with a resizable divider.
/// </summary>
[TemplatePart("PART_LeftContentPresenter", typeof(ContentPresenter))]
[TemplatePart("PART_RightContentPresenter", typeof(ContentPresenter))]
[TemplatePart("PART_Thumb", typeof(Thumb))]
[TemplatePart("PART_Root", typeof(Panel))]
public class HorizontallySplittedView : TemplatedControl, IDockAreaView
{
    /// <summary>Defines the <see cref="LeftContent"/> property.</summary>
    public static readonly StyledProperty<object?> LeftContentProperty =
        AvaloniaProperty.Register<HorizontallySplittedView, object?>(nameof(LeftContent));

    /// <summary>Defines the <see cref="LeftContentTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> LeftContentTemplateProperty =
        AvaloniaProperty.Register<HorizontallySplittedView, IDataTemplate?>(nameof(LeftContentTemplate));

    /// <summary>Defines the <see cref="LeftWidthProportion"/> property.</summary>
    public static readonly StyledProperty<double> LeftWidthProportionProperty =
        AvaloniaProperty.Register<HorizontallySplittedView, double>(nameof(LeftWidthProportion), defaultValue: 1);

    /// <summary>Defines the <see cref="RightContent"/> property.</summary>
    public static readonly StyledProperty<object?> RightContentProperty =
        AvaloniaProperty.Register<HorizontallySplittedView, object?>(nameof(RightContent));

    /// <summary>Defines the <see cref="RightContentTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> RightContentTemplateProperty =
        AvaloniaProperty.Register<HorizontallySplittedView, IDataTemplate?>(nameof(RightContentTemplate));

    /// <summary>Defines the <see cref="RightWidthProportion"/> property.</summary>
    public static readonly StyledProperty<double> RightWidthProportionProperty =
        AvaloniaProperty.Register<HorizontallySplittedView, double>(nameof(RightWidthProportion), defaultValue: 1);

    private DockArea? _leftDockArea;
    private DockArea? _rightDockArea;
    private ContentPresenter? _leftPresenter;
    private ContentPresenter? _rightPresenter;
    private Thumb? _thumb;
    private Panel? _root;
    private bool _dragEventSubscribed;
    private IDisposable? _visibilitySubscription;
    private const double ThumbPadding = 2;

    static HorizontallySplittedView()
    {
        DockAreaDragDropBehavior.BehaviorTypeProperty.Changed.AddClassHandler<HorizontallySplittedView, Type>(
            (s, e) => s.OnBehaviorTypeChanged(e));
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

    /// <summary>Gets or sets the left width proportion.</summary>
    public double LeftWidthProportion
    {
        get => GetValue(LeftWidthProportionProperty);
        set => SetValue(LeftWidthProportionProperty, value);
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

    /// <summary>Gets or sets the right width proportion.</summary>
    public double RightWidthProportion
    {
        get => GetValue(RightWidthProportionProperty);
        set => SetValue(RightWidthProportionProperty, value);
    }

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

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _visibilitySubscription?.Dispose();

        _leftPresenter = e.NameScope.Get<ContentPresenter>("PART_LeftContentPresenter");
        _rightPresenter = e.NameScope.Get<ContentPresenter>("PART_RightContentPresenter");
        _root = e.NameScope.Get<Panel>("PART_Root");
        _thumb = e.NameScope.Get<Thumb>("PART_Thumb");

        var leftObs = _leftPresenter.IsChildVisibleObservable();
        var rightObs = _rightPresenter.IsChildVisibleObservable();

        _visibilitySubscription = Observable.CombineLatest(leftObs, rightObs, (l, r) => (l, r))
            .Subscribe(new AnonymousObserver<(bool left, bool right)>(t =>
            {
                _thumb!.IsVisible = t.left && t.right;
                IsVisible = t.left || t.right;
                UpdateSize(Bounds.Size);
            }));

        _thumb.DragDelta += OnThumbDragDelta;
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == LeftContentProperty || change.Property == RightContentProperty)
            ContentChanged(change);
        else if (change.Property == LeftWidthProportionProperty || change.Property == RightWidthProportionProperty)
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
            case nameof(LeftContent): _leftDockArea = null; break;
            case nameof(RightContent): _rightDockArea = null; break;
        }

        switch (dockArea.Target)
        {
            case nameof(LeftContent): _leftDockArea = dockArea; break;
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
        if (!_dragEventSubscribed || (_leftDockArea == null && _rightDockArea == null)) return;

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
        if (_leftPresenter == null || _rightPresenter == null || _root == null || _thumb == null)
            return;

        var size = Bounds.Size;
        var newWidth = _leftPresenter.Width + e.Vector.X;

        if (newWidth + 5 >= size.Width || newWidth <= 5) return;

        LeftWidthProportion = Math.Clamp(newWidth / size.Width, 0, 1);
        RightWidthProportion = Math.Clamp(1 - LeftWidthProportion, 0, 1);
    }

    private void UpdateSize(Size size)
    {
        if (_leftPresenter == null || _rightPresenter == null || _thumb == null) return;

        var (leftWidth, rightWidth) = GetAbsoluteWidths(size);

        if (_leftPresenter.IsChildVisible() && _rightPresenter.IsChildVisible())
        {
            _leftPresenter.Margin = new Thickness(0);
            _leftPresenter.Width = leftWidth;
            _thumb.Margin = new Thickness(leftWidth - ThumbPadding, 0, 0, 0);
            _rightPresenter.Margin = new Thickness(leftWidth + ThumbPadding, 0, 0, 0);
            _rightPresenter.Width = Math.Max(rightWidth - ThumbPadding, 0);
        }
        else if (_leftPresenter.IsChildVisible())
        {
            _leftPresenter.Margin = new Thickness(0);
            _leftPresenter.Width = size.Width;
        }
        else if (_rightPresenter.IsChildVisible())
        {
            _rightPresenter.Margin = new Thickness(0);
            _rightPresenter.Width = size.Width;
        }
    }

    private (double left, double right) GetAbsoluteWidths(Size availableSize)
    {
        var den = LeftWidthProportion + RightWidthProportion;
        if (den == 0) return (availableSize.Width / 2, availableSize.Width / 2);
        var factor = availableSize.Width / den;
        return (factor * LeftWidthProportion, factor * RightWidthProportion);
    }
}
