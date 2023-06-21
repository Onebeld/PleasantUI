using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace PleasantUI.Controls;

public class NavigationViewItemBase : TreeViewItem
{
    private Geometry? _icon;
    private Type? _typeContent;
    private object _title = "Title";
    private int _navigationViewDistance;
    private double _externalLength;

    public static readonly DirectProperty<NavigationViewItemBase, object?> ContentProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItemBase, object?>(
            nameof(Content),
            o => o.Content,
            (o, v) => o.Content = v);

    public static readonly DirectProperty<NavigationViewItemBase, Geometry?> IconProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItemBase, Geometry?>(
            nameof(Icon),
            o => o.Icon,
            (o, v) => o.Icon = v);

    public static readonly DirectProperty<NavigationViewItemBase, object> TitleProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItemBase, object>(
            nameof(Title),
            o => o.Title,
            (o, v) => o.Title = v);

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<NavigationViewItemBase, bool>(nameof(IsOpen), true);

    public static readonly StyledProperty<bool> SelectOnCloseProperty =
        AvaloniaProperty.Register<NavigationViewItemBase, bool>(nameof(SelectOnClose));

    public static readonly StyledProperty<ClickMode> ClickModeProperty =
        Button.ClickModeProperty.AddOwner<NavigationViewItemBase>();

    public static readonly DirectProperty<NavigationViewItemBase, int> NavigationViewDistanceProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItemBase, int>(nameof(NavigationViewDistance), o => o.Level);

    public static readonly StyledProperty<double> CompactPaneLengthProperty =
        AvaloniaProperty.Register<NavigationViewItemBase, double>(nameof(CompactPaneLength));

    public static readonly StyledProperty<double> OpenPaneLengthProperty =
        AvaloniaProperty.Register<NavigationViewItemBase, double>(nameof(OpenPaneLength));

    public static readonly DirectProperty<NavigationViewItemBase, double> ExternalLengthProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItemBase, double>(nameof(ExternalLength),
            o => o.ExternalLength);

    public object? Content
    {
        get => _content;
        set => SetAndRaise(ContentProperty, ref _content, value);
    }

    public Geometry? Icon
    {
        get => _icon;
        set => SetAndRaise(IconProperty, ref _icon, value);
    }

    public object Title
    {
        get => _title;
        set => SetAndRaise(TitleProperty, ref _title, value);
    }

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public bool SelectOnClose
    {
        get => GetValue(SelectOnCloseProperty);
        set => SetValue(SelectOnCloseProperty, value);
    }

    public ClickMode ClickMode
    {
        get => GetValue(ClickModeProperty);
        set => SetValue(ClickModeProperty, value);
    }

    public int NavigationViewDistance
    {
        get => _navigationViewDistance;
        protected set => SetAndRaise(LevelProperty, ref _navigationViewDistance, value);
    }

    public double CompactPaneLength
    {
        get => GetValue(CompactPaneLengthProperty);
        set => SetValue(CompactPaneLengthProperty, value);
    }

    public double OpenPaneLength
    {
        get => GetValue(OpenPaneLengthProperty);
        set => SetValue(OpenPaneLengthProperty, value);
    }

    public double ExternalLength
    {
        get => _externalLength;
        private set => SetAndRaise(ExternalLengthProperty, ref _externalLength, value);
    }
    
    
    /// <summary>
    /// Needed if AOT does not support creating a class instance by type
    /// </summary>
    public Func<Control> FuncControl { get; set; }

    private object? _content;

    static NavigationViewItemBase()
    {
        IsExpandedProperty.Changed.AddClassHandler<NavigationViewItemBase>(
            (x, _) =>
            {
                switch (x.IsExpanded)
                {
                    case true:
                    {
                        RoutedEventArgs routedEventArgs = new(OpenedEvent);
                        x.RaiseEvent(routedEventArgs);
                        break;
                    }
                    case false:
                    {
                        RoutedEventArgs routedEventArgs = new(ClosedEvent);
                        x.RaiseEvent(routedEventArgs);
                        break;
                    }
                }
            });
        OpenedEvent.AddClassHandler<NavigationViewItemBase>((x, e) => x.OnOpened(x, e));
        ClosedEvent.AddClassHandler<NavigationViewItemBase>((x, e) => x.OnClosed(x, e));
        IsSelectedProperty.Changed.AddClassHandler<NavigationViewItemBase>
        ((x, e) =>
        {
            switch (x.IsSelected)
            {
                case true:
                    x.OnSelected(x, e);
                    break;
                case false:
                    x.OnDeselected(x, e);
                    break;
            }
        });
        IsOpenProperty.Changed.Subscribe(OnIsOpenChanged);
        OpenPaneLengthProperty.Changed.Subscribe(OnPaneSizesChanged);
        CompactPaneLengthProperty.Changed.Subscribe(OnPaneSizesChanged);
    }

    private static void OnPaneSizesChanged(AvaloniaPropertyChangedEventArgs<double> e)
    {
        if (e.Sender is NavigationViewItemBase n)
        {
            n.ExternalLength = n.OpenPaneLength - n.CompactPaneLength;
        }
    }

    private static void OnIsOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is NavigationViewItem sender)
        {
            if (sender.IsSelected && sender.Parent is NavigationViewItem { Parent: NavigationView nwp, SelectOnClose: true } nw)
            {
                nwp.SelectSingleItem(nw);
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

    public NavigationViewItemBase()
    {
        NavigationViewDistance = 0;
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

    public event EventHandler<RoutedEventArgs> Opened
    {
        add => AddHandler(OpenedEvent, value);
        remove => RemoveHandler(OpenedEvent, value);
    }

    public static readonly RoutedEvent<RoutedEventArgs> OpenedEvent =
        RoutedEvent.Register<NavigationViewItemBase, RoutedEventArgs>(nameof(Opened), RoutingStrategies.Tunnel);

    public event EventHandler<RoutedEventArgs> Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    public static readonly RoutedEvent<RoutedEventArgs> ClosedEvent =
        RoutedEvent.Register<NavigationViewItemBase, RoutedEventArgs>(nameof(Closed), RoutingStrategies.Tunnel);
}