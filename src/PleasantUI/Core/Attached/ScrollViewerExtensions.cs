/*
 * SPDX-FileCopyrightText: 2026 Dmitry Zhutkov (Onebeld) <onebeld@gmail.com>
 * SPDX-FileCopyrightText: 2025 Egolds <https://github.com/Egolds>
 * SPDX-License-Identifier: MIT
 *
 * SPDX-FileComment: A modified version of the VerticalScrollViewerAnimatedBehavior.cs component
 * (original: https://github.com/Egolds/Xaml.Behaviors.Interactions.Animated/blob/master/Xaml.Behaviors.Interactions.Animated/ScrollViewer/VerticalScrollViewerAnimatedBehavior.cs)
 * Changes:
 * 1. Renamed to ScrollViewerExtensions instead of VerticalScrollViewerAnimatedBehavior.
 * 2. Attached properties were used instead of Xaml.Behaviors.Interactivity.
 * 3. Smooth horizontal scrolling has been implemented.
 * 4. Instead of await Task.Delay(), DispatcherTimer was used.
 */

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

/// <summary>
/// A class for extending additional capabilities for <see cref="ScrollViewer"/>
/// </summary>
public class ScrollViewerExtensions : AvaloniaObject
{
    /// <summary>
    /// Specifies the mode in which scrolling should occur.
    /// </summary>
    public enum ChangeSize
    {
        /// <summary>
        /// Scrolling along the line
        /// </summary>
        Line,
        /// <summary>
        /// Full scrolling of visible content
        /// </summary>
        Page
    }
    
    /// <summary>
    /// Specifies the type of scrolling direction when using a mouse.
    /// </summary>
    public enum ScrollOrientation
    {
        /// <summary>
        /// Vertical mouse wheel scrolling, original behavior
        /// </summary>
        Vertical,
        /// <summary>
        /// Scrolling horizontally with the mouse wheel
        /// </summary>
        Horizontal
    }

    /// <summary>
    /// Defines the EnableAnimatedScroll attached property.
    /// </summary>
    public static readonly AttachedProperty<bool> EnableAnimatedScrollProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, bool>("EnableAnimatedScroll", typeof(ScrollViewerExtensions));

    /// <summary>
    /// Defines the ScrollStepSize attached property.
    /// </summary>
    public static readonly AttachedProperty<double> ScrollStepSizeProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, double>("ScrollStepSize", typeof(ScrollViewerExtensions), 100.0);

    /// <summary>
    /// Defines the ScrollChangeSize attached property.
    /// </summary>
    public static readonly AttachedProperty<ChangeSize> ScrollChangeSizeProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, ChangeSize>("ScrollChangeSize", typeof(ScrollViewerExtensions));

    /// <summary>
    /// Defines the AnimationState attached property.
    /// </summary>
    private static readonly AttachedProperty<ScrollAnimationState?> AnimationStateProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, ScrollAnimationState?>("AnimationState", typeof(ScrollViewerExtensions));
    
    /// <summary>
    /// Defines the ScrollOrientation attached property.
    /// </summary>
    public static readonly AttachedProperty<ScrollOrientation> ScrollOrientationProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, ScrollOrientation>("ScrollOrientation", typeof(ScrollViewerExtensions));

    /// <summary>
    /// Gets whether smooth scrolling is enabled.
    /// </summary>
    /// <param name="obj">Original control</param>
    /// <returns>Smooth scrolling is enabled</returns>
    public static bool GetEnableAnimatedScroll(ScrollViewer obj) => obj.GetValue(EnableAnimatedScrollProperty);

    /// <summary>
    /// Sets whether smooth scrolling is enabled.
    /// </summary>
    /// <param name="obj">Original control</param>
    /// <param name="value">Smooth scrolling is enabled</param>
    public static void SetEnableAnimatedScroll(ScrollViewer obj, bool value) => obj.SetValue(EnableAnimatedScrollProperty, value);

    /// <summary>
    /// Gets the scroll step size for the <see cref="ChangeSize.Line"/> scroll type.
    /// </summary>
    /// <param name="obj">Original control</param>
    /// <returns>Scroll step size</returns>
    public static double GetScrollStepSize(ScrollViewer obj) => obj.GetValue(ScrollStepSizeProperty);
    
    /// <summary>
    /// Sets the scroll step size for the <see cref="ChangeSize.Line"/> scroll type.
    /// </summary>
    /// <param name="obj">Original control</param>
    /// <param name="value">Scroll step size</param>
    public static void SetScrollStepSize(ScrollViewer obj, double value) => obj.SetValue(ScrollStepSizeProperty, value);

    /// <summary>
    /// Gets the scroll mode
    /// </summary>
    /// <param name="obj">Original control</param>
    /// <returns>Scroll mode</returns>
    public static ChangeSize GetScrollChangeSize(ScrollViewer obj) => obj.GetValue(ScrollChangeSizeProperty);

    /// <summary>
    /// Sets the scroll mode
    /// </summary>
    /// <param name="obj">Original control</param>
    /// <param name="value">Scroll mode</param>
    public static void SetScrollChangeSize(ScrollViewer obj, ChangeSize value) => obj.SetValue(ScrollChangeSizeProperty, value);
    
    /// <summary>
    /// Gets the scroll orientation
    /// </summary>
    /// <param name="obj">Original control</param>
    /// <returns>Scroll orientation</returns>
    public static ScrollOrientation GetScrollOrientation(ScrollViewer obj) => obj.GetValue(ScrollOrientationProperty);
    
    /// <summary>
    /// Sets the scroll orientation
    /// </summary>
    /// <param name="obj">Original control</param>
    /// <param name="value">Scroll orientation</param>
    public static void SetScrollOrientation(ScrollViewer obj, ScrollOrientation value) => obj.SetValue(ScrollOrientationProperty, value);

    static ScrollViewerExtensions()
    {
        EnableAnimatedScrollProperty.Changed.AddClassHandler<ScrollViewer>(OnEnableAnimatedScrollChanged);
    }

    private static void OnEnableAnimatedScrollChanged(ScrollViewer sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
        {
            sender.AddHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged,
                RoutingStrategies.Tunnel);
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

        ScrollContentPresenter? scp = state.ScrollContentPresenter;
        if (scp == null) return;

        object? src = e.Source;
        while (src != null && !Equals(src, scp))
        {
            if (src is ScrollContentPresenter scp2)
            {
                if (scp2 == scp) break;
                
                bool isAtYMax = e.Delta.Y > 0 ? scp2.Offset.Y == 0 : Math.Abs(scp2.Offset.Y - (scp2.Extent.Height - scp2.Viewport.Height)) < 0.1;
                bool isAtXMax = e.Delta.X > 0 ? scp2.Offset.X == 0 : Math.Abs(scp2.Offset.X - (scp2.Extent.Width - scp2.Viewport.Width)) < 0.1;

                if (isAtYMax && isAtXMax)
                    src = scp2.GetVisualParent();
                else
                    return;
            }
            else if (src is Visual visual)
            {
                src = visual.GetVisualParent();
            }
        }

        if (!Equals(src, scp))
        {
            e.Handled = !(src as ScrollContentPresenter)?.IsScrollChainingEnabled ?? false;
            return;
        }

        double deltaX = e.Delta.X;
        double deltaY = e.Delta.Y;
        
        ScrollOrientation orientation = GetScrollOrientation(scrollViewer);
        bool isShiftPressed = e.KeyModifiers.HasFlag(KeyModifiers.Shift);

        if (orientation == ScrollOrientation.Horizontal)
        {
            if (deltaX == 0 && deltaY != 0)
            {
                if (!isShiftPressed)
                {
                    deltaX = deltaY;
                    deltaY = 0;
                }
            }
        }
        else
        {
            if (deltaX == 0 && deltaY != 0 && isShiftPressed)
            {
                deltaX = deltaY;
                deltaY = 0;
            }
        }

        double maxOffsetY = scp.Extent.Height - scp.Viewport.Height;
        double maxOffsetX = scp.Extent.Width - scp.Viewport.Width;
        
        ILogicalScrollable? scrollable = scp.Child as ILogicalScrollable;
        bool isLogical = scrollable?.IsLogicalScrollEnabled == true;

        double stepSize = GetScrollStepSize(scrollViewer);
        double stepX = 0;
        double stepY = 0;

        if (deltaY != 0 && scp.Extent.Height > scp.Viewport.Height)
        {
            double height = isLogical ? scrollable!.ScrollSize.Height : stepSize;
            double targetY = Math.Clamp(scp.Offset.Y + (-deltaY * height), 0, maxOffsetY);
            Vector snapped = SnapOffset(scp, new Vector(scp.Offset.X, targetY), new Vector(0, deltaY), true);
            stepY = snapped.Y - scp.Offset.Y;
        }

        if (deltaX != 0 && scp.Extent.Width > scp.Viewport.Width)
        {
            double width = isLogical ? scrollable!.ScrollSize.Width : stepSize;
            double targetX = Math.Clamp(scp.Offset.X + (-deltaX * width), 0, maxOffsetX);
            Vector snapped = SnapOffset(scp, new Vector(targetX, scp.Offset.Y), new Vector(deltaX, 0), true);
            stepX = snapped.X - scp.Offset.X;
        }

        if (stepX != 0 || stepY != 0)
        {
            ChangeSize changeSize = GetScrollChangeSize(scrollViewer);
            double finalDeltaX = stepX;
            double finalDeltaY = stepY;

            if (changeSize == ChangeSize.Page)
            {
                if (deltaX != 0) finalDeltaX = deltaX > 0 ? -scrollViewer.Bounds.Width : scrollViewer.Bounds.Width;
                if (deltaY != 0) finalDeltaY = deltaY > 0 ? -scrollViewer.Bounds.Height : scrollViewer.Bounds.Height;
            }

            state.AnimateScroll(finalDeltaX, finalDeltaY);
        }

        bool offsetChanged = (stepX != 0 || stepY != 0);
        e.Handled = !scp.IsScrollChainingEnabled || offsetChanged;
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
        private Vector _targetOffset;
        private Vector _startOffset;
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

        public void AnimateScroll(double deltaX, double deltaY)
        {
            DateTime currentTime = DateTime.Now;
            Vector maxOffset = new(
                Math.Max(0, _scrollViewer.Extent.Width - _scrollViewer.Bounds.Width),
                Math.Max(0, _scrollViewer.Extent.Height - _scrollViewer.Bounds.Height)
            );

            if (_isAnimating)
            {
                double elapsedTime = (currentTime - _animationStartTime).TotalMilliseconds;
                double progress = Math.Min(elapsedTime / AnimationDuration, 1.0);

                SineEaseOut easing = new();
                double easedProgress = easing.Ease(progress);

                _startOffset += easedProgress * (_targetOffset - _startOffset);
                _targetOffset = new Vector(
                    Math.Clamp(_targetOffset.X + deltaX, 0, maxOffset.X),
                    Math.Clamp(_targetOffset.Y + deltaY, 0, maxOffset.Y)
                );
                _animationStartTime = currentTime;
            }
            else
            {
                _isAnimating = true;
                _startOffset = _scrollViewer.Offset;
                _targetOffset = new Vector(
                    Math.Clamp(_startOffset.X + deltaX, 0, maxOffset.X),
                    Math.Clamp(_startOffset.Y + deltaY, 0, maxOffset.Y)
                );
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
                _scrollViewer.Offset = _targetOffset;
                _isAnimating = false;
                _timer.Stop();
                return;
            }

            double progress = elapsedTime / AnimationDuration;
            SineEaseOut easing = new();
            double easedProgress = easing.Ease(progress);

            Vector currentOffset = _startOffset + (easedProgress * (_targetOffset - _startOffset));
            _scrollViewer.Offset = currentOffset;
        }
    }
}