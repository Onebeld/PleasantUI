using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using PleasantUI.Extensions;
using PleasantUI.Reactive;

namespace PleasantUI.Controls;

public class NavigationViewItem : TreeViewItem
{
    private Geometry? _icon;
    private object _title = "Title";
    private int _navigationViewDistance;
    private double _externalLength;
    
    /// <inheritdoc cref="StyleKeyOverride"/>
    protected override Type StyleKeyOverride => typeof(NavigationViewItem);
    
    public static readonly DirectProperty<NavigationViewItem, object?> ContentProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItem, object?>(
            nameof(Content),
            o => o.Content,
            (o, v) => o.Content = v);

    public static readonly DirectProperty<NavigationViewItem, Geometry?> IconProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItem, Geometry?>(
            nameof(Icon),
            o => o.Icon,
            (o, v) => o.Icon = v);

    public static readonly DirectProperty<NavigationViewItem, object> TitleProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItem, object>(
            nameof(Title),
            o => o.Title,
            (o, v) => o.Title = v);
    
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<NavigationViewItem, bool>(nameof(IsOpen));

    public static readonly StyledProperty<bool> SelectOnCloseProperty =
        AvaloniaProperty.Register<NavigationViewItem, bool>(nameof(SelectOnClose));

    public static readonly StyledProperty<ClickMode> ClickModeProperty =
        Button.ClickModeProperty.AddOwner<NavigationViewItem>();

    public static readonly DirectProperty<NavigationViewItem, int> NavigationViewDistanceProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItem, int>(nameof(NavigationViewDistance), o => o.Level);

    public static readonly StyledProperty<double> CompactPaneLengthProperty =
        AvaloniaProperty.Register<NavigationViewItem, double>(nameof(CompactPaneLength));

    public static readonly StyledProperty<double> OpenPaneLengthProperty =
        AvaloniaProperty.Register<NavigationViewItem, double>(nameof(OpenPaneLength));

    public static readonly DirectProperty<NavigationViewItem, double> ExternalLengthProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItem, double>(nameof(ExternalLength),
            o => o.ExternalLength);
    
    public static readonly RoutedEvent<RoutedEventArgs> OpenedEvent =
        RoutedEvent.Register<NavigationViewItem, RoutedEventArgs>(nameof(Opened), RoutingStrategies.Tunnel);

    public static readonly RoutedEvent<RoutedEventArgs> ClosedEvent =
        RoutedEvent.Register<NavigationViewItem, RoutedEventArgs>(nameof(Closed), RoutingStrategies.Tunnel);
    
    /// <summary>
    /// Gets or sets the content of the property.
    /// </summary>
    /// <value>
    /// The content of the property.
    /// </value>
    public object? Content
    {
        get => _content;
        set => SetAndRaise(ContentProperty, ref _content, value);
    }

    /// <summary>
    /// Gets or sets the icon displayed for the property.
    /// </summary>
    /// <value>
    /// The icon geometry value for the property. If no icon is set, the value is null.
    /// </value>
    public Geometry? Icon
    {
        get => _icon;
        set => SetAndRaise(IconProperty, ref _icon, value);
    }

    /// <summary>
    /// Represents the title of an object.
    /// </summary>
    /// <value>
    /// The title of the object.
    /// </value>
    /// <remarks>
    /// This property allows getting and setting the title of the object.
    /// The title can be used to provide a descriptive name or label for the object.
    /// </remarks>
    public object Title
    {
        get => _title;
        set => SetAndRaise(TitleProperty, ref _title, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the property is open.
    /// </summary>
    /// <value>
    /// <c>true</c> if the property is open; otherwise, <c>false</c>.
    /// </value>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the control should automatically select the text upon closing.
    /// </summary>
    /// <value>
    /// <c>true</c> if the control should automatically select the text upon closing; otherwise, <c>false</c>.
    /// </value>
    public bool SelectOnClose
    {
        get => GetValue(SelectOnCloseProperty);
        set => SetValue(SelectOnCloseProperty, value);
    }

    /// <summary>
    /// Gets or sets the click mode for the control.
    /// </summary>
    /// <value>
    /// The click mode for the control.
    /// </value>
    public ClickMode ClickMode
    {
        get => GetValue(ClickModeProperty);
        set => SetValue(ClickModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the distance of the navigation view.
    /// </summary>
    /// <value>
    /// The distance of the navigation view.
    /// </value>
    public int NavigationViewDistance
    {
        get => _navigationViewDistance;
        protected set => SetAndRaise(LevelProperty, ref _navigationViewDistance, value);
    }

    /// <summary>
    /// Gets or sets the length of a compact pane.
    /// </summary>
    /// <value>
    /// The length of a compact pane.
    /// </value>
    public double CompactPaneLength
    {
        get => GetValue(CompactPaneLengthProperty);
        set => SetValue(CompactPaneLengthProperty, value);
    }

    /// <summary>
    /// Gets or sets the length of the open pane.
    /// </summary>
    /// <value>
    /// The length of the open pane.
    /// </value>
    /// <remarks>
    /// This property represents the length of the open pane in the user interface.
    /// The value is specified in a floating-point number, allowing fractional values.
    /// </remarks>
    public double OpenPaneLength
    {
        get => GetValue(OpenPaneLengthProperty);
        set => SetValue(OpenPaneLengthProperty, value);
    }

    /// <summary>
    /// Gets the external length property.
    /// </summary>
    /// <value>
    /// The external length.
    /// </value>
    public double ExternalLength
    {
        get => _externalLength;
        private set => SetAndRaise(ExternalLengthProperty, ref _externalLength, value);
    }

    /// <summary>
    /// Occurs when the Opened event is raised.
    /// </summary>
    public event EventHandler<RoutedEventArgs> Opened
    {
        add => AddHandler(OpenedEvent, value);
        remove => RemoveHandler(OpenedEvent, value);
    }

    /// <summary>
    /// Represents the event that is raised when a control is closed.
    /// </summary>
    /// <remarks>
    /// This event is raised when a control is closed. Subscribers can handle this event to perform certain actions when the control is closed.
    /// </remarks>
    public event EventHandler<RoutedEventArgs> Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }
    
    /// <summary>
    /// Needed if AOT does not support creating a class instance by type
    /// </summary>
    public Func<Control>? FuncControl { get; set; }

    private object? _content;

    static NavigationViewItem()
    {
        IsExpandedProperty.Changed.AddClassHandler<NavigationViewItem>(
            (navigationViewItem, _) =>
            {
                if (navigationViewItem.IsExpanded)
                {
                    RoutedEventArgs routedEventArgs = new(OpenedEvent);
                    navigationViewItem.RaiseEvent(routedEventArgs);
                }
                else
                {
                    RoutedEventArgs routedEventArgs = new(ClosedEvent);
                    navigationViewItem.RaiseEvent(routedEventArgs);
                }
            });
        OpenedEvent.AddClassHandler<NavigationViewItem>((x, e) => x.OnOpened(x, e));
        ClosedEvent.AddClassHandler<NavigationViewItem>((x, e) => x.OnClosed(x, e));
        IsSelectedProperty.Changed.AddClassHandler<NavigationViewItem>
        ((navigationViewItem, e) =>
        {
            if (navigationViewItem.IsSelected)
                navigationViewItem.OnSelected(navigationViewItem, e);
            else
                navigationViewItem.OnDeselected(navigationViewItem, e);
        });
        IsOpenProperty.Changed.Subscribe(OnIsOpenChanged);
        OpenPaneLengthProperty.Changed.Subscribe(OnPaneSizesChanged);
        CompactPaneLengthProperty.Changed.Subscribe(OnPaneSizesChanged);
        
        FocusableProperty.OverrideDefaultValue<NavigationViewItem>(true);
        ClickModeProperty.OverrideDefaultValue<NavigationViewItem>(ClickMode.Release);
    }
    
    public NavigationViewItem()
    {
        NavigationViewDistance = 0;
    }
    
    private static void OnPaneSizesChanged(AvaloniaPropertyChangedEventArgs<double> e)
    {
        if (e.Sender is NavigationViewItem navigationViewItem)
        {
            navigationViewItem.ExternalLength = navigationViewItem.OpenPaneLength - navigationViewItem.CompactPaneLength;
        }
    }

    private static void OnIsOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is NavigationViewItem sender)
        {
            if (sender.IsSelected && sender.Parent is NavigationViewItem { Parent: NavigationView navigationView, SelectOnClose: true } navigationViewItem)
            {
                navigationView.SelectSingleItem(navigationViewItem);
            }

            switch (sender.IsOpen)
            {
                case true:
                    sender.RaiseEvent(new RoutedEventArgs(OpenedEvent));
                    break;
                case false:
                    sender.IsExpanded = false;
                    sender.RaiseEvent(new RoutedEventArgs(ClosedEvent));
                    break;
            }
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePseudoClasses();
    }

    protected virtual void OnDeselected(object sender, AvaloniaPropertyChangedEventArgs e)
    {
    }

    protected virtual void OnSelected(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (Parent is NavigationView { DisplayMode: SplitViewDisplayMode.CompactOverlay or SplitViewDisplayMode.Overlay } navigationView)
            navigationView.IsOpen = false;
    }

    protected virtual void OnOpened(object sender, RoutedEventArgs e)
    {
        UpdatePseudoClasses();
    }

    protected virtual void OnClosed(object sender, RoutedEventArgs e)
    {
        IsExpanded = false;
        UpdatePseudoClasses();
        
        if (SelectOnClose)
            this.GetParentTOfLogical<NavigationView>()?.SelectSingleItem(this);
    }

    private void UpdatePseudoClasses()
    {
        if (IsOpen)
        {
            PseudoClasses.Remove(":closed");
            PseudoClasses.Add(":opened");
        }
        else
        {
            PseudoClasses.Remove(":opened");
            PseudoClasses.Add(":closed");
        }
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        NavigationViewDistance = Extensions.LogicalExtensions.CalculateDistanceFromLogicalParent<NavigationView>(this) - 1;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            e.Handled = true;

            if (ClickMode == ClickMode.Press)
                Select();
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            e.Handled = true;

            if (ClickMode == ClickMode.Release &&
                this.GetVisualsAt(e.GetPosition(this)).Any(c => this == c || this.IsVisualAncestorOf(c)))
            {
                Select();
            }
        }
    }

    private void Select()
    {
        if (!IsSelected)
            this.GetParentTOfLogical<NavigationView>()?.SelectSingleItem(this);
    }
}