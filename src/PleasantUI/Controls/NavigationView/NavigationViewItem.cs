using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Reactive;
using Avalonia.VisualTree;
using PleasantUI.Core.Extensions;
using LogicalExtensions = PleasantUI.Core.Extensions.LogicalExtensions;

namespace PleasantUI.Controls;

/// <summary>
/// Represents an item within a <see cref="NavigationView" />.
/// </summary>
[TemplatePart("PART_Popup", typeof(Popup))]
public class NavigationViewItem : TreeViewItem
{
    private object? _content;
    private double _externalLength;
    private Geometry? _icon;

    private int _navigationViewDistance;
    private object _title = "Title";
    private bool _isSubMenuOpen;

    private Popup? _popup;
    private SmoothScrollViewer? _popupScrollViewer;
    private NavigationViewSubMenuControl? _subMenuControl;
    
    /// <summary>
    /// Defines the <see cref="Content" /> property.
    /// </summary>
    public static readonly DirectProperty<NavigationViewItem, object?> ContentProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItem, object?>(
            nameof(Content),
            o => o.Content,
            (o, v) => o.Content = v);

    /// <summary>
    /// Defines the <see cref="Icon" /> property.
    /// </summary>
    public static readonly DirectProperty<NavigationViewItem, Geometry?> IconProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItem, Geometry?>(
            nameof(Icon),
            o => o.Icon,
            (o, v) => o.Icon = v);

    /// <summary>
    /// Defines the <see cref="Title" /> property.
    /// </summary>
    public static readonly DirectProperty<NavigationViewItem, object> TitleProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItem, object>(
            nameof(Title),
            o => o.Title,
            (o, v) => o.Title = v);

    /// <summary>
    /// Defines the <see cref="IsOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<NavigationViewItem, bool>(nameof(IsOpen));

    /// <summary>
    /// Defines the <see cref="SelectOnClose" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> SelectOnCloseProperty =
        AvaloniaProperty.Register<NavigationViewItem, bool>(nameof(SelectOnClose));

    /// <summary>
    /// Defines the <see cref="ClickMode" /> property.
    /// </summary>
    public static readonly StyledProperty<ClickMode> ClickModeProperty =
        Button.ClickModeProperty.AddOwner<NavigationViewItem>();

    /// <summary>
    /// Defines the <see cref="NavigationViewDistance" /> property.
    /// </summary>
    public static readonly DirectProperty<NavigationViewItem, int> NavigationViewDistanceProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItem, int>(
            nameof(NavigationViewDistance),
            o => o.NavigationViewDistance,
            (o, v) => o.NavigationViewDistance = v);

    /// <summary>
    /// Defines the <see cref="CompactPaneLength" /> property.
    /// </summary>
    public static readonly StyledProperty<double> CompactPaneLengthProperty =
        AvaloniaProperty.Register<NavigationViewItem, double>(nameof(CompactPaneLength));

    /// <summary>
    /// Defines the <see cref="OpenPaneLength" /> property.
    /// </summary>
    public static readonly StyledProperty<double> OpenPaneLengthProperty =
        AvaloniaProperty.Register<NavigationViewItem, double>(nameof(OpenPaneLength));

    /// <summary>
    /// Defines the <see cref="ExternalLength" /> property.
    /// </summary>
    public static readonly DirectProperty<NavigationViewItem, double> ExternalLengthProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItem, double>(nameof(ExternalLength),
            o => o.ExternalLength);

    /// <summary>
    /// Defines the <see cref="IsSubMenuOpen" /> property.
    /// Controls whether the flyout popup submenu is open (used in compact/collapsed pane mode).
    /// </summary>
    public static readonly DirectProperty<NavigationViewItem, bool> IsSubMenuOpenProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItem, bool>(
            nameof(IsSubMenuOpen),
            o => o.IsSubMenuOpen,
            (o, v) => o.IsSubMenuOpen = v);

    /// <summary>
    /// Defines the <see cref="NavigationView" /> property.
    /// Reference to the parent NavigationView for popup submenu items that aren't in the logical tree.
    /// </summary>
    public static readonly StyledProperty<NavigationView?> NavigationViewProperty =
        AvaloniaProperty.Register<NavigationViewItem, NavigationView?>(nameof(NavigationView));

    /// <summary>
    /// Defines the routed event for when the <see cref="NavigationViewItem" /> is opened.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> OpenedEvent =
        RoutedEvent.Register<NavigationViewItem, RoutedEventArgs>(nameof(Opened), RoutingStrategies.Tunnel);

    /// <summary>
    /// Defines the routed event for when the <see cref="NavigationViewItem" /> is closed.
    /// </summary>
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
    /// Gets or sets a value indicating whether the flyout submenu popup is open.
    /// This is used when the navigation pane is in compact (collapsed) mode and the item has children.
    /// </summary>
    public bool IsSubMenuOpen
    {
        get => _isSubMenuOpen;
        set => SetAndRaise(IsSubMenuOpenProperty, ref _isSubMenuOpen, value);
    }

    /// <summary>
    /// Gets or sets the parent NavigationView.
    /// Used by popup submenu items to navigate when not in the logical tree.
    /// </summary>
    public NavigationView? NavigationView
    {
        get => GetValue(NavigationViewProperty);
        set => SetValue(NavigationViewProperty, value);
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
    /// This event is raised when a control is closed. Subscribers can handle this event to perform certain actions when
    /// the control is closed.
    /// </remarks>
    public event EventHandler<RoutedEventArgs> Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }
    
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
        IsSubMenuOpenProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<bool>>(OnIsSubMenuOpenChanged));
        IsOpenProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<bool>>(OnIsOpenChanged));
        OpenPaneLengthProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<double>>(OnPaneSizesChanged));
        CompactPaneLengthProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<double>>(OnPaneSizesChanged));

        FocusableProperty.OverrideDefaultValue<NavigationViewItem>(true);
        ClickModeProperty.OverrideDefaultValue<NavigationViewItem>(ClickMode.Release);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationViewItem" /> class.
    /// </summary>
    public NavigationViewItem()
    {
        NavigationViewDistance = 0;
    }

    /// <summary>
    /// Called when the item is deselected.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    protected virtual void OnDeselected(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        Debug.WriteLine($"[NavItem] OnDeselected header={Header} tag={Tag}");
    }

    /// <summary>
    /// Called when the item is selected.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    protected virtual void OnSelected(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        Debug.WriteLine($"[NavItem] OnSelected header={Header} tag={Tag} parentDisplayMode={(Parent as NavigationView)?.DisplayMode}");
        if (Parent is NavigationView
            {
                DisplayMode: SplitViewDisplayMode.CompactOverlay or SplitViewDisplayMode.Overlay
            } navigationView)
        {
            Debug.WriteLine($"[NavItem] OnSelected closing pane (CompactOverlay/Overlay) header={Header}");
            navigationView.IsOpen = false;
        }
    }

    /// <summary>
    /// Called when the item is opened (expanded).
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    protected virtual void OnOpened(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine($"[NavItem] OnOpened header={Header} tag={Tag} IsExpanded={IsExpanded}");
        UpdatePseudoClasses();
    }

    /// <summary>
    /// Called when the item is closed (collapsed).
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    protected virtual void OnClosed(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine($"[NavItem] OnClosed header={Header} tag={Tag} IsExpanded={IsExpanded} SelectOnClose={SelectOnClose}");
        IsExpanded = false;
        UpdatePseudoClasses();

        if (SelectOnClose)
        {
            Debug.WriteLine($"[NavItem] OnClosed SelectOnClose=true, selecting parent NavigationView item header={Header}");
            this.GetParentTOfLogical<NavigationView>()?.SelectSingleItem(this);
        }
    }
    
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(NavigationViewItem);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        Debug.WriteLine($"[NavItem] OnApplyTemplate header={Header} tag={Tag}");
        base.OnApplyTemplate(e);

        if (_popup is not null)
        {
            _popup.Closed -= OnPopupClosed;
        }

        _popup = e.NameScope.Find<Popup>("PART_Popup");
        _popupScrollViewer = e.NameScope.Find<SmoothScrollViewer>("PART_PopupScrollViewer");
        _subMenuControl = e.NameScope.Find<NavigationViewSubMenuControl>("PART_SubMenuControl");

        Debug.WriteLine($"[NavItem] OnApplyTemplate parts found: popup={_popup is not null} scrollViewer={_popupScrollViewer is not null} subMenuControl={_subMenuControl is not null}");

        if (_subMenuControl is not null)
        {
            _subMenuControl.NavigationViewItem = this;
        }

        if (_popup is not null)
        {
            _popup.Closed += OnPopupClosed;
        }

        UpdatePseudoClasses();
    }

    /// <inheritdoc />
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        NavigationViewDistance = LogicalExtensions.CalculateDistanceFromLogicalParent<NavigationView>(this) - 1;
        Debug.WriteLine($"[NavItem] OnAttachedToLogicalTree header={Header} tag={Tag} distance={NavigationViewDistance} parent={Parent?.GetType().Name}");
    }

    /// <inheritdoc />
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            Debug.WriteLine($"[NavItem] OnPointerPressed header={Header} tag={Tag} clickMode={ClickMode}");
            e.Handled = true;

            if (ClickMode == ClickMode.Press)
                Select();
        }
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (e.InitialPressMouseButton != MouseButton.Left) return;

        e.Handled = true;

        if (ClickMode == ClickMode.Release &&
            this.GetVisualsAt(e.GetPosition(this)).Any(c => this == c || this.IsVisualAncestorOf(c)))
        {
            Debug.WriteLine($"[NavItem] OnPointerReleased triggering Select header={Header} tag={Tag}");
            Select();
        }
    }

    private static void OnPaneSizesChanged(AvaloniaPropertyChangedEventArgs<double> e)
    {
        if (e.Sender is NavigationViewItem navigationViewItem)
        {
            var prev = navigationViewItem.ExternalLength;
            navigationViewItem.ExternalLength =
                navigationViewItem.OpenPaneLength - navigationViewItem.CompactPaneLength;
            Debug.WriteLine($"[NavItem] OnPaneSizesChanged header={navigationViewItem.Header} openPane={navigationViewItem.OpenPaneLength} compactPane={navigationViewItem.CompactPaneLength} externalLength={prev}→{navigationViewItem.ExternalLength}");
        }
    }

    private static void OnIsOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is not NavigationViewItem sender) return;

        Debug.WriteLine($"[NavItem] OnIsOpenChanged header={sender.Header} tag={sender.Tag} IsOpen={sender.IsOpen} IsExpanded={sender.IsExpanded}");

        if (sender is
            {
                IsSelected: true,
                Parent: NavigationViewItem
                {
                    Parent: NavigationView navigationView, SelectOnClose: true
                } navigationViewItem
            })
        {
            Debug.WriteLine($"[NavItem] OnIsOpenChanged selected child → SelectSingleItem on parent header={navigationViewItem.Header}");
            navigationView.SelectSingleItem(navigationViewItem);
        }

        switch (sender.IsOpen)
        {
            case true:
                Debug.WriteLine($"[NavItem] OnIsOpenChanged IsOpen=true → closing popup header={sender.Header}");
                sender.RaiseEvent(new RoutedEventArgs(OpenedEvent));
                sender.IsSubMenuOpen = false;
                break;
            case false:
                Debug.WriteLine($"[NavItem] OnIsOpenChanged IsOpen=false → collapsing header={sender.Header}");
                sender.RaiseEvent(new RoutedEventArgs(ClosedEvent));
                break;
        }
    }

    private static void OnIsSubMenuOpenChanged(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (e.Sender is not NavigationViewItem item) return;

        Debug.WriteLine($"[NavItem] OnIsSubMenuOpenChanged header={item.Header} tag={item.Tag} IsSubMenuOpen={e.NewValue.GetValueOrDefault()}");

        if (e.NewValue.GetValueOrDefault())
        {
            Debug.WriteLine($"[NavItem] OnIsSubMenuOpenChanged opening popup → AddPopupItemsPresenter header={item.Header}");
            item.AddPopupItemsPresenter();
        }
        else
        {
            Debug.WriteLine($"[NavItem] OnIsSubMenuOpenChanged closing popup → RemovePopupItemsPresenter header={item.Header}");
            item.RemovePopupItemsPresenter();
        }
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        Debug.WriteLine($"[NavItem] OnPopupClosed header={Header} tag={Tag} → setting IsSubMenuOpen=false");
        IsSubMenuOpen = false;
    }

    private void AddPopupItemsPresenter()
    {
        Debug.WriteLine($"[NavItem] AddPopupItemsPresenter header={Header} subMenuControl={_subMenuControl is not null} scrollViewer={_popupScrollViewer is not null} childCount={LogicalChildren.Count}");
        if (_subMenuControl is null || _popupScrollViewer is null) return;

        _subMenuControl.ItemsSource = LogicalChildren;

        if (!ReferenceEquals(_popupScrollViewer.Content, _subMenuControl))
            _popupScrollViewer.Content = _subMenuControl;

        Debug.WriteLine($"[NavItem] AddPopupItemsPresenter done — itemsSource set childCount={LogicalChildren.Count}");
    }
    

    private void RemovePopupItemsPresenter()
    {
        Debug.WriteLine($"[NavItem] RemovePopupItemsPresenter header={Header} subMenuControl={_subMenuControl is not null}");
        if (_subMenuControl is null) return;

        if (_popupScrollViewer?.Content == _subMenuControl)
            _popupScrollViewer.Content = null;

        _subMenuControl.ItemsSource = null;
        Debug.WriteLine($"[NavItem] RemovePopupItemsPresenter done header={Header}");
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
    Debug.WriteLine($"[NavItem] UpdatePseudoClasses header={Header} IsOpen={IsOpen} pseudoClasses=[{string.Join(",", PseudoClasses)}]");
}

private void Select()
{
    var navigationView = this.GetParentTOfLogical<NavigationView>() ?? NavigationView;
    var isPaneOpen = navigationView?.IsOpen ?? IsOpen;
    var hasChildren = this.LogicalChildren.OfType<NavigationViewItem>().Any();
    bool isPopupClone = navigationView is not null && this.GetParentTOfLogical<NavigationView>() is null;

    Debug.WriteLine($"[NavItem] Select header={Header} tag={Tag} isPaneOpen={isPaneOpen} hasChildren={hasChildren} isPopupClone={isPopupClone} IsSelected={IsSelected}");

    if (!isPaneOpen && hasChildren)
    {
        Debug.WriteLine($"[NavItem] Select compact+hasChildren → toggling IsSubMenuOpen={!IsSubMenuOpen} header={Header}");
        IsSubMenuOpen = !IsSubMenuOpen;
        return;
    }

    if (isPopupClone && navigationView is not null && Tag is not null)
    {
        var original = navigationView.GetLogicalDescendants()
            .OfType<NavigationViewItem>()
            .FirstOrDefault(x => Equals(x.Tag, Tag));
        Debug.WriteLine($"[NavItem] Select popup clone → found original={original?.Header} tag={Tag}");
        if (original is not null)
        {
            navigationView.SelectSingleItem(original);
            return;
        }
    }

    if (!IsSelected)
    {
        Debug.WriteLine($"[NavItem] Select → calling SelectSingleItem on NavigationView header={Header} tag={Tag}");
        navigationView?.SelectSingleItem(this);
    }
    else
    {
        Debug.WriteLine($"[NavItem] Select → already selected, skipping header={Header}");
    }
}
}