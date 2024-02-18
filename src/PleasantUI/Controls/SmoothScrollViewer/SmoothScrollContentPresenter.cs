using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PleasantUI.Reactive;

namespace PleasantUI.Controls;

/// <summary>
/// Represents a control that provides smooth scrolling behavior for its content.
/// </summary>
public class SmoothScrollContentPresenter : ContentPresenter, IScrollable, IScrollAnchorProvider
{
    private const double EdgeDetectionTolerance = 0.1;
    private const int LogicalScrollItemSize = 50;

    private bool _canHorizontallyScroll;
    private bool _canVerticallyScroll;
    private bool _arranging;
    private Size _extent;
    private Vector _offset;
    //private Easing _smoothScrollEasing = new BounceEaseInOut();
    //private TimeSpan _smoothScrollDuration = TimeSpan.FromMilliseconds(0);
    //private bool _usesSmoothScrolling = true;
    private readonly DispatcherTimer _smoothScrollTimer; //Timer(1);
    private bool _smoothScrollTimerStarted;
    private double _animStartOffsetX;
    private double _animStartOffsetY;
    private double _offsetX;
    private double _offsetY;
    private double _animDuration;
    private double _animTimeRemaining;

    private Easing? _currentEasing;
    private IDisposable? _logicalScrollSubscription;
    private Size _viewport;
    private Dictionary<int, Vector>? _activeLogicalGestureScrolls;
    private List<Control>? _anchorCandidates;
    private Control? _anchorElement;
    private Rect _anchorElementBounds;
    private bool _isAnchorElementDirty;

    /// <summary>
    /// Defines the <see cref="CanHorizontallyScroll"/> property.
    /// </summary>
    public static readonly DirectProperty<SmoothScrollContentPresenter, bool> CanHorizontallyScrollProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollContentPresenter, bool>(
            nameof(CanHorizontallyScroll),
            o => o.CanHorizontallyScroll,
            (o, v) => o.CanHorizontallyScroll = v);

    /// <summary>
    /// Defines the <see cref="CanVerticallyScroll"/> property.
    /// </summary>
    public static readonly DirectProperty<SmoothScrollContentPresenter, bool> CanVerticallyScrollProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollContentPresenter, bool>(
            nameof(CanVerticallyScroll),
            o => o.CanVerticallyScroll,
            (o, v) => o.CanVerticallyScroll = v);

    /// <summary>
    /// Defines the <see cref="Extent"/> property.
    /// </summary>
    public static readonly DirectProperty<SmoothScrollContentPresenter, Size> ExtentProperty =
        SmoothScrollViewer.ExtentProperty.AddOwner<SmoothScrollContentPresenter>(
            o => o.Extent,
            (o, v) => o.Extent = v);

    /// <summary>
    /// Defines the <see cref="Offset"/> property.
    /// </summary>
    public static readonly DirectProperty<SmoothScrollContentPresenter, Vector> OffsetProperty =
        SmoothScrollViewer.OffsetProperty.AddOwner<SmoothScrollContentPresenter>(
            o => o.Offset,
            (o, v) => o.Offset = v);

    /// <summary>
    /// Defines the <see cref="AnimationOffset"/> property.
    /// </summary>
    public static readonly StyledProperty<Vector> AnimationOffsetProperty =
        AvaloniaProperty.Register<SmoothScrollContentPresenter, Vector>(nameof(AnimationOffset));

    /// <summary>
    /// Defines the <see cref="Viewport"/> property.
    /// </summary>
    public static readonly DirectProperty<SmoothScrollContentPresenter, Size> ViewportProperty =
        SmoothScrollViewer.ViewportProperty.AddOwner<SmoothScrollContentPresenter>(
            o => o.Viewport,
            (o, v) => o.Viewport = v);


    /// <summary>
    /// Gets or sets a value indicating whether the content can be scrolled horizontally.
    /// </summary>
    public bool CanHorizontallyScroll
    {
        get => _canHorizontallyScroll;
        set => SetAndRaise(CanHorizontallyScrollProperty, ref _canHorizontallyScroll, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the content can be scrolled horizontally.
    /// </summary>
    public bool CanVerticallyScroll
    {
        get => _canVerticallyScroll;
        set => SetAndRaise(CanVerticallyScrollProperty, ref _canVerticallyScroll, value);
    }

    /// <summary>
    /// Gets the extent of the scrollable content.
    /// </summary>
    public Size Extent
    {
        get => _extent;
        private set => SetAndRaise(ExtentProperty, ref _extent, value);
    }

    /// <summary>
    /// Gets or sets the current logical scroll offset.
    /// </summary>
    public Vector Offset
    {
        get => _offset;
        set => SetAndRaise(OffsetProperty, ref _offset, SmoothScrollViewer.CoerceOffset(Extent, Viewport, value));
    }

    /// <summary>
    /// Gets or sets the current visible scroll offset.
    /// </summary>
    public Vector AnimationOffset
    {
        get => GetValue(AnimationOffsetProperty);
        set => SetValue(AnimationOffsetProperty, value);
    }

    /// <summary>
    /// Gets the size of the viewport on the scrollable content.
    /// </summary>
    public Size Viewport
    {
        get => _viewport;
        private set => SetAndRaise(ViewportProperty, ref _viewport, value);
    }

    private bool UsesSmoothScrolling => ShouldUseSmoothScrolling(out Easing _, out TimeSpan __);

    private Vector CurrentFromOffset => UsesSmoothScrolling ? AnimationOffset : Offset;
    
    /// <summary>
    /// Initializes static members of the <see cref="SmoothScrollContentPresenter"/> class.
    /// </summary>
    static SmoothScrollContentPresenter()
    {
        ClipToBoundsProperty.OverrideDefaultValue(typeof(SmoothScrollContentPresenter), true);
        ChildProperty.Changed.AddClassHandler<SmoothScrollContentPresenter>((x, e) => x.ChildChanged(e));
        OffsetProperty.Changed.AddClassHandler<SmoothScrollContentPresenter>((x, e) => x.OffsetChanged(e));

        AffectsArrange<SmoothScrollContentPresenter>(AnimationOffsetProperty);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmoothScrollContentPresenter"/> class.
    /// </summary>
    public SmoothScrollContentPresenter()
    {
        AddHandler(RequestBringIntoViewEvent, BringIntoViewRequested);
        AddHandler(Gestures.ScrollGestureEvent, OnScrollGesture);

        this.GetObservable(ChildProperty).Subscribe(UpdateScrollableSubscription);

        _smoothScrollTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(1), DispatcherPriority.Render,
            SmoothScrollTimer_Elapsed);
    }

    private bool ShouldUseSmoothScrolling(out Easing? easing, out TimeSpan duration)
    {
        if (TemplatedParent is SmoothScrollViewer scrollV)
        {
            easing = scrollV.SmoothScrollEasing;
            duration = scrollV.SmoothScrollDuration;
            return easing != null && duration is { TotalMilliseconds: > 0 };
        }
        else
        {
            easing = null;
            duration = TimeSpan.Zero;
            return false;
        }
    }

    /// <inheritdoc/>
    Control? IScrollAnchorProvider.CurrentAnchor
    {
        get
        {
            EnsureAnchorElementSelection();
            return _anchorElement;
        }
    }

    /// <summary>
    /// Attempts to bring a portion of the target visual into view by scrolling the content.
    /// </summary>
    /// <param name="target">The target visual.</param>
    /// <param name="targetRect">The portion of the target visual to bring into view.</param>
    /// <returns>True if the scroll offset was changed; otherwise false.</returns>
    public bool BringDescendantIntoView(Visual? target, Rect targetRect)
    {
        if (Child?.IsEffectivelyVisible != true)
        {
            return false;
        }

        ILogicalScrollable? scrollable = Child as ILogicalScrollable;

        if (scrollable?.IsLogicalScrollEnabled == true && target is Control control)
        {
            return scrollable.BringIntoView(control, targetRect);
        }

        Matrix? transform = target?.TransformToVisual(Child);

        if (transform == null)
        {
            return false;
        }

        Rect rect = targetRect.TransformToAABB(transform.Value);
        Vector offset = CurrentFromOffset;
        bool result = false;

        if (rect.Bottom > offset.Y + Viewport.Height)
        {
            offset = offset.WithY(rect.Bottom - Viewport.Height + Child.Margin.Top);
            result = true;
        }

        if (rect.Y < offset.Y)
        {
            offset = offset.WithY(rect.Y);
            result = true;
        }

        if (rect.Right > offset.X + Viewport.Width)
        {
            offset = offset.WithX(rect.Right - Viewport.Width + Child.Margin.Left);
            result = true;
        }

        if (rect.X < offset.X)
        {
            offset = offset.WithX(rect.X);
            result = true;
        }

        if (result)
        {
            Offset = offset;
        }

        return result;
    }

    /// <inheritdoc/>
    void IScrollAnchorProvider.RegisterAnchorCandidate(Control element)
    {
        if (!this.IsVisualAncestorOf(element))
        {
            throw new InvalidOperationException(
                "An anchor control must be a visual descendent of the SmoothScrollContentPresenter.");
        }

        _anchorCandidates ??= [];
        _anchorCandidates.Add(element);
        _isAnchorElementDirty = true;
    }

    /// <inheritdoc/>
    void IScrollAnchorProvider.UnregisterAnchorCandidate(Control element)
    {
        _anchorCandidates?.Remove(element);
        _isAnchorElementDirty = true;

        if (_anchorElement == element)
        {
            _anchorElement = null;
        }
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (_logicalScrollSubscription != null || Child == null)
        {
            return base.MeasureOverride(availableSize);
        }

        Size constraint = new(
            CanHorizontallyScroll ? double.PositiveInfinity : availableSize.Width,
            CanVerticallyScroll ? double.PositiveInfinity : availableSize.Height);

        Child.Measure(constraint);
        return Child.DesiredSize.Constrain(availableSize);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (_logicalScrollSubscription != null || Child == null)
        {
            return base.ArrangeOverride(finalSize);
        }

        return ArrangeWithAnchoring(finalSize);
    }

    private Size ArrangeWithAnchoring(Size finalSize)
    {
        Size size = new(
            CanHorizontallyScroll ? Math.Max(Child!.DesiredSize.Width, finalSize.Width) : finalSize.Width,
            CanVerticallyScroll ? Math.Max(Child!.DesiredSize.Height, finalSize.Height) : finalSize.Height);

        Vector TrackAnchor()
        {
            // If we have an anchor and its position relative to Child has changed during the
            // arrange then that change wasn't just due to scrolling (as scrolling doesn't adjust
            // relative positions within Child).
            if (_anchorElement != null &&
                TranslateBounds(_anchorElement, Child!, out Rect updatedBounds) &&
                updatedBounds.Position != _anchorElementBounds.Position)
            {
                Point offset = updatedBounds.Position - _anchorElementBounds.Position;
                return offset;
            }

            return default;
        }

        Vector currentFromOffset = CurrentFromOffset;
        bool isAnchoring = currentFromOffset.X >= EdgeDetectionTolerance ||
                           currentFromOffset.Y >= EdgeDetectionTolerance;

        if (isAnchoring)
        {
            // Calculate the new anchor element if necessary.
            EnsureAnchorElementSelection();

            // Do the arrange.
            ArrangeOverrideImpl(size, -currentFromOffset);

            // If the anchor moved during the arrange, we need to adjust the offset and do another arrange.
            Vector anchorShift = TrackAnchor();

            if (anchorShift != default)
            {
                Vector newOffset = currentFromOffset + anchorShift;
                Size newExtent = Extent;
                Vector maxOffset = new(Extent.Width - Viewport.Width, Extent.Height - Viewport.Height);

                if (newOffset.X > maxOffset.X)
                {
                    newExtent = newExtent.WithWidth(newOffset.X + Viewport.Width);
                }

                if (newOffset.Y > maxOffset.Y)
                {
                    newExtent = newExtent.WithHeight(newOffset.Y + Viewport.Height);
                }

                Extent = newExtent;

                try
                {
                    _arranging = true;
                    Offset = newOffset;
                }
                finally
                {
                    _arranging = false;
                }

                ArrangeOverrideImpl(size, -currentFromOffset);
            }
        }
        else
        {
            ArrangeOverrideImpl(size, -currentFromOffset);
        }

        Viewport = finalSize;
        Extent = Child!.Bounds.Size.Inflate(Child.Margin);
        _isAnchorElementDirty = true;

        return finalSize;
    }

    internal Size ArrangeOverrideImpl(Size finalSize, Vector offset)
    {
        if (Child == null) return finalSize;

        Thickness padding = Padding + BorderThickness;
        HorizontalAlignment horizontalContentAlignment = HorizontalContentAlignment;
        VerticalAlignment verticalContentAlignment = VerticalContentAlignment;
        bool useLayoutRounding = UseLayoutRounding;
        Size availableSize = finalSize;
        Size sizeForChild = availableSize;
        double scale = LayoutHelper.GetLayoutScale(this);
        double originX = offset.X;
        double originY = offset.Y;

        if (horizontalContentAlignment != HorizontalAlignment.Stretch)
        {
            sizeForChild = sizeForChild.WithWidth(Math.Min(sizeForChild.Width, DesiredSize.Width));
        }

        if (verticalContentAlignment != VerticalAlignment.Stretch)
        {
            sizeForChild = sizeForChild.WithHeight(Math.Min(sizeForChild.Height, DesiredSize.Height));
        }

        if (useLayoutRounding)
        {
            sizeForChild = LayoutHelper.RoundLayoutSizeUp(sizeForChild, scale, scale);
            availableSize = LayoutHelper.RoundLayoutSizeUp(availableSize, scale, scale);
        }

        switch (horizontalContentAlignment)
        {
            case HorizontalAlignment.Center:
                originX += (availableSize.Width - sizeForChild.Width) * 0.5;
                break;
            case HorizontalAlignment.Right:
                originX += availableSize.Width - sizeForChild.Width;
                break;
        }

        switch (verticalContentAlignment)
        {
            case VerticalAlignment.Center:
                originY += (availableSize.Height - sizeForChild.Height) * 0.5;
                break;
            case VerticalAlignment.Bottom:
                originY += availableSize.Height - sizeForChild.Height;
                break;
        }

        if (useLayoutRounding)
        {
            originX = LayoutHelper.RoundLayoutValue(originX, scale);
            originY = LayoutHelper.RoundLayoutValue(originY, scale);
        }

        Rect boundsForChild =
            new Rect(originX, originY, sizeForChild.Width, sizeForChild.Height).Deflate(padding);

        Child.Arrange(boundsForChild);

        return finalSize;
    }


    private void OnScrollGesture(object? sender, ScrollGestureEventArgs e)
    {
        if (Extent.Height > Viewport.Height || Extent.Width > Viewport.Width)
        {
            ILogicalScrollable? scrollable = Child as ILogicalScrollable;
            bool isLogical = scrollable?.IsLogicalScrollEnabled == true;

            double x = Offset.X;
            double y = Offset.Y;

            Vector delta = default;
            if (isLogical)
                _activeLogicalGestureScrolls?.TryGetValue(e.Id, out delta);
            delta += e.Delta;

            if (Extent.Height > Viewport.Height)
            {
                double dy;
                if (isLogical)
                {
                    double logicalUnits = delta.Y / LogicalScrollItemSize;
                    delta = delta.WithY(delta.Y - logicalUnits * LogicalScrollItemSize);
                    dy = logicalUnits * scrollable!.ScrollSize.Height;
                }
                else
                    dy = delta.Y;


                y += dy;
                y = Math.Max(y, 0);
                y = Math.Min(y, Extent.Height - Viewport.Height);
            }

            if (Extent.Width > Viewport.Width)
            {
                double dx;
                if (isLogical)
                {
                    double logicalUnits = delta.X / LogicalScrollItemSize;
                    delta = delta.WithX(delta.X - logicalUnits * LogicalScrollItemSize);
                    dx = logicalUnits * scrollable!.ScrollSize.Width;
                }
                else
                    dx = delta.X;

                x += dx;
                x = Math.Max(x, 0);
                x = Math.Min(x, Extent.Width - Viewport.Width);
            }

            if (isLogical)
            {
                if (_activeLogicalGestureScrolls == null)
                    _activeLogicalGestureScrolls = new Dictionary<int, Vector>();
                _activeLogicalGestureScrolls[e.Id] = delta;
            }

            Offset = new Vector(x, y);
            e.Handled = true;
        }
    }

    /*private void OnScrollGestureEnded(object sender, ScrollGestureEndedEventArgs e)
        => _activeLogicalGestureScrolls?.Remove(e.Id);*/

    /// <inheritdoc/>
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (Extent.Height > Viewport.Height || Extent.Width > Viewport.Width)
        {
            ILogicalScrollable? scrollable = Child as ILogicalScrollable;
            bool isLogical = scrollable?.IsLogicalScrollEnabled == true;

            Vector currentFromOffset = Offset;
            double x = currentFromOffset.X;
            double y = currentFromOffset.Y;

            if (Extent.Height > Viewport.Height)
            {
                double height = isLogical ? scrollable!.ScrollSize.Height : 50;
                y += -e.Delta.Y * height;
                y = Math.Max(y, 0);
                y = Math.Min(y, Extent.Height - Viewport.Height);
            }

            if (Extent.Width > Viewport.Width)
            {
                double width = isLogical ? scrollable!.ScrollSize.Width : 50;
                x += -e.Delta.X * width;
                x = Math.Max(x, 0);
                x = Math.Min(x, Extent.Width - Viewport.Width);
            }

            Offset = new Vector(x, y);
            e.Handled = true;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == OffsetProperty && !_arranging)
        {
            InvalidateArrange();
        }

        base.OnPropertyChanged(change);
    }

    private void BringIntoViewRequested(object? sender, RequestBringIntoViewEventArgs e)
    {
        e.Handled = BringDescendantIntoView(e.TargetObject, e.TargetRect);
    }

    private void ChildChanged(AvaloniaPropertyChangedEventArgs e)
    {
        UpdateScrollableSubscription((Control?)e.NewValue);

        if (e.OldValue != null)
        {
            Offset = default;
        }
    }

    private void OffsetChanged(AvaloniaPropertyChangedEventArgs e)
    {
        Vector? oldOffset = (Vector?)e.OldValue;
        Vector? newOffset = (Vector?)e.NewValue;
        if (ShouldUseSmoothScrolling(out Easing? ease, out TimeSpan dur))
        {
            StartAnimatingOffset(oldOffset, newOffset, ease!, dur);
        }
        else
        {
            AnimationOffset = newOffset.GetValueOrDefault();
        }
    }
    private void StartAnimatingOffset(Vector? oldOffset, Vector? newOffset, Easing? scrollEasing,
        TimeSpan scrollDuration)
    {
        Vector newOff = newOffset.GetValueOrDefault();

        if (_smoothScrollTimerStarted)
        {
            _animStartOffsetX = _offsetX;
            _animStartOffsetY = _offsetY;
        }

        _offsetX = newOff.X;
        _offsetY = newOff.Y;

        _animDuration = scrollDuration.TotalMilliseconds;
        _animTimeRemaining = _animDuration;


        if (!_smoothScrollTimerStarted)
        {
            Vector oldOff = oldOffset.GetValueOrDefault();

            _animStartOffsetX = oldOff.X;
            _animStartOffsetY = oldOff.Y;

            _currentEasing = scrollEasing;
            _smoothScrollTimerStarted = true;
            _smoothScrollTimer.Start();
        }
    }

    private void SmoothScrollTimer_Elapsed(object? sender, EventArgs e)
    {
        double totalDistanceX = _offsetX - _animStartOffsetX;
        double totalDistanceY = _offsetY - _animStartOffsetY;

        double percentage = 1 - Math.Min(Math.Max(0, _animTimeRemaining / _animDuration), 1);

        if (_currentEasing != null)
        {
            double easedPercentage = _currentEasing.Ease(percentage);

            double currentX = _animStartOffsetX + totalDistanceX * easedPercentage;
            double currentY = _animStartOffsetY + totalDistanceY * easedPercentage;
            _animTimeRemaining--;

            AnimationOffset = new Vector(currentX, currentY);
        }

        if (_animTimeRemaining <= 0)
        {
            //Console.WriteLine("Stopping...");
            _smoothScrollTimerStarted = false;
            _smoothScrollTimer.Stop();
        }
    }

    //private const string AnimPseudoclass = ":animating";

    /*void SetAnimPseudoclass(bool add)
    {
        if (add)
            PseudoClasses.Add(AnimPseudoclass);
        else
            PseudoClasses.Remove(AnimPseudoclass);
    }*/

    private void UpdateScrollableSubscription(Control? child)
    {
        ILogicalScrollable? scrollable = child as ILogicalScrollable;

        _logicalScrollSubscription?.Dispose();
        _logicalScrollSubscription = null;

        if (scrollable != null)
        {
            scrollable.ScrollInvalidated += ScrollInvalidated;

            if (scrollable.IsLogicalScrollEnabled)
            {
                _logicalScrollSubscription = new CompositeDisposable(
                    this.GetObservable(CanHorizontallyScrollProperty)
                        .Subscribe(x => scrollable.CanHorizontallyScroll = x),
                    this.GetObservable(CanVerticallyScrollProperty)
                        .Subscribe(x => scrollable.CanVerticallyScroll = x),
                    this.GetObservable(OffsetProperty)
                        .Skip(1).Subscribe(x => scrollable.Offset = x),
                    Disposable.Create(() => scrollable.ScrollInvalidated -= ScrollInvalidated));
                UpdateFromScrollable(scrollable);
            }
        }
    }

    private void ScrollInvalidated(object? sender, EventArgs e)
    {
        UpdateFromScrollable((ILogicalScrollable)sender!);
    }

    private void UpdateFromScrollable(ILogicalScrollable? scrollable)
    {
        bool logicalScroll = _logicalScrollSubscription != null;

        if (scrollable != null && logicalScroll != scrollable.IsLogicalScrollEnabled)
        {
            UpdateScrollableSubscription(Child);
            Offset = default;
            InvalidateMeasure();
        }
        else if (scrollable is { IsLogicalScrollEnabled: true })
        {
            Viewport = scrollable.Viewport;
            Extent = scrollable.Extent;
            Offset = scrollable.Offset;
        }
    }

    private void EnsureAnchorElementSelection()
    {
        if (!_isAnchorElementDirty || _anchorCandidates is null)
            return;

        _anchorElement = null;
        _anchorElementBounds = default;
        _isAnchorElementDirty = false;

        Control? bestCandidate = default;
        double bestCandidateDistance = double.MaxValue;

        // Find the anchor candidate that is scrolled closest to the top-left of this
        // SmoothScrollContentPresenter.
        foreach (Control element in _anchorCandidates)
        {
            if (element.IsVisible && GetViewportBounds(element, out Rect bounds))
            {
                Vector distance = bounds.Position;
                double candidateDistance = Math.Abs(distance.Length);

                if (candidateDistance < bestCandidateDistance)
                {
                    bestCandidate = element;
                    bestCandidateDistance = candidateDistance;
                }
            }
        }

        if (bestCandidate != null)
        {
            // We have a candidate, calculate its bounds relative to Child. Because these
            // bounds aren't relative to the SmoothScrollContentPresenter itself, if they change
            // then we know it wasn't just due to scrolling.
            Rect unscrolledBounds = TranslateBounds(bestCandidate, Child!);
            _anchorElement = bestCandidate;
            _anchorElementBounds = unscrolledBounds;
        }
    }

    private bool GetViewportBounds(Control element, out Rect bounds)
    {
        if (TranslateBounds(element, Child!, out Rect childBounds))
        {
            // We want the bounds relative to the new Offset, regardless of whether the child
            // control has actually been arranged to this offset yet, so translate first to the
            // child control and then apply Offset rather than translating directly to this
            // control.
            Rect thisBounds = new(Bounds.Size);
            bounds = new Rect(childBounds.Position - CurrentFromOffset, childBounds.Size);
            return bounds.Intersects(thisBounds);
        }

        bounds = default;
        return false;
    }

    private Rect TranslateBounds(Control control, Control to)
    {
        if (TranslateBounds(control, to, out Rect bounds))
        {
            return bounds;
        }

        throw new InvalidOperationException("The control's bounds could not be translated to the requested control.");
    }

    private bool TranslateBounds(Visual control, Visual to, out Rect bounds)
    {
        if (!control.IsVisible)
        {
            bounds = default;
            return false;
        }

        Point? p = control.TranslatePoint(default, to);
        bounds = p.HasValue ? new Rect(p.Value, control.Bounds.Size) : default;
        return p.HasValue;
    }
}