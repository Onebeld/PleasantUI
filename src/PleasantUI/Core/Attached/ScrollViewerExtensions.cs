/*
 * SPDX-FileCopyrightText: 2026 Dmitry Zhutkov (Onebeld) <onebeld@gmail.com>
 * SPDX-FileCopyrightText: 2025 Egolds <https://github.com/Egolds>
 * SPDX-License-Identifier: MIT
 *
 * Modified from original source:
 * https://github.com/Egolds/Xaml.Behaviors.Interactions.Animated/blob/master/Xaml.Behaviors.Interactions.Animated/ScrollViewer/VerticalScrollViewerAnimatedBehavior.cs
 * 
 * Changes:
 * 1. Renamed to ScrollViewerExtensions instead of VerticalScrollViewerAnimatedBehavior.
 * 2. Attached properties were used instead of Xaml.Behaviors.Interactivity.
 * 3. Smooth horizontal scrolling has been implemented.
 * 4. Instead of await Task.Delay(), DispatcherTimer was used.
 * 5. Turning smooth scrolling on or off.
 * 6. Possibility of horizontal scrolling using the mouse wheel.
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
using PleasantUI.Core.Internal.Extensions;

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
    /// Defines the HandleCustomScroll attached property.
    /// </summary>
    public static readonly AttachedProperty<bool> HandleCustomScrollProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, bool>("HandleCustomScroll", typeof(ScrollViewerExtensions));

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
    /// Gets whether custom behavior is enabled for the <see cref="ScrollViewer"/>.
    /// </summary>
    /// <param name="obj">Original control</param>
    /// <returns>Custom behavior is enabled</returns>
    public static bool GetHandleCustomScroll(ScrollViewer obj) => obj.GetValue(HandleCustomScrollProperty);
    
    /// <summary>
    /// Sets whether custom behavior is enabled for the <see cref="ScrollViewer"/>.
    /// </summary>
    /// <param name="obj">Original control</param>
    /// <param name="value">Custom behavior is enabled</param>
    public static void SetHandleCustomScroll(ScrollViewer obj, bool value) => obj.SetValue(HandleCustomScrollProperty, value);

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
        HandleCustomScrollProperty.Changed.AddClassHandler<ScrollViewer>(OnHandleCustomScrollChanged);
    }

    private static void OnHandleCustomScrollChanged(ScrollViewer sender, AvaloniaPropertyChangedEventArgs e)
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
        if (sender is not ScrollViewer currentScrollViewer) return;
        
        double deltaX = e.Delta.X;
        double deltaY = e.Delta.Y;
    
        ScrollOrientation orientation = GetScrollOrientation(currentScrollViewer);
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
        
        Vector realDelta = new(deltaX, deltaY);
        
        ScrollViewer? activeChildScrollViewer = null;
        IEnumerable<Visual> descendants = currentScrollViewer.GetVisualDescendants();
        foreach (Visual visual in descendants)
        {
            if (visual is ScrollViewer childScrollViewer && childScrollViewer != currentScrollViewer)
            {
                if (childScrollViewer.IsPointerOver && GetHandleCustomScroll(childScrollViewer))
                {
                    activeChildScrollViewer = childScrollViewer;
                    break;
                }
            }
        }

        if (activeChildScrollViewer?.Presenter is ScrollContentPresenter childScp)
        {
            if (CanScroll(childScp, realDelta))
                return;
        }

        ScrollAnimationState? state = currentScrollViewer.GetValue(AnimationStateProperty);
        if (state == null) return;

        state.ScrollContentPresenter ??= currentScrollViewer.Presenter as ScrollContentPresenter;

        ScrollContentPresenter? currentScp = state.ScrollContentPresenter;
        if (currentScp == null) return;

        double maxOffsetY = currentScp.Extent.Height - currentScp.Viewport.Height;
        double maxOffsetX = currentScp.Extent.Width - currentScp.Viewport.Width;
        
        ILogicalScrollable? scrollable = currentScp.Child as ILogicalScrollable;
        bool isLogical = scrollable?.IsLogicalScrollEnabled == true;

        double stepSize = GetScrollStepSize(currentScrollViewer);
        double stepX = 0;
        double stepY = 0;

        if (realDelta.Y != 0 && currentScp.Extent.Height > currentScp.Viewport.Height)
        {
            double height = isLogical ? scrollable!.ScrollSize.Height : stepSize;
            double targetY = Math.Clamp(currentScp.Offset.Y + (-realDelta.Y * height), 0, maxOffsetY);
            stepY = targetY - currentScp.Offset.Y;
        }

        if (realDelta.X != 0 && currentScp.Extent.Width > currentScp.Viewport.Width)
        {
            double width = isLogical ? scrollable!.ScrollSize.Width : stepSize;
            double targetX = Math.Clamp(currentScp.Offset.X + (-realDelta.X * width), 0, maxOffsetX);
            stepX = targetX - currentScp.Offset.X;
        }

        if (stepX != 0 || stepY != 0)
        {
            if (!GetEnableAnimatedScroll(currentScrollViewer) && activeChildScrollViewer != null)
            {
                if (activeChildScrollViewer.Presenter is ScrollContentPresenter childScp1 && CanScroll(childScp1, realDelta))
                    return;
            }
            
            ChangeSize changeSize = GetScrollChangeSize(currentScrollViewer);
            double finalDeltaX = stepX;
            double finalDeltaY = stepY;

            if (changeSize == ChangeSize.Page)
            {
                if (realDelta.X != 0) finalDeltaX = realDelta.X > 0 ? -currentScrollViewer.Bounds.Width : currentScrollViewer.Bounds.Width;
                if (realDelta.Y != 0) finalDeltaY = realDelta.Y > 0 ? -currentScrollViewer.Bounds.Height : currentScrollViewer.Bounds.Height;
            }
            
            if (GetEnableAnimatedScroll(currentScrollViewer))
                state.AnimateScroll(finalDeltaX, finalDeltaY);
            else
            {
                state.Reset();
                currentScrollViewer.Offset = new Vector(
                    Math.Clamp(currentScrollViewer.Offset.X + finalDeltaX, 0, maxOffsetX),
                    Math.Clamp(currentScrollViewer.Offset.Y + finalDeltaY, 0, maxOffsetY)
                );
            }

            e.Handled = true;
        }
        else
        {
            if (!currentScp.IsScrollChainingEnabled) 
                e.Handled = true;
        }
    }
    
    private static bool CanScroll(ScrollContentPresenter scp, Vector delta)
    {
        const double epsilon = 0.5;

        if (delta.Y > 0 && scp.Offset.Y > epsilon) return true;
        if (delta.Y < 0 && scp.Offset.Y < (scp.Extent.Height - scp.Viewport.Height) - epsilon) return true;
        if (delta.X > 0 && scp.Offset.X > epsilon) return true;
        if (delta.X < 0 && scp.Offset.X < (scp.Extent.Width - scp.Viewport.Width) - epsilon) return true;

        return false;
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

            if (!_isAnimating)
            {
                _startOffset = _scrollViewer.Offset;
                _targetOffset = _startOffset;
            }

            if (_isAnimating)
            {
                Vector currentDirection = _targetOffset - _startOffset;
                
                if ((deltaX > 0 && currentDirection.X < 0) || (deltaX < 0 && currentDirection.X > 0) ||
                    (deltaY > 0 && currentDirection.Y < 0) || (deltaY < 0 && currentDirection.Y > 0))
                {
                    _startOffset = _scrollViewer.Offset;
                    _targetOffset = _startOffset;
                }
                else
                {
                    double elapsedTime = (currentTime - _animationStartTime).TotalMilliseconds;
                    double progress = Math.Min(elapsedTime / AnimationDuration, 1.0);
                    SineEaseOut easing = new();
                    _startOffset += easing.Ease(progress) * (_targetOffset - _startOffset);
                }
        
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