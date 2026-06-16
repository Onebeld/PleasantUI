using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PleasantUI.Core.Extensions;

namespace PleasantUI.Core.Attached;

public class ScrollViewerExtensions : AvaloniaObject
{
    public enum ChangeSize
    {
        Line,
        Page
    }

    public static readonly AttachedProperty<bool> EnableAnimatedScrollProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, bool>("EnableAnimatedScroll", typeof(ScrollViewerExtensions));

    public static readonly AttachedProperty<double> ScrollStepSizeProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, double>("ScrollStepSize", typeof(ScrollViewerExtensions), 100.0);

    public static readonly AttachedProperty<ChangeSize> ScrollChangeSizeProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, ChangeSize>("ScrollChangeSize", typeof(ScrollViewerExtensions));

    private static readonly AttachedProperty<ScrollAnimationState?> AnimationStateProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, ScrollAnimationState?>("AnimationState", typeof(ScrollViewerExtensions));

    public static bool GetEnableAnimatedScroll(ScrollViewer obj) => obj.GetValue(EnableAnimatedScrollProperty);

    public static void SetEnableAnimatedScroll(ScrollViewer obj, bool value) => obj.SetValue(EnableAnimatedScrollProperty, value);

    public static double GetScrollStepSize(ScrollViewer obj) => obj.GetValue(ScrollStepSizeProperty);
    
    public static void SetScrollStepSize(ScrollViewer obj, double value) => obj.SetValue(ScrollStepSizeProperty, value);

    public static ChangeSize GetScrollChangeSize(ScrollViewer obj) => obj.GetValue(ScrollChangeSizeProperty);

    public static void SetScrollChangeSize(ScrollViewer obj, ChangeSize value) => obj.SetValue(ScrollChangeSizeProperty, value);

    static ScrollViewerExtensions()
    {
        EnableAnimatedScrollProperty.Changed.AddClassHandler<ScrollViewer>(OnEnableAnimatedScrollChanged);
    }

    private static void OnEnableAnimatedScrollChanged(ScrollViewer sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
        {
            sender.AddHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged,
                RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            sender.Loaded += ScrollViewer_Loaded;

            ScrollAnimationState state = new(sender);
            sender.SetValue(AnimationStateProperty, state);

            if (sender.Presenter is ScrollContentPresenter scp)
                state.ScrollContentPresenter = scp;
        }
        else
        {
            sender.RemoveHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged);
            sender.Loaded -= ScrollViewer_Loaded;

            ScrollAnimationState? state = sender.GetValue(AnimationStateProperty);
            state?.Reset();
            sender.ClearValue(AnimationStateProperty);
        }
    }

    private static void ScrollViewer_Loaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer)
            return;

        ScrollAnimationState? state = scrollViewer.GetValue(AnimationStateProperty);
        state?.ScrollContentPresenter = scrollViewer.Presenter as ScrollContentPresenter;
    }

    private static void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;

        ScrollAnimationState? state = scrollViewer.GetValue(AnimationStateProperty);
        if (state == null) return;

        state.ScrollContentPresenter ??= scrollViewer.Presenter as ScrollContentPresenter;

        ScrollContentPresenter? scrollContentPresenter = state.ScrollContentPresenter;
        if (scrollContentPresenter == null)
        {
            e.Handled = false;
            return;
        }

        object? src = e.Source;
        while (src != null && !Equals(src, scrollContentPresenter))
        {
            if (src is ScrollContentPresenter contentPresenter)
            {
                if (contentPresenter == scrollContentPresenter) break;

                if ((e.Delta.Y > 0 && contentPresenter.Offset.Y == 0) ||
                    (e.Delta.Y < 0 && Math.Abs(contentPresenter.Offset.Y - (contentPresenter.Extent.Height - contentPresenter.Viewport.Height)) < 0.1))
                    src = contentPresenter.GetVisualParent();
                else
                    return;
            }
            else if (src is Visual visual) 
                src = visual.GetVisualParent();
        }

        if (!Equals(src, scrollContentPresenter))
        {
            e.Handled = !(src as ScrollContentPresenter)?.IsScrollChainingEnabled ?? false;
            return;
        }

        Vector delta = e.Delta;
        double x = scrollContentPresenter.Offset.X;
        double y = scrollContentPresenter.Offset.Y;
        double maxOffsetY = scrollContentPresenter.Extent.Height - scrollContentPresenter.Viewport.Height;
        ILogicalScrollable? scrollable = scrollContentPresenter.Child as ILogicalScrollable;
        bool isLogical = scrollable?.IsLogicalScrollEnabled == true;

        if (scrollContentPresenter.Extent.Height > scrollContentPresenter.Viewport.Height)
        {
            double height = isLogical ? scrollable!.ScrollSize.Height : GetScrollStepSize(scrollViewer);
            y += -delta.Y * height;
            y = Math.Max(y, 0);
            y = Math.Min(y, maxOffsetY);
        }

        Vector newOffset = SnapOffset(scrollContentPresenter, new Vector(x, y), delta, true);
        double step = Math.Abs(newOffset.Y - scrollContentPresenter.Offset.Y);

        ChangeSize changeSize = GetScrollChangeSize(scrollViewer);
        if (delta.Y > 0)
            state.AnimateScroll(changeSize == ChangeSize.Line ? -step : -scrollViewer.Bounds.Height);
        else
            state.AnimateScroll(changeSize == ChangeSize.Line ? step : scrollViewer.Bounds.Height);

        bool offsetChanged = newOffset != scrollContentPresenter.Offset;
        e.Handled = !scrollContentPresenter.IsScrollChainingEnabled || offsetChanged;
    }

    private static Vector SnapOffset(ScrollContentPresenter scp, Vector offset, Vector direction = default, bool snapToNext = false)
    {
        IScrollSnapPointsInfo? scrollable = GetScrollSnapPointsInfo(scp);
        if (scrollable == null || scp.VerticalSnapPointsType == SnapPointsType.None)
            return offset;

        Vector diff = GetAlignmentDiff(scp);
        bool areVerticalSnapPointsRegular = scrollable.AreVerticalSnapPointsRegular;
        IReadOnlyList<double>? verticalSnapPoints = null;
        double verticalSnapPoint = 0;
        double verticalSnapPointOffset = 0;

        if (!areVerticalSnapPointsRegular)
            verticalSnapPoints =
                scrollable.GetIrregularSnapPoints(Orientation.Vertical, scp.VerticalSnapPointsAlignment);
        else
            verticalSnapPoint = scrollable.GetRegularSnapPoints(Orientation.Vertical, scp.VerticalSnapPointsAlignment,
                out verticalSnapPointOffset);

        if ((!areVerticalSnapPointsRegular && (!(verticalSnapPoints?.Count > 0))) || (snapToNext && direction.Y == 0))
            return offset;

        Vector estimatedOffset = new(offset.X, offset.Y + diff.Y);
        double previousSnapPoint = 0, nextSnapPoint = 0, midPoint = 0;

        if (areVerticalSnapPointsRegular)
        {
            previousSnapPoint = (int)(estimatedOffset.Y / verticalSnapPoint) * verticalSnapPoint +
                                verticalSnapPointOffset;
            nextSnapPoint = previousSnapPoint + verticalSnapPoint;
            midPoint = (previousSnapPoint + nextSnapPoint) / 2;
        }
        else if (verticalSnapPoints?.Count > 0)
        {
            (previousSnapPoint, nextSnapPoint) = FindNearestSnapPoint(verticalSnapPoints, estimatedOffset.Y);
            midPoint = (previousSnapPoint + nextSnapPoint) / 2;
        }

        double nearestSnapPoint = snapToNext
            ? (direction.Y > 0 ? previousSnapPoint : nextSnapPoint)
            : estimatedOffset.Y < midPoint
                ? previousSnapPoint
                : nextSnapPoint;

        offset = new Vector(offset.X, nearestSnapPoint - diff.Y);

        return offset;
    }

    private static IScrollSnapPointsInfo? GetScrollSnapPointsInfo(ScrollContentPresenter scp)
    {
        object? scrollable = scp.Content switch
        {
            ItemsControl itemsControl => itemsControl.Presenter?.Panel,
            ItemsPresenter itemsPresenter => itemsPresenter.Panel,
            _ => scp.Content
        };

        return scrollable as IScrollSnapPointsInfo;
    }

    private static Vector GetAlignmentDiff(ScrollContentPresenter scp)
    {
        Vector vector = default;
        switch (scp.VerticalSnapPointsAlignment)
        {
            case SnapPointsAlignment.Center: vector += new Vector(0, scp.Viewport.Height / 2); break;
            case SnapPointsAlignment.Far: vector += new Vector(0, scp.Viewport.Height); break;
        }

        switch (scp.HorizontalSnapPointsAlignment)
        {
            case SnapPointsAlignment.Center: vector += new Vector(scp.Viewport.Width / 2, 0); break;
            case SnapPointsAlignment.Far: vector += new Vector(scp.Viewport.Width, 0); break;
        }

        return vector;
    }

    private static (double previous, double next) FindNearestSnapPoint(IReadOnlyList<double> snapPoints, double value)
    {
        int point = snapPoints.BinarySearch(value, Comparer<double>.Default);
        double previousSnapPoint, nextSnapPoint;
        if (point < 0)
        {
            point = ~point;
            previousSnapPoint = snapPoints[Math.Max(0, point - 1)];
            nextSnapPoint = point >= snapPoints.Count ? snapPoints[^1] : snapPoints[Math.Max(0, point)];
        }
        else
        {
            previousSnapPoint = nextSnapPoint = snapPoints[Math.Max(0, point)];
        }

        return (previousSnapPoint, nextSnapPoint);
    }

    private class ScrollAnimationState
    {
        private const double AnimationDuration = 100;
        
        private readonly ScrollViewer _scrollViewer;
        private readonly DispatcherTimer _timer;
        
        private bool _isAnimating;
        private double _targetOffset;
        private double _startOffset;
        private DateTime _animationStartTime;

        public ScrollContentPresenter? ScrollContentPresenter { get; set; }

        public ScrollAnimationState(ScrollViewer scrollViewer)
        {
            _scrollViewer = scrollViewer;

            _timer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(1)
            };
            _timer.Tick += OnTimerTick;
        }

        public void Reset()
        {
            _isAnimating = false;
            _timer.Stop();
            ScrollContentPresenter = null;
        }

        public void AnimateScroll(double delta)
        {
            DateTime currentTime = DateTime.Now;
            double maxOffset = _scrollViewer.Extent.Height - _scrollViewer.Bounds.Height;
            if (maxOffset < 0) maxOffset = 0;

            if (_isAnimating)
            {
                double elapsedTime = (currentTime - _animationStartTime).TotalMilliseconds;
                double progress = Math.Min(elapsedTime / AnimationDuration, 1.0);

                SineEaseOut easing = new();
                double easedProgress = easing.Ease(progress);

                _startOffset += easedProgress * (_targetOffset - _startOffset);
                _targetOffset += delta;
                _targetOffset = Math.Clamp(_targetOffset, 0, maxOffset);
                _animationStartTime = currentTime;
            }
            else
            {
                _isAnimating = true;
                _startOffset = _scrollViewer.Offset.Y;
                _targetOffset = _startOffset + delta;
                _targetOffset = Math.Clamp(_targetOffset, 0, maxOffset);
                _animationStartTime = currentTime;

                _timer.Start();
            }
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            if (!_isAnimating)
            {
                _timer.Stop();
                return;
            }

            double elapsedTime = (DateTime.Now - _animationStartTime).TotalMilliseconds;

            if (elapsedTime >= AnimationDuration)
            {
                _scrollViewer.Offset = new Vector(_scrollViewer.Offset.X, _targetOffset);
                _isAnimating = false;
                _timer.Stop();
                return;
            }

            double progress = elapsedTime / AnimationDuration;
            SineEaseOut easing = new();
            double easedProgress = easing.Ease(progress);

            double currentOffset = _startOffset + (easedProgress * (_targetOffset - _startOffset));
            _scrollViewer.Offset = new Vector(_scrollViewer.Offset.X, currentOffset);
        }
    }
}