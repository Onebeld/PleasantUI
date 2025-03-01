using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using PleasantUI.Reactive;

namespace PleasantUI.Controls;

/// <summary>
/// Provides smooth scrolling behavior for a scrollable content.
/// </summary>
[TemplatePart("PART_ScrollDecreaseButton", typeof(RepeatButton))]
[TemplatePart("PART_ScrollIncreaseButton", typeof(RepeatButton))]
[TemplatePart("PART_HorizontalScrollBar", typeof(ScrollBar))]
[TemplatePart("PART_VerticalScrollBar", typeof(ScrollBar))]
public class SmoothScrollViewer : ContentControl, IScrollable, IScrollAnchorProvider
{
    private const double DefaultSmallChange = 16;

    private IDisposable? _childSubscription;
    private Size _extent;
    private bool _isExpanded;
    private Size _largeChange;
    private ILogicalScrollable? _logicalScrollable;
    private Vector _offset;
    private Size _oldExtent;
    private Vector _oldOffset;
    private Size _oldViewport;
    private IDisposable? _scrollBarExpandSubscription;
    private Size _smallChange = new(DefaultSmallChange, DefaultSmallChange);
    private Size _viewport;

    /// <summary>
    /// Defines the <see cref="CanHorizontallyScroll" /> property.
    /// </summary>
    /// <remarks>
    /// There is no public C# accessor for this property as it is intended to be bound to by a
    /// <see cref="ScrollContentPresenter" /> in the control's template.
    /// </remarks>
    public static readonly DirectProperty<SmoothScrollViewer, bool> CanHorizontallyScrollProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, bool>(
            nameof(CanHorizontallyScroll),
            o => o.CanHorizontallyScroll);

    /// <summary>
    /// Defines the <see cref="CanVerticallyScroll" /> property.
    /// </summary>
    /// <remarks>
    /// There is no public C# accessor for this property as it is intended to be bound to by a
    /// <see cref="ScrollContentPresenter" /> in the control's template.
    /// </remarks>
    public static readonly DirectProperty<SmoothScrollViewer, bool> CanVerticallyScrollProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, bool>(
            nameof(CanVerticallyScroll),
            o => o.CanVerticallyScroll);

    /// <summary>
    /// Defines the <see cref="Extent" /> property.
    /// </summary>
    public static readonly DirectProperty<SmoothScrollViewer, Size> ExtentProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, Size>(nameof(Extent),
            o => o.Extent,
            (o, v) => o.Extent = v);

    /// <summary>
    /// Defines the <see cref="Offset" /> property.
    /// </summary>
    public static readonly DirectProperty<SmoothScrollViewer, Vector> OffsetProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, Vector>(
            nameof(Offset),
            o => o.Offset,
            (o, v) => o.Offset = v);

    /// <summary>
    /// Defines the <see cref="SmoothScrollEasing" /> property.
    /// </summary>
    public static readonly StyledProperty<Easing?> SmoothScrollEasingProperty =
        AvaloniaProperty.Register<SmoothScrollViewer, Easing?>(nameof(SmoothScrollEasing));

    /// <summary>
    /// Defines the <see cref="SmoothScrollDuration" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> SmoothScrollDurationProperty =
        AvaloniaProperty.Register<SmoothScrollViewer, TimeSpan>(nameof(SmoothScrollDuration));

    /// <summary>
    /// Defines the <see cref="Viewport" /> property.
    /// </summary>
    public static readonly DirectProperty<SmoothScrollViewer, Size> ViewportProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, Size>(nameof(Viewport),
            o => o.Viewport,
            (o, v) => o.Viewport = v);

    /// <summary>
    /// Defines the <see cref="LargeChange" /> property.
    /// </summary>
    public static readonly DirectProperty<SmoothScrollViewer, Size> LargeChangeProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, Size>(
            nameof(LargeChange),
            o => o.LargeChange);

    /// <summary>
    /// Defines the <see cref="SmallChange" /> property.
    /// </summary>
    public static readonly DirectProperty<SmoothScrollViewer, Size> SmallChangeProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, Size>(
            nameof(SmallChange),
            o => o.SmallChange);

    /// <summary>
    /// Defines the HorizontalScrollBarMaximum property.
    /// </summary>
    /// <remarks>
    /// There is no public C# accessor for this property as it is intended to be bound to by a
    /// <see cref="ScrollContentPresenter" /> in the control's template.
    /// </remarks>
    public static readonly DirectProperty<SmoothScrollViewer, double> HorizontalScrollBarMaximumProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, double>(
            nameof(HorizontalScrollBarMaximum),
            o => o.HorizontalScrollBarMaximum);

    /// <summary>
    /// Defines the HorizontalScrollBarValue property.
    /// </summary>
    /// <remarks>
    /// There is no public C# accessor for this property as it is intended to be bound to by a
    /// <see cref="ScrollContentPresenter" /> in the control's template.
    /// </remarks>
    public static readonly DirectProperty<SmoothScrollViewer, double> HorizontalScrollBarValueProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, double>(
            nameof(HorizontalScrollBarValue),
            o => o.HorizontalScrollBarValue,
            (o, v) => o.HorizontalScrollBarValue = v);

    /// <summary>
    /// Defines the HorizontalScrollBarViewportSize property.
    /// </summary>
    /// <remarks>
    /// There is no public C# accessor for this property as it is intended to be bound to by a
    /// <see cref="ScrollContentPresenter" /> in the control's template.
    /// </remarks>
    public static readonly DirectProperty<SmoothScrollViewer, double> HorizontalScrollBarViewportSizeProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, double>(
            nameof(HorizontalScrollBarViewportSize),
            o => o.HorizontalScrollBarViewportSize);

    /// <summary>
    /// Defines the <see cref="HorizontalScrollBarVisibility" /> property.
    /// </summary>
    public static readonly AttachedProperty<ScrollBarVisibility> HorizontalScrollBarVisibilityProperty =
        AvaloniaProperty.RegisterAttached<SmoothScrollViewer, Control, ScrollBarVisibility>(
            nameof(HorizontalScrollBarVisibility));

    /// <summary>
    /// Defines the VerticalScrollBarMaximum property.
    /// </summary>
    /// <remarks>
    /// There is no public C# accessor for this property as it is intended to be bound to by a
    /// <see cref="ScrollContentPresenter" /> in the control's template.
    /// </remarks>
    public static readonly DirectProperty<SmoothScrollViewer, double> VerticalScrollBarMaximumProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, double>(
            nameof(VerticalScrollBarMaximum),
            o => o.VerticalScrollBarMaximum);

    /// <summary>
    /// Defines the VerticalScrollBarValue property.
    /// </summary>
    /// <remarks>
    /// There is no public C# accessor for this property as it is intended to be bound to by a
    /// <see cref="ScrollContentPresenter" /> in the control's template.
    /// </remarks>
    public static readonly DirectProperty<SmoothScrollViewer, double> VerticalScrollBarValueProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, double>(
            nameof(VerticalScrollBarValue),
            o => o.VerticalScrollBarValue,
            (o, v) => o.VerticalScrollBarValue = v);

    /// <summary>
    /// Defines the VerticalScrollBarViewportSize property.
    /// </summary>
    /// <remarks>
    /// There is no public C# accessor for this property as it is intended to be bound to by a
    /// <see cref="ScrollContentPresenter" /> in the control's template.
    /// </remarks>
    public static readonly DirectProperty<SmoothScrollViewer, double> VerticalScrollBarViewportSizeProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, double>(
            nameof(VerticalScrollBarViewportSize),
            o => o.VerticalScrollBarViewportSize);

    /// <summary>
    /// Defines the <see cref="VerticalScrollBarVisibility" /> property.
    /// </summary>
    public static readonly AttachedProperty<ScrollBarVisibility> VerticalScrollBarVisibilityProperty =
        AvaloniaProperty.RegisterAttached<SmoothScrollViewer, Control, ScrollBarVisibility>(
            nameof(VerticalScrollBarVisibility),
            ScrollBarVisibility.Auto);

    /// <summary>
    ///  Defines the <see cref="IsExpandedProperty" /> property.
    /// </summary>
    public static readonly DirectProperty<SmoothScrollViewer, bool> IsExpandedProperty =
        ScrollBar.IsExpandedProperty.AddOwner<SmoothScrollViewer>(o => o.IsExpanded);

    /// <summary>
    /// Defines the <see cref="AllowAutoHide" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> AllowAutoHideProperty =
        ScrollBar.AllowAutoHideProperty.AddOwner<SmoothScrollViewer>();

    /// <summary>
    /// Gets the value of the <see cref="VisibleMaximumProperty" /> property.
    /// </summary>
    public static readonly DirectProperty<SmoothScrollViewer, bool> VisibleMaximumProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, bool>(
            nameof(VisibleMaximum),
            o => o.VisibleMaximum);

    /// <summary>
    /// Gets the value of the <see cref="HorizontalScrollBarEnableDecreaseProperty" /> property.
    /// </summary>
    public static readonly DirectProperty<SmoothScrollViewer, bool> HorizontalScrollBarEnableDecreaseProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, bool>(
            nameof(HorizontalScrollBarEnableDecrease),
            o => o.HorizontalScrollBarEnableDecrease);

    /// <summary>
    /// Gets the value of the <see cref="HorizontalScrollBarEnableIncreaseProperty" /> property.
    /// </summary>
    public static readonly DirectProperty<SmoothScrollViewer, bool> HorizontalScrollBarEnableIncreaseProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, bool>(
            nameof(HorizontalScrollBarEnableIncrease),
            o => o.HorizontalScrollBarEnableIncrease);

    /// <summary>
    /// Defines the <see cref="ScrollChanged" /> event.
    /// </summary>
    public static readonly RoutedEvent<ScrollChangedEventArgs> ScrollChangedEvent =
        RoutedEvent.Register<SmoothScrollViewer, ScrollChangedEventArgs>(
            nameof(ScrollChanged),
            RoutingStrategies.Bubble);

    /// <summary>
    /// Gets or sets the current scroll easing function.
    /// </summary>
    public Easing? SmoothScrollEasing
    {
        get => GetValue(SmoothScrollEasingProperty);
        set => SetValue(SmoothScrollEasingProperty, value);
    }

    /// <summary>
    /// Gets or sets the current smooth-scroll duration.
    /// </summary>
    public TimeSpan SmoothScrollDuration
    {
        get => GetValue(SmoothScrollDurationProperty);
        set => SetValue(SmoothScrollDurationProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal scrollbar visibility.
    /// </summary>
    public ScrollBarVisibility HorizontalScrollBarVisibility
    {
        get => GetValue(HorizontalScrollBarVisibilityProperty);
        set => SetValue(HorizontalScrollBarVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical scrollbar visibility.
    /// </summary>
    public ScrollBarVisibility VerticalScrollBarVisibility
    {
        get => GetValue(VerticalScrollBarVisibilityProperty);
        set => SetValue(VerticalScrollBarVisibilityProperty, value);
    }

    /// <summary>
    /// Gets a value indicating whether the viewer can scroll horizontally.
    /// </summary>
    private bool CanHorizontallyScroll => HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled;

    /// <summary>
    /// Gets a value indicating whether the viewer can scroll vertically.
    /// </summary>
    private bool CanVerticallyScroll => VerticalScrollBarVisibility != ScrollBarVisibility.Disabled;

    /// <summary>
    /// Gets the maximum horizontal scrollbar value.
    /// </summary>
    public double HorizontalScrollBarMaximum => Max(_extent.Width - _viewport.Width, 0);

    /// <summary>
    /// Gets a value indicating whether the horizontal scrollbar maximum is greater than 1.
    /// </summary>
    public bool VisibleMaximum => Math.Round(HorizontalScrollBarMaximum) > 1;

    /// <summary>
    /// Gets a value indicating whether the horizontal scrollbar decrease button should be enabled.
    /// </summary>
    public bool HorizontalScrollBarEnableDecrease => Offset.X != 0;

    /// <summary>
    /// Gets a value indicating whether the horizontal scrollbar increase button should be enabled.
    /// </summary>
    public bool HorizontalScrollBarEnableIncrease => Offset.X != HorizontalScrollBarMaximum;

    /// <summary>
    /// Gets the large (page) change value for the scroll viewer.
    /// </summary>
    public Size LargeChange => _largeChange;

    /// <summary>
    /// Gets the small (line) change value for the scroll viewer.
    /// </summary>
    public Size SmallChange => _smallChange;

    /// <summary>
    /// Gets or sets the horizontal scrollbar value.
    /// </summary>
    private double HorizontalScrollBarValue
    {
        get => _offset.X;
        set
        {
            if (_offset.X != value)
            {
                double old = Offset.X;
                Offset = Offset.WithX(value);
                OnPropertyChanged(new AvaloniaPropertyChangedEventArgs<double>(this, HorizontalScrollBarValueProperty,
                    old, value, BindingPriority.Style));
            }
        }
    }

    /// <summary>
    /// Gets the size of the horizontal scrollbar viewport.
    /// </summary>
    private double HorizontalScrollBarViewportSize => _viewport.Width;

    /// <summary>
    /// Gets the maximum vertical scrollbar value.
    /// </summary>
    public double VerticalScrollBarMaximum => Max(_extent.Height - _viewport.Height, 0);

    /// <summary>
    /// Gets or sets the vertical scrollbar value.
    /// </summary>
    private double VerticalScrollBarValue
    {
        get => _offset.Y;
        set
        {
            if (_offset.Y != value)
            {
                double old = Offset.Y;
                Offset = Offset.WithY(value);
                OnPropertyChanged(new AvaloniaPropertyChangedEventArgs<double>(this, VerticalScrollBarValueProperty,
                    old, value, BindingPriority.Style));
            }
        }
    }

    /// <summary>
    /// Gets the size of the vertical scrollbar viewport.
    /// </summary>
    private double VerticalScrollBarViewportSize => _viewport.Height;

    /// <summary>
    /// Gets a value that indicates whether any scrollbar is expanded.
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        private set => SetAndRaise(ScrollBar.IsExpandedProperty, ref _isExpanded, value);
    }

    /// <summary>
    /// Gets a value that indicates whether scrollbars can hide itself when user is not interacting with it.
    /// </summary>
    public bool AllowAutoHide
    {
        get => GetValue(AllowAutoHideProperty);
        set => SetValue(AllowAutoHideProperty, value);
    }

    /// <summary>
    /// Gets the extent of the scrollable content.
    /// </summary>
    public Size Extent
    {
        get => _extent;
        private set
        {
            if (SetAndRaise(ExtentProperty, ref _extent, value)) CalculatedPropertiesChanged();
        }
    }

    /// <summary>
    /// Gets or sets the current scroll offset.
    /// </summary>
    public Vector Offset
    {
        get => _offset;
        set
        {
            if (SetAndRaise(OffsetProperty, ref _offset, CoerceOffset(Extent, Viewport, value)))
                CalculatedPropertiesChanged();
        }
    }

    /// <summary>
    /// Gets the size of the viewport on the scrollable content.
    /// </summary>
    public Size Viewport
    {
        get => _viewport;
        private set
        {
            if (SetAndRaise(ViewportProperty, ref _viewport, value)) CalculatedPropertiesChanged();
        }
    }

    /// <summary>
    /// Initializes static members of the <see cref="SmoothScrollViewer" /> class.
    /// </summary>
    static SmoothScrollViewer()
    {
        HorizontalScrollBarVisibilityProperty.Changed.AddClassHandler<SmoothScrollViewer>((x, e) =>
            x.ScrollBarVisibilityChanged(e));
        VerticalScrollBarVisibilityProperty.Changed.AddClassHandler<SmoothScrollViewer>((x, e) =>
            x.ScrollBarVisibilityChanged(e));

        AffectsMeasure<SmoothScrollViewer>(SmoothScrollEasingProperty, SmoothScrollDurationProperty);
        AffectsArrange<SmoothScrollViewer>(SmoothScrollEasingProperty, SmoothScrollDurationProperty);
        AffectsRender<SmoothScrollViewer>(SmoothScrollEasingProperty, SmoothScrollDurationProperty);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmoothScrollViewer" /> class.
    /// </summary>
    public SmoothScrollViewer()
    {
        LayoutUpdated += OnLayoutUpdated;
    }

    /// <inheritdoc />
    public void RegisterAnchorCandidate(Control element)
    {
        (Presenter as IScrollAnchorProvider)?.RegisterAnchorCandidate(element);
    }

    /// <inheritdoc />
    public void UnregisterAnchorCandidate(Control element)
    {
        (Presenter as IScrollAnchorProvider)?.UnregisterAnchorCandidate(element);
    }

    /// <inheritdoc />
    public Control? CurrentAnchor => (Presenter as IScrollAnchorProvider)?.CurrentAnchor;

    /// <summary>
    /// Occurs when changes are detected to the scroll position, extent, or viewport size.
    /// </summary>
    public event EventHandler<ScrollChangedEventArgs> ScrollChanged
    {
        add => AddHandler(ScrollChangedEvent, value);
        remove => RemoveHandler(ScrollChangedEvent, value);
    }

    /// <summary>
    /// Scrolls the content up one line.
    /// </summary>
    public void LineUp()
    {
        Offset -= new Vector(0, _smallChange.Height);
    }

    /// <summary>
    /// Scrolls the content down one line.
    /// </summary>
    public void LineDown()
    {
        Offset += new Vector(0, _smallChange.Height);
    }

    /// <summary>
    /// Scrolls the content left one line.
    /// </summary>
    public void LineLeft()
    {
        Offset -= new Vector(_smallChange.Width, 0);
    }

    /// <summary>
    /// Scrolls the content right one line.
    /// </summary>
    public void LineRight()
    {
        Offset += new Vector(_smallChange.Width, 0);
    }

    /// <summary>
    /// Scrolls the content upward by one page.
    /// </summary>
    public void PageUp()
    {
        VerticalScrollBarValue = Math.Max(_offset.Y - _viewport.Height, 0);
    }

    /// <summary>
    /// Scrolls the content downward by one page.
    /// </summary>
    public void PageDown()
    {
        VerticalScrollBarValue = Math.Min(_offset.Y + _viewport.Height, VerticalScrollBarMaximum);
    }

    /// <summary>
    /// Scrolls the content left by one page.
    /// </summary>
    public void PageLeft()
    {
        HorizontalScrollBarValue = Math.Max(_offset.X - _viewport.Width, 0);
    }

    /// <summary>
    /// Scrolls the content tight by one page.
    /// </summary>
    public void PageRight()
    {
        HorizontalScrollBarValue = Math.Min(_offset.X + _viewport.Width, HorizontalScrollBarMaximum);
    }

    /// <summary>
    /// Scrolls to the top-left corner of the content.
    /// </summary>
    public void ScrollToHome()
    {
        Offset = new Vector(double.NegativeInfinity, double.NegativeInfinity);
    }

    /// <summary>
    /// Scrolls to the bottom-left corner of the content.
    /// </summary>
    public void ScrollToEnd()
    {
        Offset = new Vector(double.NegativeInfinity, double.PositiveInfinity);
    }

    /// <summary>
    /// Gets the value of the HorizontalScrollBarVisibility attached property.
    /// </summary>
    /// <param name="control">The control to read the value from.</param>
    /// <returns>The value of the property.</returns>
    public static ScrollBarVisibility GetHorizontalScrollBarVisibility(Control control)
    {
        return control.GetValue(HorizontalScrollBarVisibilityProperty);
    }

    /// <summary>
    /// Gets the value of the HorizontalScrollBarVisibility attached property.
    /// </summary>
    /// <param name="control">The control to set the value on.</param>
    /// <param name="value">The value of the property.</param>
    public static void SetHorizontalScrollBarVisibility(Control control, ScrollBarVisibility value)
    {
        control.SetValue(HorizontalScrollBarVisibilityProperty, value);
    }

    /// <summary>
    /// Gets the value of the VerticalScrollBarVisibility attached property.
    /// </summary>
    /// <param name="control">The control to read the value from.</param>
    /// <returns>The value of the property.</returns>
    public static ScrollBarVisibility GetVerticalScrollBarVisibility(Control control)
    {
        return control.GetValue(VerticalScrollBarVisibilityProperty);
    }

    /// <summary>
    /// Gets the value of the VerticalScrollBarVisibility attached property.
    /// </summary>
    /// <param name="control">The control to set the value on.</param>
    /// <param name="value">The value of the property.</param>
    public static void SetVerticalScrollBarVisibility(Control control, ScrollBarVisibility value)
    {
        control.SetValue(VerticalScrollBarVisibilityProperty, value);
    }

    /// <summary>
    /// Registers the specified <see cref="ContentPresenter"/> and subscribes to its child property changes.
    /// </summary>
    /// <param name="presenter">The content presenter to register.</param>
    /// <returns><see langword="true"/> if the registration is successful; otherwise, <see langword="false"/>.</returns>
    protected override bool RegisterContentPresenter(ContentPresenter presenter)
    {
        _childSubscription?.Dispose();
        _childSubscription = null;

        if (base.RegisterContentPresenter(presenter))
        {
            ContentPresenter? contentPresenter = Presenter;
            _childSubscription = contentPresenter?
                .GetObservable(ContentPresenter.ChildProperty)
                .Subscribe(ChildChanged);
            return true;
        }

        return false;
    }

    internal static Vector CoerceOffset(Size extent, Size viewport, Vector offset)
    {
        double maxX = Math.Max(extent.Width - viewport.Width, 0);
        double maxY = Math.Max(extent.Height - viewport.Height, 0);
        return new Vector(Clamp(offset.X, 0, maxX), Clamp(offset.Y, 0, maxY));
    }

    private static double Clamp(double value, double min, double max)
    {
        return value < min ? min : value > max ? max : value;
    }

    private static double Max(double x, double y)
    {
        double result = Math.Max(x, y);
        return double.IsNaN(result) ? 0 : result;
    }

    private void ChildChanged(Control? child)
    {
        if (_logicalScrollable != null)
        {
            _logicalScrollable.ScrollInvalidated -= LogicalScrollInvalidated;
            _logicalScrollable = null;
        }

        if (child is ILogicalScrollable logical)
        {
            _logicalScrollable = logical;
            logical.ScrollInvalidated += LogicalScrollInvalidated;
        }

        CalculatedPropertiesChanged();
    }

    private void LogicalScrollInvalidated(object? sender, EventArgs e)
    {
        CalculatedPropertiesChanged();
    }

    private void ScrollBarVisibilityChanged(AvaloniaPropertyChangedEventArgs e)
    {
        bool wasEnabled = !ScrollBarVisibility.Disabled.Equals(e.OldValue);
        bool isEnabled = !ScrollBarVisibility.Disabled.Equals(e.NewValue);

        if (wasEnabled != isEnabled)
        {
            if (e.Property == HorizontalScrollBarVisibilityProperty)
                SetAndRaise(CanHorizontallyScrollProperty, ref wasEnabled, isEnabled);
            else if (e.Property == VerticalScrollBarVisibilityProperty)
                SetAndRaise(CanVerticallyScrollProperty, ref wasEnabled, isEnabled);
        }
    }

    private void CalculatedPropertiesChanged()
    {
        // Pass old values of 0 here because we don't have the old values at this point,
        // and it shouldn't matter as only the template uses these properies.
        RaisePropertyChanged(HorizontalScrollBarMaximumProperty, 0, HorizontalScrollBarMaximum);
        RaisePropertyChanged(HorizontalScrollBarValueProperty, 0, HorizontalScrollBarValue);
        RaisePropertyChanged(HorizontalScrollBarViewportSizeProperty, 0, HorizontalScrollBarViewportSize);
        RaisePropertyChanged(VerticalScrollBarMaximumProperty, 0, VerticalScrollBarMaximum);
        RaisePropertyChanged(VerticalScrollBarValueProperty, 0, VerticalScrollBarValue);
        RaisePropertyChanged(VerticalScrollBarViewportSizeProperty, 0, VerticalScrollBarViewportSize);
        RaisePropertyChanged(VisibleMaximumProperty, false, VisibleMaximum);
        RaisePropertyChanged(HorizontalScrollBarEnableIncreaseProperty, false, HorizontalScrollBarEnableIncrease);
        RaisePropertyChanged(HorizontalScrollBarEnableDecreaseProperty, false, HorizontalScrollBarEnableDecrease);

        if (_logicalScrollable?.IsLogicalScrollEnabled == true)
        {
            SetAndRaise(SmallChangeProperty, ref _smallChange, _logicalScrollable.ScrollSize);
            SetAndRaise(LargeChangeProperty, ref _largeChange, _logicalScrollable.PageScrollSize);
        }
        else
        {
            SetAndRaise(SmallChangeProperty, ref _smallChange, new Size(DefaultSmallChange, DefaultSmallChange));
            SetAndRaise(LargeChangeProperty, ref _largeChange, Viewport);
        }
    }

    /// <summary>
    /// Handles key press events for page navigation.
    /// </summary>
    /// <param name="e">The key event arguments containing information about the pressed key.</param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.PageUp)
        {
            PageUp();
            e.Handled = true;
        }
        else if (e.Key == Key.PageDown)
        {
            PageDown();
            e.Handled = true;
        }
    }

    /// <summary>
    /// Called when a change in scrolling state is detected, such as a change in scroll
    /// position, extent, or viewport size.
    /// </summary>
    /// <param name="e">The event args.</param>
    /// <remarks>
    /// If you override this method, call `base.OnScrollChanged(ScrollChangedEventArgs)` to
    /// ensure that this event is raised.
    /// </remarks>
    private void OnScrollChanged(ScrollChangedEventArgs e)
    {
        RaiseEvent(e);
    }

    /// <summary>
    /// Applies the control's template and initializes necessary elements.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        RepeatButton? scrollDecreaseButton = e.NameScope.Find<RepeatButton>("PART_ScrollDecreaseButton");
        RepeatButton? scrollIncreaseButton = e.NameScope.Find<RepeatButton>("PART_ScrollIncreaseButton");

        if (scrollDecreaseButton is not null && scrollIncreaseButton is not null)
        {
            scrollDecreaseButton.Click += ScrollDecreaseButtonOnClick;
            scrollIncreaseButton.Click += ScrollIncreaseButtonOnClick;
        }

        KeyDown += OnKeyDown;

        _scrollBarExpandSubscription?.Dispose();

        _scrollBarExpandSubscription = SubscribeToScrollBars(e);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Up)
            Offset += new Vector(190, 0);
        else if (e.Key == Key.Down)
            Offset -= new Vector(190, 0);
    }

    private void ScrollIncreaseButtonOnClick(object? sender, RoutedEventArgs e)
    {
        Offset += new Vector(190, 0);
    }

    private void ScrollDecreaseButtonOnClick(object? sender, RoutedEventArgs e)
    {
        Offset -= new Vector(190, 0);
    }

    private IDisposable? SubscribeToScrollBars(TemplateAppliedEventArgs e)
    {
        static IObservable<bool>? GetExpandedObservable(ScrollBar? scrollBar)
        {
            return scrollBar?.GetObservable(ScrollBar.IsExpandedProperty);
        }

        ScrollBar? horizontalScrollBar = e.NameScope.Find<ScrollBar>("PART_HorizontalScrollBar");
        ScrollBar? verticalScrollBar = e.NameScope.Find<ScrollBar>("PART_VerticalScrollBar");

        IObservable<bool>? horizontalExpanded = GetExpandedObservable(horizontalScrollBar);
        IObservable<bool>? verticalExpanded = GetExpandedObservable(verticalScrollBar);

        IObservable<bool>? actualExpanded = null;

        if (horizontalExpanded != null && verticalExpanded != null)
        {
            actualExpanded = horizontalExpanded.CombineLatest(verticalExpanded, (h, v) => h || v);
        }
        else
        {
            if (horizontalExpanded != null)
                actualExpanded = horizontalExpanded;
            else if (verticalExpanded != null) actualExpanded = verticalExpanded;
        }

        return actualExpanded?.Subscribe(OnScrollBarExpandedChanged);
    }

    private void OnScrollBarExpandedChanged(bool isExpanded)
    {
        IsExpanded = isExpanded;
    }

    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        RaiseScrollChanged();
    }

    private void RaiseScrollChanged()
    {
        Vector extentDelta = new(Extent.Width - _oldExtent.Width, Extent.Height - _oldExtent.Height);
        Vector offsetDelta = Offset - _oldOffset;
        Vector viewportDelta = new(Viewport.Width - _oldViewport.Width, Viewport.Height - _oldViewport.Height);

        if (!extentDelta.NearlyEquals(default) || !offsetDelta.NearlyEquals(default) ||
            !viewportDelta.NearlyEquals(default))
        {
            ScrollChangedEventArgs e = new(extentDelta, offsetDelta, viewportDelta);
            OnScrollChanged(e);

            _oldExtent = Extent;
            _oldOffset = Offset;
            _oldViewport = Viewport;
        }
    }
}