using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Reactive;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PleasantUI.Controls.Chrome;

namespace PleasantUI.Controls;

/// <summary>
/// Specifies the position of the navigation pane.
/// </summary>
public enum NavigationViewPosition
{
    /// <summary>Side pane (default left-side SplitView layout).</summary>
    Left,
    /// <summary>Horizontal bar above the content area.</summary>
    Top,
    /// <summary>Horizontal bar below the content area.</summary>
    Bottom
}

/// <summary>
/// Represents a navigation view control that displays a tree of items.
/// </summary>
/// <remarks>
/// The <c>NavigationView</c> control inherits from the <see cref="TreeView" /> control and adds additional
/// properties for customizing the appearance and behavior of the navigation view.
/// </remarks>
[PseudoClasses(":normal", ":compact", ":left", ":top", ":bottom")]
[TemplatePart("PART_HeaderItem", typeof(Button))]
[TemplatePart("PART_BackButton", typeof(Button))]
[TemplatePart("PART_SelectedContentPresenter", typeof(ContentPresenter))]
public class NavigationView : TreeView
{
    private const double LittleWidth = 1005;
    private const double VeryLittleWidth = 650;
    private double titleBarHeight;

    private Border? _container;
    private Grid? _mainGrid;
    private DockPanel? _dockPanel;
    private Border? _marginPanel;
    private StackPanel? _stackPanelButtons;
    private DockPanel? _topBottomLayout;

    private Button? _backButton;
    private ICommand? _backButtonCommand;

    private PleasantWindow? _window;

    private CancellationTokenSource? _cancellationTokenSource;
    private ContentPresenter? _contentPresenter;
    private ContentPresenter? _topBottomContentPresenter;

    // Stable wrapper controls permanently assigned to each presenter.
    // The actual page content is placed inside the active wrapper; the inactive wrapper is hidden.
    // This avoids all single-visual-parent conflicts because the wrappers never move between
    // presenters — only the content inside them changes, and Border.Child assignment is safe
    // because we always clear the old wrapper's child before setting the new one.
    private readonly Border _leftWrapper    = new() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch };
    private readonly Border _topBottomWrapper = new() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch };

    // Cache: the last content object and which position it was routed for.
    private object? _cachedContent;
    private NavigationViewPosition _cachedContentPosition;

    private Button? _headerItem;

    private IEnumerable<string>? _itemsAsStrings;

    private object? _selectedContent;

    // Separate item collections for top/bottom nav bar — never shared with the left-pane Items.
    private readonly Avalonia.Collections.AvaloniaList<NavigationViewItem> _topItems = new();
    private readonly Avalonia.Collections.AvaloniaList<NavigationViewItem> _bottomItems = new();

    // Stores the IsExpanded state of group items before the pane collapses to compact mode,
    // keyed by the NavigationViewItem instance so each item's state is tracked independently.
    private readonly Dictionary<NavigationViewItem, bool> _expandedStates = new();

    /// <summary>
    /// Defines the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<Geometry> IconProperty =
        AvaloniaProperty.Register<NavigationView, Geometry>(nameof(Icon));

    /// <summary>
    /// Defines the <see cref="SelectedContent" /> property.
    /// </summary>
    public static readonly DirectProperty<NavigationView, object?> SelectedContentProperty =
        AvaloniaProperty.RegisterDirect<NavigationView, object?>(nameof(SelectedContent), o => o.SelectedContent);

    /// <summary>
    /// Defines the <see cref="SelectedContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> SelectedContentTemplateProperty =
        AvaloniaProperty.Register<NavigationView, IDataTemplate>(nameof(SelectedContentTemplate));

    /// <summary>
    /// Defines the <see cref="CompactPaneLength" /> property.
    /// </summary>
    public static readonly StyledProperty<double> CompactPaneLengthProperty =
        AvaloniaProperty.Register<NavigationView, double>(nameof(CompactPaneLength));

    /// <summary>
    /// Defines the <see cref="OpenPaneLength" /> property.
    /// </summary>
    public static readonly StyledProperty<double> OpenPaneLengthProperty =
        AvaloniaProperty.Register<NavigationView, double>(nameof(OpenPaneLength));

    /// <summary>
    /// Defines the <see cref="IsOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<NavigationView, bool>(nameof(IsOpen));

    /// <summary>
    /// Defines the <see cref="DynamicDisplayMode" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> DynamicDisplayModeProperty =
        AvaloniaProperty.Register<NavigationView, bool>(nameof(DynamicDisplayMode), true);

    /// <summary>
    /// Defines the <see cref="BindWindowSettings" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> BindWindowSettingsProperty =
        AvaloniaProperty.Register<NavigationView, bool>(nameof(BindWindowSettings), true);

    /// <summary>
    /// Defines the <see cref="TransitionAnimation" /> property.
    /// </summary>
    public static readonly StyledProperty<Animation?> TransitionAnimationProperty =
        AvaloniaProperty.Register<NavigationView, Animation?>(nameof(TransitionAnimation));

    /// <summary>
    /// Defines the <see cref="DisplayMode" /> property.
    /// </summary>
    public static readonly StyledProperty<SplitViewDisplayMode> DisplayModeProperty =
        AvaloniaProperty.Register<NavigationView, SplitViewDisplayMode>(nameof(DisplayMode),
            SplitViewDisplayMode.CompactInline);

    /// <summary>
    /// Defines the <see cref="AlwaysOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> AlwaysOpenProperty =
        AvaloniaProperty.Register<NavigationView, bool>(nameof(AlwaysOpen));

    /// <summary>
    /// Defines the <see cref="DisplayTopIndent" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> DisplayTopIndentProperty =
        AvaloniaProperty.Register<NavigationView, bool>(nameof(DisplayTopIndent), true);

    /// <summary>
    /// Defines the <see cref="NotMakeOffsetForContentPanel" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> NotMakeOffsetForContentPanelProperty =
        AvaloniaProperty.Register<NavigationView, bool>(nameof(NotMakeOffsetForContentPanel));

    /// <summary>
    /// Defines the <see cref="ItemsAsStrings" /> property.
    /// </summary>
    public static readonly DirectProperty<NavigationView, IEnumerable<string>?> ItemsAsStringsProperty =
        AvaloniaProperty.RegisterDirect<NavigationView, IEnumerable<string>?>(nameof(ItemsAsStrings),
            o => o.ItemsAsStrings);

    /// <summary>
    /// Defines the <see cref="IsFloatingHeader" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsFloatingHeaderProperty =
        AvaloniaProperty.Register<NavigationView, bool>(nameof(IsFloatingHeader));

    /// <summary>
    /// Defines the <see cref="BackButtonCommand" /> property.
    /// </summary>
    public static readonly DirectProperty<NavigationView, ICommand?> BackButtonCommandProperty =
        AvaloniaProperty.RegisterDirect<NavigationView, ICommand?>(nameof(BackButtonCommand),
            navigationView => navigationView.BackButtonCommand,
            (navigationView, command) => navigationView.BackButtonCommand = command, enableDataValidation: true);

    /// <summary>
    /// Defines the <see cref="ShowBackButton" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowBackButtonProperty =
        AvaloniaProperty.Register<NavigationView, bool>(nameof(ShowBackButton));

    /// <summary>
    /// Defines the <see cref="ButtonsPanelOffset" /> property.
    /// When true, the hamburger/back buttons panel is pushed down by the window titlebar height
    /// so it sits flush below the titlebar rather than overlapping it.
    /// </summary>
    public static readonly StyledProperty<bool> ButtonsPanelOffsetProperty =
        AvaloniaProperty.Register<NavigationView, bool>(nameof(ButtonsPanelOffset), false);

    /// <summary>
    /// Defines the <see cref="Position" /> property.
    /// </summary>
    public static readonly StyledProperty<NavigationViewPosition> PositionProperty =
        AvaloniaProperty.Register<NavigationView, NavigationViewPosition>(nameof(Position), NavigationViewPosition.Left);

    /// <summary>
    /// Defines the <see cref="TopItems" /> property.
    /// Items displayed in the top navigation bar (Position=Top). Completely separate from <see cref="ItemsControl.Items"/>.
    /// </summary>
    public static readonly DirectProperty<NavigationView, Avalonia.Collections.AvaloniaList<NavigationViewItem>> TopItemsProperty =
        AvaloniaProperty.RegisterDirect<NavigationView, Avalonia.Collections.AvaloniaList<NavigationViewItem>>(
            nameof(TopItems), o => o.TopItems);

    /// <summary>
    /// Defines the <see cref="BottomItems" /> property.
    /// Items displayed in the bottom navigation bar (Position=Bottom). Completely separate from <see cref="ItemsControl.Items"/>.
    /// </summary>
    public static readonly DirectProperty<NavigationView, Avalonia.Collections.AvaloniaList<NavigationViewItem>> BottomItemsProperty =
        AvaloniaProperty.RegisterDirect<NavigationView, Avalonia.Collections.AvaloniaList<NavigationViewItem>>(
            nameof(BottomItems), o => o.BottomItems);

    /// <summary>
    /// Gets or sets the geometry of the icon.
    /// </summary>
    /// <value>
    /// The geometry of the icon.
    /// </value>
    public Geometry Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets the selected content.
    /// </summary>
    /// <remarks>
    /// The selected content property represents the currently selected content item.
    /// It can be any object or null.
    /// </remarks>
    public object? SelectedContent
    {
        get => _selectedContent;
        private set => SetAndRaise(SelectedContentProperty, ref _selectedContent, value);
    }

    /// <summary>
    /// Gets or sets the data template used for the selected content of the property.
    /// </summary>
    /// <remarks>
    /// The data template defines the appearance and layout of the selected content.
    /// </remarks>
    /// <value>
    /// The data template used for the selected content.
    /// </value>
    public IDataTemplate SelectedContentTemplate
    {
        get => GetValue(SelectedContentTemplateProperty);
        set => SetValue(SelectedContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the length of the compact pane.
    /// </summary>
    /// <value>
    /// The length of the compact pane.
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
    public double OpenPaneLength
    {
        get => GetValue(OpenPaneLengthProperty);
        set => SetValue(OpenPaneLengthProperty, value);
    }

    /// <summary>
    /// Gets or sets the open state of the object.
    /// </summary>
    /// <value>
    /// <c>true</c> if the object is open; otherwise, <c>false</c>.
    /// </value>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the property AlwaysOpen is enabled.
    /// </summary>
    /// <value>
    /// True if the property AlwaysOpen is enabled; otherwise, false.
    /// </value>
    public bool AlwaysOpen
    {
        get => GetValue(AlwaysOpenProperty);
        set => SetValue(AlwaysOpenProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the top indent is displayed.
    /// </summary>
    /// <value>
    /// <c>true</c> if the top indent is displayed; otherwise, <c>false</c>.
    /// </value>
    public bool DisplayTopIndent
    {
        get => GetValue(DisplayTopIndentProperty);
        set => SetValue(DisplayTopIndentProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the content panel should have an offset or not.
    /// </summary>
    /// <value>
    /// <c>true</c> if the content panel should not have an offset; otherwise, <c>false</c>.
    /// </value>
    public bool NotMakeOffsetForContentPanel
    {
        get => GetValue(NotMakeOffsetForContentPanelProperty);
        set => SetValue(NotMakeOffsetForContentPanelProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the back button should be shown.
    /// </summary>
    /// <value>
    /// <c>true</c> if back button should be shown; otherwise, <c>false</c>.
    /// </value>
    public bool ShowBackButton
    {
        get => GetValue(ShowBackButtonProperty);
        set => SetValue(ShowBackButtonProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the hamburger/back buttons panel is pushed down by the titlebar height.
    /// When true (default), buttons sit flush below the titlebar. When false, original behavior.
    /// </summary>
    public bool ButtonsPanelOffset
    {
        get => GetValue(ButtonsPanelOffsetProperty);
        set => SetValue(ButtonsPanelOffsetProperty, value);
    }

    /// <summary>
    /// Gets or sets the position of the navigation pane (Left, Top, or Bottom).
    /// </summary>
    public NavigationViewPosition Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    /// <summary>
    /// Gets the items collection for the top navigation bar.
    /// Populate this instead of <see cref="ItemsControl.Items"/> when using <see cref="NavigationViewPosition.Top"/>.
    /// </summary>
    public Avalonia.Collections.AvaloniaList<NavigationViewItem> TopItems => _topItems;

    /// <summary>
    /// Gets the items collection for the bottom navigation bar.
    /// Populate this instead of <see cref="ItemsControl.Items"/> when using <see cref="NavigationViewPosition.Bottom"/>.
    /// </summary>
    public Avalonia.Collections.AvaloniaList<NavigationViewItem> BottomItems => _bottomItems;

    /// <summary>
    /// Gets or sets the display mode of the SplitView control.
    /// </summary>
    /// <remarks>
    /// The display mode controls how the content and pane of the SplitView control are displayed.
    /// </remarks>
    public SplitViewDisplayMode DisplayMode
    {
        get => GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the transition animations for the property.
    /// </summary>
    /// <value>
    /// The transition animations for the property.
    /// </value>
    public Animation? TransitionAnimation
    {
        get => GetValue(TransitionAnimationProperty);
        set => SetValue(TransitionAnimationProperty, value);
    }

    /// <summary>
    /// Gets or sets the collection of items as strings.
    /// </summary>
    /// <remarks>
    /// The collection of items as strings is an IEnumerable of strings.
    /// It can only be modified internally through the private setter.
    /// </remarks>
    /// <value>
    /// The collection of items as strings.
    /// </value>
    public IEnumerable<string>? ItemsAsStrings
    {
        get => _itemsAsStrings;
        private set => SetAndRaise(ItemsAsStringsProperty, ref _itemsAsStrings, value);
    }

    /// <summary>
    /// Gets or sets the value indicating whether the dynamic display mode is enabled.
    /// </summary>
    /// <value>
    /// <c>true</c> if the dynamic display mode is enabled; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// When the dynamic display mode is enabled, the display behavior will dynamically adjust based on certain conditions
    /// or events.
    /// </remarks>
    public bool DynamicDisplayMode
    {
        get => GetValue(DynamicDisplayModeProperty);
        set => SetValue(DynamicDisplayModeProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the header is floating.
    /// </summary>
    /// <value>
    /// <c>true</c> if the header is floating; otherwise, <c>false</c>.
    /// </value>
    public bool IsFloatingHeader
    {
        get => GetValue(IsFloatingHeaderProperty);
        set => SetValue(IsFloatingHeaderProperty, value);
    }

    /// <summary>
    /// Gets or sets the value indicating whether the window settings should be bound.
    /// </summary>
    /// <remarks>
    /// The BindWindowSettings property determines if the window settings should be bound. When set to true, the window
    /// settings will be updated when the property changes. When set to false,
    /// the window settings will remain unchanged.
    /// </remarks>
    /// <value>
    /// <c>true</c> if the window settings should be bound; otherwise, <c>false</c>.
    /// </value>
    public bool BindWindowSettings
    {
        get => GetValue(BindWindowSettingsProperty);
        set => SetValue(BindWindowSettingsProperty, value);
    }

    /// <summary>
    /// Gets or sets an <see cref="ICommand" /> to be invoked when the button is clicked.
    /// </summary>
    public ICommand? BackButtonCommand
    {
        get => _backButtonCommand;
        set => SetAndRaise(BackButtonCommandProperty, ref _backButtonCommand, value);
    }

    static NavigationView()
    {
        SelectionModeProperty.OverrideDefaultValue<NavigationView>(SelectionMode.Single);
        SelectedItemProperty.Changed.AddClassHandler<NavigationView>((x, _) => x.OnSelectedItemChanged());
        IsOpenProperty.Changed.AddClassHandler<NavigationView>((x, e) => x.OnIsOpenChanged(e));
        ShowBackButtonProperty.Changed.AddClassHandler<NavigationView>((x, _) => x.UpdateMarginPanel());
        DisplayModeProperty.Changed.AddClassHandler<NavigationView>((x, _) => x.UpdateMarginPanel());
        ButtonsPanelOffsetProperty.Changed.AddClassHandler<NavigationView>((x, _) => x.UpdateMarginPanel());
        PositionProperty.Changed.AddClassHandler<NavigationView>((x, e) => x.OnPositionChanged(e));
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationView"/> class.
    /// </summary>
    public NavigationView()
    {
        PseudoClasses.Add(":normal");
        PseudoClasses.Add(":left");
        this.GetObservable(BoundsProperty).Subscribe(new AnonymousObserver<Rect>(bounds =>
        {
            Dispatcher.UIThread.InvokeAsync(() => OnBoundsChanged(bounds));
        }));
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        Debug.WriteLine("[NavigationView] OnApplyTemplate");
        base.OnApplyTemplate(e);

        _stackPanelButtons = e.NameScope.Find<StackPanel>("PART_StackPanelButtons");
        _headerItem = e.NameScope.Find<Button>("PART_HeaderItem");
        _backButton = e.NameScope.Find<Button>("PART_BackButton");
        _contentPresenter = e.NameScope.Find<ContentPresenter>("PART_SelectedContentPresenter");
        _topBottomContentPresenter = e.NameScope.Find<ContentPresenter>("PART_TopBottomContentPresenter");
        _container = e.NameScope.Find<Border>("PART_Container");
        _mainGrid = e.NameScope.Find<Grid>("PART_SplitViewGrid");
        _dockPanel = e.NameScope.Find<DockPanel>("PART_ItemsPresenterDockPanel");
        _marginPanel = e.NameScope.Find<Border>("PART_MarginPanel");
        _topBottomLayout = e.NameScope.Find<DockPanel>("PART_TopBottomLayout");

        // Wire up top/bottom bar item selection — items are owned by TopItems/BottomItems, not Items.
        WireTopBottomItemSelection(e.NameScope.Find<ItemsControl>("PART_TopItemsControl"));
        WireTopBottomItemSelection(e.NameScope.Find<ItemsControl>("PART_BottomItemsControl"));

        Debug.WriteLine($"[NavigationView] OnApplyTemplate parts: headerItem={_headerItem is not null} backButton={_backButton is not null} contentPresenter={_contentPresenter is not null}");

        // Template re-application (e.g. theme change) gives us brand-new presenter instances.
        // Assign the stable wrapper controls to the new presenters. The wrappers never move
        // between presenters — they are permanently owned by their respective presenter.
        // Content routing works by placing the actual page content inside the active wrapper
        // and hiding the inactive wrapper, avoiding all single-visual-parent conflicts.
        // Use comprehensive safe assignment to prevent visual tree violations.
        Debug.WriteLine("[NavigationView] OnApplyTemplate safely assigning wrappers to presenters");
        bool leftAssigned = SafelyAssignWrapperToPresenter(_contentPresenter, _leftWrapper, "_leftWrapper");
        bool topBottomAssigned = SafelyAssignWrapperToPresenter(_topBottomContentPresenter, _topBottomWrapper, "_topBottomWrapper");
        Debug.WriteLine($"[NavigationView] OnApplyTemplate wrapper assignment results: left={leftAssigned}, topBottom={topBottomAssigned}");

        // Explicitly sync layout panel visibility with current Position (survives style invalidation).
        UpdateLayoutVisibility(Position);

        RestoreCachedContent();

        if (_headerItem != null)
        {
            _headerItem.Click += (_, _) => IsOpen = AlwaysOpen || !IsOpen;
        }

        BackButtonCommandProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<ICommand?>>(x =>
        {
            if (_backButton != null)
                _backButton.IsVisible = x.NewValue.Value is not null;
        }));

        if (TopLevel.GetTopLevel(this) is PleasantWindow window)
        {
            _window = window;
            titleBarHeight = window.TitleBarHeight;
            Debug.WriteLine($"[NavigationView] OnApplyTemplate PleasantWindow found titleBarHeight={titleBarHeight}");
            UpdateMacNavigationLayout(window);
            UpdateContainerTitleHeight(window);
            UpdateMarginPanel();
            UpdateTitleBarOffset(window);
            UpdateTopBottomLayout(window);

            window.GetObservable(PleasantWindow.TitleBarHeightProperty)
                .Subscribe(new AnonymousObserver<double>(h =>
                {
                    titleBarHeight = h;
                    UpdateContainerTitleHeight(window);
                    UpdateMarginPanel();
                    UpdateTopBottomLayout(window);
                }));

            // Auto-sync ButtonsPanelOffset with TitleBarType — Compact = offset on, others = off
            window.GetObservable(PleasantWindow.TitleBarTypeProperty)
                .Subscribe(new AnonymousObserver<PleasantTitleBar.Type>(type =>
                {
                    ButtonsPanelOffset = type == PleasantTitleBar.Type.Compact;
                }));

            this.GetObservable(ButtonsPanelOffsetProperty)
                .Subscribe(new AnonymousObserver<bool>(_ => UpdateTitleBarOffset(window)));

            this.GetObservable(CompactPaneLengthProperty)
                .Subscribe(new AnonymousObserver<double>(_ => UpdateTitleBarOffset(window)));
        }

        UpdateTitleAndSelectedContent();
    }
    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Debug.WriteLine($"[NavigationView] OnLoaded itemCount={Items.Count} firstItem={(Items.Count > 0 ? (Items[0] as NavigationViewItem)?.Header : "none")}");

        if (Position == NavigationViewPosition.Left)
        {
            if (Items.Count > 0 && Items[0] is ISelectable selectableItem)
            {
                Debug.WriteLine($"[NavigationView] OnLoaded selecting first item header={(selectableItem as NavigationViewItem)?.Header}");
                SelectSingleItem(selectableItem, false);
            }
        }
        else
        {
            var firstItems = Position == NavigationViewPosition.Top ? _topItems : _bottomItems;
            if (firstItems.Count > 0)
                SelectTopBottomItem(firstItems[0]);
        }
    }

    /// <inheritdoc />
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        Debug.WriteLine($"[NavigationView] OnAttachedToLogicalTree itemCount={Items.Count}");

        if (Position == NavigationViewPosition.Left && Items.Count > 0 && Items[0] is ISelectable selectableItem)
        {
            Debug.WriteLine($"[NavigationView] OnAttachedToLogicalTree selecting first item header={(selectableItem as NavigationViewItem)?.Header}");
            SelectSingleItem(selectableItem);
        }
    }

    internal void SelectSingleItem(ISelectable item, bool runAnimation = true)
    {
        Debug.WriteLine($"[NavigationView] SelectSingleItem header={(item as NavigationViewItem)?.Header} runAnimation={runAnimation}");
        // Always run deselection of other items, even if this item is already selected
        CloseAllSubMenuPopups();
        SelectSingleItemCore(item, runAnimation);
    }

    private void UpdateMacNavigationLayout(PleasantWindow window)
    {
        if (!window.EnableCustomTitleBar || !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return;

        window.GetObservable(Window.WindowStateProperty).Subscribe(new AnonymousObserver<WindowState>(state =>
        {
            if (state == WindowState.FullScreen)
            {
                if (_mainGrid is { RowDefinitions.Count: > 0 })
                    _mainGrid.RowDefinitions.RemoveAt(0);
                if (_stackPanelButtons != null)
                    _stackPanelButtons.Margin = new Thickness(5);
                if (_marginPanel != null)
                    Grid.SetRow(_marginPanel, 1);
                if (_dockPanel != null)
                    Grid.SetRow(_dockPanel, 2);
            }
            else if (_mainGrid != null && _marginPanel != null && _dockPanel != null &&
                     _stackPanelButtons != null && _headerItem != null)
            {
                if (_mainGrid.RowDefinitions.Count == 0 ||
                    _mainGrid.RowDefinitions[0].Height.Value != _headerItem.Height)
                {
                    _mainGrid.RowDefinitions.Insert(0, new RowDefinition { Height = new GridLength(_headerItem.Height, GridUnitType.Pixel) });
                }
                _stackPanelButtons.Margin = new Thickness(5, titleBarHeight + 6, 5, 5);
                Grid.SetRow(_marginPanel, 2);
                Grid.SetRow(_dockPanel, 3);
            }
        }));
    }

    private void UpdateContainerTitleHeight(PleasantWindow window)
    {
        if (_container == null) return;

        Thickness margin = window.EnableCustomTitleBar ? new Thickness(8, titleBarHeight + 1, 8, 8) : new Thickness(0);
        Debug.WriteLine($"[NavigationView] UpdateContainerTitleHeight enableCustomTitleBar={window.EnableCustomTitleBar} margin={margin}");

        _container.CornerRadius = new CornerRadius(8);
        _container.Margin = margin;
    }

    private void UpdateTitleBarOffset(PleasantWindow window)
    {
        if (!window.EnableCustomTitleBar) return;

        // Find the PleasantTitleBar in the window's template
        var titleBar = window.GetTemplateChildren().OfType<PleasantTitleBar>().FirstOrDefault();
        if (titleBar == null) return;

        if (Position != NavigationViewPosition.Left)
        {
            // Top/Bottom: no hamburger button at all — title hugs the left edge.
            titleBar.LeftClearance = 0;
        }
        else if (ButtonsPanelOffset)
        {
            // Left + hamburger below titlebar — remove clearance so logo hugs left
            titleBar.LeftClearance = 0;
        }
        else
        {
            // Left + hamburger overlaps titlebar — restore clearance so logo clears the hamburger
            titleBar.LeftClearance = 40;
        }
    }

    /// <summary>
    /// Applies the correct top margin to <c>PART_TopBottomLayout</c> so the horizontal nav bar
    /// sits flush below the custom titlebar (Top position) or the content area is inset correctly
    /// (Bottom position). For Left position this is a no-op.
    /// </summary>
    private void UpdateTopBottomLayout(PleasantWindow window)
    {
        if (_topBottomLayout is null) return;

        if (!window.EnableCustomTitleBar || Position == NavigationViewPosition.Left)
        {
            _topBottomLayout.Margin = new Thickness(0);
            return;
        }

        // For Top: push the entire DockPanel down by titleBarHeight so the bar clears the titlebar.
        // For Bottom: same — the content area starts below the titlebar, bar docks to the bottom.
        _topBottomLayout.Margin = new Thickness(0, titleBarHeight, 0, 0);
        Debug.WriteLine($"[NavigationView] UpdateTopBottomLayout position={Position} titleBarHeight={titleBarHeight} → margin=0,{titleBarHeight},0,0");
    }

    /// <summary>
    /// Explicitly sets IsVisible on the SplitView and TopBottomLayout panels to match the current Position.
    /// This bypasses pseudo-class styles which can fail to re-apply after style tree changes (e.g. VGUI theme add/remove).
    /// </summary>
    private void UpdateLayoutVisibility(NavigationViewPosition position)
    {
        // Find the SplitView named "split" in the template
        var splitView = this.GetTemplateChildren().OfType<SplitView>().FirstOrDefault(x => x.Name == "split");
        var stackPanelButtons = _stackPanelButtons;

        bool isLeft = position == NavigationViewPosition.Left;
        bool isTop = position == NavigationViewPosition.Top;
        bool isBottom = position == NavigationViewPosition.Bottom;

        if (splitView is not null)
            splitView.IsVisible = isLeft;
        if (stackPanelButtons is not null)
            stackPanelButtons.IsVisible = isLeft;
        if (_topBottomLayout is not null)
            _topBottomLayout.IsVisible = isTop || isBottom;

        // Show/hide the top or bottom bar within the layout
        var topBar = this.GetVisualDescendants().OfType<Border>().FirstOrDefault(x => x.Name == "PART_TopBar");
        var bottomBar = this.GetVisualDescendants().OfType<Border>().FirstOrDefault(x => x.Name == "PART_BottomBar");

        if (topBar is not null) topBar.IsVisible = isTop;
        if (bottomBar is not null) bottomBar.IsVisible = isBottom;

        Debug.WriteLine($"[NavigationView] UpdateLayoutVisibility position={position} splitView={splitView is not null} topBottomLayout={_topBottomLayout is not null}");
    }

    private void UpdateMarginPanel()
    {
        if (_marginPanel == null) return;

        bool noBackButton = !ShowBackButton || DisplayMode == SplitViewDisplayMode.Overlay;

        double result;
        if (ButtonsPanelOffset)
        {
            // Buttons panel top = titleBarHeight + 5 (margin).
            // Button heights: back (37, optional) + spacing (5) + hamburger (37) + bottom margin (5).
            double buttonsHeight = noBackButton
                ? 37 + 5          // hamburger + bottom margin
                : 37 + 5 + 37 + 5; // back + spacing + hamburger + bottom margin
            result = titleBarHeight + 5 + buttonsHeight + 5; // top margin + buttons + gap
        }
        else
        {
            // Original behavior: fixed heights from the AXAML template scaled to titleBarHeight.
            const double baselineTitleBarHeight = 44.0;
            double baseHeight = noBackButton ? 60.0 : 90.0;
            double delta = baseHeight - baselineTitleBarHeight;
            result = Math.Max(titleBarHeight + delta, baseHeight);
        }

        _marginPanel.Height = result;

        if (_stackPanelButtons != null)
        {
            double topMargin = ButtonsPanelOffset ? titleBarHeight + 5 : 5;
            _stackPanelButtons.Margin = new Thickness(5, topMargin, 5, 5);
        }

        Debug.WriteLine($"[NavigationView] UpdateMarginPanel titleBarHeight={titleBarHeight} noBack={noBackButton} offset={ButtonsPanelOffset} → {result}");
    }
    
    private void OnBoundsChanged(Rect rect)
    {
        // Ignore zero-size bounds — this fires during initial layout before the window is rendered.
        // Acting on it would corrupt the saved expanded state of all items.
        if (rect.Width <= 0)
        {
            Debug.WriteLine($"[NavigationView] OnBoundsChanged width={rect.Width:F0} → ignored (zero width, initial layout)");
            return;
        }

        // Dynamic display mode only applies to the left-pane layout.
        if (Position != NavigationViewPosition.Left) return;

        if (DynamicDisplayMode)
        {
            bool isLittle = rect.Width <= LittleWidth;
            bool isVeryLittle = rect.Width <= VeryLittleWidth;

            if (!isLittle && !isVeryLittle)
            {
                Debug.WriteLine($"[NavigationView] OnBoundsChanged width={rect.Width:F0} → CompactInline IsOpen stays");
                UpdatePseudoClasses(false);
                DisplayMode = SplitViewDisplayMode.CompactInline;
            }
            else if (isLittle && !isVeryLittle)
            {
                Debug.WriteLine($"[NavigationView] OnBoundsChanged width={rect.Width:F0} → CompactOverlay IsOpen=false");
                UpdatePseudoClasses(false);
                DisplayMode = SplitViewDisplayMode.CompactOverlay;
                IsOpen = false;
            }
            else if (isLittle && isVeryLittle)
            {
                Debug.WriteLine($"[NavigationView] OnBoundsChanged width={rect.Width:F0} → Overlay IsOpen=false");
                UpdatePseudoClasses(true);
                DisplayMode = SplitViewDisplayMode.Overlay;
                IsOpen = false;
            }
        }
    }

    private void SelectSingleItemCore(object? item, bool runAnimation = true)
    {
        Debug.WriteLine($"[NavigationView] SelectSingleItemCore item={(item as NavigationViewItem)?.Header} tag={(item as NavigationViewItem)?.Tag}");
        
        var activePresenter = Position == NavigationViewPosition.Left ? _contentPresenter : _topBottomContentPresenter;
        
        if (SelectedItem != item && TransitionAnimation is not null && activePresenter is not null && runAnimation)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            TransitionAnimation.RunAsync(activePresenter, _cancellationTokenSource.Token);
        }

        // Deselect all previously selected items in the tree before selecting the new one
        foreach (var navItem in this.GetLogicalDescendants().OfType<NavigationViewItem>())
        {
            if (navItem.IsSelected && !ReferenceEquals(navItem, item))
                navItem.IsSelected = false;
        }
        if (item is ISelectable selectableItem)
            selectableItem.IsSelected = true;

        SelectedItem = item;
    }

    private void UpdatePseudoClasses(bool isCompact)
    {
        Debug.WriteLine($"[NavigationView] UpdatePseudoClasses isCompact={isCompact}");
        switch (isCompact)
        {
            case true:
                PseudoClasses.Add(":compact");
                break;
            case false:
                PseudoClasses.Remove(":compact");
                break;
        }
    }

    private void OnPositionChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var position = (NavigationViewPosition)(e.NewValue ?? NavigationViewPosition.Left);
        PseudoClasses.Remove(":left");
        PseudoClasses.Remove(":top");
        PseudoClasses.Remove(":bottom");
        switch (position)
        {
            case NavigationViewPosition.Top:
                PseudoClasses.Add(":top");
                break;
            case NavigationViewPosition.Bottom:
                PseudoClasses.Add(":bottom");
                break;
            default:
                PseudoClasses.Add(":left");
                break;
        }
        Debug.WriteLine($"[NavigationView] OnPositionChanged position={position}");

        // Explicitly manage layout panel visibility to survive style invalidation
        // (e.g. when VGUI theme styles are added/removed, pseudo-class styles may not re-apply).
        UpdateLayoutVisibility(position);

        // Rebuild the Top/Bottom proxy item collections from Items.
        RebuildTopBottomProxies(position);

        // Update top/bottom layout margins and titlebar clearance for the new position.
        if (_window is not null)
        {
            UpdateTopBottomLayout(_window);
            UpdateTitleBarOffset(_window);
        }

        // Re-route the current content to the now-active presenter.
        UpdateTitleAndSelectedContent();
    }

    /// <summary>
    /// Rebuilds <see cref="TopItems"/> or <see cref="BottomItems"/> with proxy clones of the
    /// top-level items when switching to a horizontal layout, or clears them when
    /// returning to the Left layout.  Proxies are new instances — they never share visual identity
    /// with the originals, so Avalonia's single-visual-parent rule is never violated.
    /// </summary>
    private void RebuildTopBottomProxies(NavigationViewPosition position)
    {
        _topItems.Clear();
        _bottomItems.Clear();

        if (position == NavigationViewPosition.Left)
            return;

        var target = position == NavigationViewPosition.Top ? _topItems : _bottomItems;

        foreach (var original in Items.OfType<NavigationViewItem>())
        {
            var proxy = CreateProxy(original);
            target.Add(proxy);
        }
    }

    /// <summary>
    /// Creates a proxy <see cref="NavigationViewItem"/> that mirrors the data of <paramref name="original"/>
    /// without sharing any visual or logical parent.  Selection on the proxy is forwarded to the original
    /// so that <see cref="UpdateTitleAndSelectedContent"/> always operates on the real item.
    /// </summary>
    private NavigationViewItem CreateProxy(NavigationViewItem original)
    {
        var proxy = new NavigationViewItem
        {
            Header  = original.Header,
            Icon    = original.Icon,
            Tag     = original.Tag,
            Content = original.Content,
            // Give the proxy a reference to this NavigationView so its Select() can call back.
            NavigationView = this,
        };

        // Forward proxy selection → original selection so content routing works correctly.
        proxy.GetObservable(NavigationViewItem.IsSelectedProperty)
            .Subscribe(new AnonymousObserver<bool>(isSelected =>
            {
                if (!isSelected) return;
                // Deselect all other proxies in the same collection.
                var collection = Position == NavigationViewPosition.Top
                    ? (IEnumerable<NavigationViewItem>)_topItems
                    : _bottomItems;
                foreach (var other in collection)
                    if (!ReferenceEquals(other, proxy))
                        other.IsSelected = false;

                // Mirror selection onto the original so UpdateTitleAndSelectedContent
                // picks up the right Content.
                if (!original.IsSelected)
                    original.IsSelected = true;

                // Drive content update directly — SelectedItem won't change because
                // the proxy is not in Items, so OnSelectedItemChanged won't fire.
                SelectedContent = original.Content;
                _cachedContent = original.Content;
                _cachedContentPosition = Position;

                RouteContentToWrapper(original.Content);
            }));

        return proxy;
    }

    /// <summary>
    /// Validates that the current operation is running on the UI thread.
    /// This is critical for all visual tree operations to prevent cross-thread exceptions.
    /// </summary>
    /// <param name="operationName">Name of the operation being performed for logging.</param>
    /// <returns>True if on UI thread, false otherwise.</returns>
    private bool ValidateUiThreadAccess(string operationName)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Debug.WriteLine($"[NavigationView] ValidateUiThreadAccess {operationName} — NOT on UI thread, operation may fail");
            return false;
        }
        
        Debug.WriteLine($"[NavigationView] ValidateUiThreadAccess {operationName} — UI thread validated");
        return true;
    }

    /// <summary>
    /// Validates that a control is not disposed and is still usable.
    /// Prevents operations on disposed controls which can cause crashes.
    /// Null controls are treated as valid (we just can't validate them) to allow
    /// early initialization before template is applied.
    /// </summary>
    /// <param name="control">The control to validate.</param>
    /// <param name="controlName">Name of the control for logging.</param>
    /// <returns>True if control is valid or null, false if disposed.</returns>
    private bool ValidateControlNotDisposed(Control? control, string controlName)
    {
        if (control is null)
        {
            Debug.WriteLine($"[NavigationView] ValidateControlNotDisposed {controlName} is null — treating as valid (early initialization)");
            return true; // Treat null as valid - we can't validate it, but that's okay
        }

        // Check if the control is in a disposed state by checking its visual parent chain
        // A disposed control typically has no visual parent or is detached from the tree
        try
        {
            var isAttached = control.IsAttachedToVisualTree();
            if (!isAttached)
            {
                Debug.WriteLine($"[NavigationView] ValidateControlNotDisposed {controlName} is not attached to visual tree — may be disposed or initializing");
                // We still allow operations on unattached controls as they might be in the process of being attached
                // This is a warning, not a failure
            }
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NavigationView] ValidateControlNotDisposed {controlName} validation FAILED: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Re-looks up the content presenter references from the current live template.
    /// Called when we detect that a stored presenter reference is stale (detached from visual tree)
    /// after a theme switch that didn't trigger OnApplyTemplate.
    /// </summary>
    private void RefreshPresentersFromTemplate()
    {
        var newLeft = this.GetVisualDescendants()
            .OfType<ContentPresenter>()
            .FirstOrDefault(x => x.Name == "PART_SelectedContentPresenter");
        var newTopBottom = this.GetVisualDescendants()
            .OfType<ContentPresenter>()
            .FirstOrDefault(x => x.Name == "PART_TopBottomContentPresenter");

        if (newLeft is not null && !ReferenceEquals(newLeft, _contentPresenter))
        {
            Debug.WriteLine("[NavigationView] RefreshPresentersFromTemplate — refreshed _contentPresenter");
            _contentPresenter = newLeft;
        }
        if (newTopBottom is not null && !ReferenceEquals(newTopBottom, _topBottomContentPresenter))
        {
            Debug.WriteLine("[NavigationView] RefreshPresentersFromTemplate — refreshed _topBottomContentPresenter");
            _topBottomContentPresenter = newTopBottom;
        }
    }

    /// <summary>
    /// Validates that a wrapper and presenter are compatible for assignment.
    /// Checks for circular references and invalid state combinations.
    /// </summary>
    /// <param name="wrapper">The wrapper Border to validate.</param>
    /// <param name="presenter">The presenter to validate.</param>
    /// <param name="wrapperName">Name of the wrapper for logging.</param>
    /// <returns>True if compatible, false otherwise.</returns>
    private bool ValidateWrapperPresenterCompatibility(Border? wrapper, ContentPresenter? presenter, string wrapperName)
    {
        if (wrapper is null || presenter is null)
        {
            Debug.WriteLine($"[NavigationView] ValidateWrapperPresenterCompatibility {wrapperName} — null wrapper or presenter");
            return false;
        }

        // Check if wrapper is already a child of the presenter (valid state)
        if (ReferenceEquals(presenter.Content, wrapper))
        {
            // Also verify the presenter is still attached to the live visual tree.
            // After a theme switch that doesn't trigger OnApplyTemplate, the presenter
            // may be a stale instance from the old template — detached from the visual tree.
            // Note: during OnApplyTemplate itself the presenter may not yet be attached,
            // so we only treat it as stale if we can find a live replacement.
            if (!presenter.IsAttachedToVisualTree())
            {
                // Try to find a live replacement in the current visual tree.
                var liveLeft = this.GetVisualDescendants()
                    .OfType<ContentPresenter>()
                    .FirstOrDefault(x => x.Name == "PART_SelectedContentPresenter" && x.IsAttachedToVisualTree());
                var liveTopBottom = this.GetVisualDescendants()
                    .OfType<ContentPresenter>()
                    .FirstOrDefault(x => x.Name == "PART_TopBottomContentPresenter" && x.IsAttachedToVisualTree());

                bool foundLiveReplacement = (ReferenceEquals(presenter, _contentPresenter) && liveLeft is not null && !ReferenceEquals(liveLeft, presenter))
                                         || (ReferenceEquals(presenter, _topBottomContentPresenter) && liveTopBottom is not null && !ReferenceEquals(liveTopBottom, presenter));

                if (foundLiveReplacement)
                {
                    Debug.WriteLine($"[NavigationView] ValidateWrapperPresenterCompatibility {wrapperName} assigned to DETACHED presenter but live replacement found — stale, needs re-assignment");
                    if (liveLeft is not null && !ReferenceEquals(liveLeft, _contentPresenter)) _contentPresenter = liveLeft;
                    if (liveTopBottom is not null && !ReferenceEquals(liveTopBottom, _topBottomContentPresenter)) _topBottomContentPresenter = liveTopBottom;
                    return false; // force re-assignment with the live presenter
                }
                // No live replacement found yet (mid-template-application) — treat as valid
                Debug.WriteLine($"[NavigationView] ValidateWrapperPresenterCompatibility {wrapperName} assigned to presenter not yet attached — treating as valid (mid-apply)");
            }
            Debug.WriteLine($"[NavigationView] ValidateWrapperPresenterCompatibility {wrapperName} already assigned to presenter — valid");
            return true;
        }

        // Check if wrapper's child is the presenter (invalid circular reference)
        if (ReferenceEquals(wrapper.Child, presenter))
        {
            Debug.WriteLine($"[NavigationView] ValidateWrapperPresenterCompatibility {wrapperName} has presenter as child — INVALID circular reference");
            return false;
        }

        // Check if presenter is already a child of wrapper (invalid circular reference)
        var presenterParent = presenter.GetVisualParent();
        if (ReferenceEquals(presenterParent, wrapper))
        {
            Debug.WriteLine($"[NavigationView] ValidateWrapperPresenterCompatibility {wrapperName} has presenter as visual parent — INVALID circular reference");
            return false;
        }

        Debug.WriteLine($"[NavigationView] ValidateWrapperPresenterCompatibility {wrapperName} — compatible");
        return true;
    }

    /// <summary>
    /// Validates that content and wrapper are compatible for assignment.
    /// Checks for circular references and invalid state combinations.
    /// </summary>
    /// <param name="content">The content control to validate.</param>
    /// <param name="wrapper">The wrapper Border to validate.</param>
    /// <param name="contentDescription">Description of the content for logging.</param>
    /// <returns>True if compatible, false otherwise.</returns>
    private bool ValidateContentWrapperCompatibility(Control? content, Border? wrapper, string contentDescription)
    {
        if (content is null || wrapper is null)
        {
            Debug.WriteLine($"[NavigationView] ValidateContentWrapperCompatibility {contentDescription} — null content or wrapper");
            return false;
        }

        // Check if content is already a child of the wrapper (valid state)
        if (ReferenceEquals(wrapper.Child, content))
        {
            Debug.WriteLine($"[NavigationView] ValidateContentWrapperCompatibility {contentDescription} already assigned to wrapper — valid");
            return true;
        }

        // Check if content's child is the wrapper (invalid circular reference)
        if (content is ContentPresenter contentPresenter && ReferenceEquals(contentPresenter.Content, wrapper))
        {
            Debug.WriteLine($"[NavigationView] ValidateContentWrapperCompatibility {contentDescription} has wrapper as content — INVALID circular reference");
            return false;
        }

        // Check if wrapper is already a child of content (invalid circular reference)
        var wrapperParent = wrapper.GetVisualParent();
        if (ReferenceEquals(wrapperParent, content))
        {
            Debug.WriteLine($"[NavigationView] ValidateContentWrapperCompatibility {contentDescription} has wrapper as visual parent — INVALID circular reference");
            return false;
        }

        Debug.WriteLine($"[NavigationView] ValidateContentWrapperCompatibility {contentDescription} — compatible");
        return true;
    }

    /// <summary>
    /// Safely detaches a wrapper Border from any current visual parent before reassignment.
    /// This prevents the "already has a visual parent" error during template re-application.
    /// Performs comprehensive validation and logging for all operations.
    /// </summary>
    /// <param name="wrapper">The wrapper Border to detach.</param>
    /// <param name="targetPresenter">The target ContentPresenter we want to assign the wrapper to.</param>
    /// <param name="wrapperName">Name of the wrapper for logging (e.g., "_leftWrapper").</param>
    /// <returns>True if detachment was successful or not needed, false if an error occurred.</returns>
    private bool SafelyDetachWrapperFromVisualParent(Border wrapper, ContentPresenter? targetPresenter, string wrapperName)
    {
        if (wrapper is null)
        {
            Debug.WriteLine($"[NavigationView] SafelyDetachWrapperFromVisualParent {wrapperName} is null — skipping");
            return false;
        }

        // Check if wrapper has a visual parent
        var currentVisualParent = wrapper.GetVisualParent();
        
        if (currentVisualParent is null)
        {
            Debug.WriteLine($"[NavigationView] SafelyDetachWrapperFromVisualParent {wrapperName} has no visual parent — safe to assign");
            return true;
        }

        // Check if the current visual parent is already the target presenter
        if (ReferenceEquals(currentVisualParent, targetPresenter))
        {
            Debug.WriteLine($"[NavigationView] SafelyDetachWrapperFromVisualParent {wrapperName} already parented to target presenter — no action needed");
            return true;
        }

        // Check if the current visual parent is a ContentPresenter
        if (currentVisualParent is ContentPresenter currentPresenter)
        {
            Debug.WriteLine($"[NavigationView] SafelyDetachWrapperFromVisualParent {wrapperName} currently parented to ContentPresenter — detaching");
            
            try
            {
                // Clear the content of the current presenter to detach the wrapper
                currentPresenter.Content = null;
                Debug.WriteLine($"[NavigationView] SafelyDetachWrapperFromVisualParent {wrapperName} successfully detached from old ContentPresenter");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NavigationView] SafelyDetachWrapperFromVisualParent {wrapperName} FAILED to detach from ContentPresenter: {ex.Message}");
                return false;
            }
        }

        // If the parent is not a ContentPresenter, we cannot safely detach it
        // This is an unexpected state - log it but don't throw
        Debug.WriteLine($"[NavigationView] SafelyDetachWrapperFromVisualParent {wrapperName} has unexpected visual parent type {currentVisualParent.GetType().Name} — cannot safely detach");
        return false;
    }

    /// <summary>
    /// Safely assigns a wrapper Border to a ContentPresenter with full validation.
    /// Performs comprehensive checks to prevent visual tree violations.
    /// Null presenters are handled gracefully - assignment is skipped but validation passes
    /// to allow early initialization before template is applied.
    /// </summary>
    /// <param name="presenter">The target ContentPresenter.</param>
    /// <param name="wrapper">The wrapper Border to assign.</param>
    /// <param name="wrapperName">Name of the wrapper for logging.</param>
    /// <returns>True if assignment was successful or skipped (null presenter), false otherwise.</returns>
    private bool SafelyAssignWrapperToPresenter(ContentPresenter? presenter, Border? wrapper, string wrapperName)
    {
        // Validate UI thread access
        if (!ValidateUiThreadAccess($"SafelyAssignWrapperToPresenter({wrapperName})"))
        {
            Debug.WriteLine($"[NavigationView] SafelyAssignWrapperToPresenter {wrapperName} — UI thread validation failed");
            return false;
        }

        // Validate controls are not disposed (null is now treated as valid)
        if (!ValidateControlNotDisposed(presenter, $"presenter for {wrapperName}"))
        {
            Debug.WriteLine($"[NavigationView] SafelyAssignWrapperToPresenter {wrapperName} — presenter validation failed");
            return false;
        }

        if (!ValidateControlNotDisposed(wrapper, wrapperName))
        {
            Debug.WriteLine($"[NavigationView] SafelyAssignWrapperToPresenter {wrapperName} — wrapper validation failed");
            return false;
        }

        if (presenter is null)
        {
            Debug.WriteLine($"[NavigationView] SafelyAssignWrapperToPresenter presenter is null for {wrapperName} — skipping assignment (early initialization)");
            return true; // Return true to indicate this is acceptable (not a failure)
        }

        if (wrapper is null)
        {
            Debug.WriteLine($"[NavigationView] SafelyAssignWrapperToPresenter {wrapperName} is null — skipping assignment");
            return false;
        }

        // Validate wrapper and presenter compatibility
        if (!ValidateWrapperPresenterCompatibility(wrapper, presenter, wrapperName))
        {
            Debug.WriteLine($"[NavigationView] SafelyAssignWrapperToPresenter {wrapperName} — compatibility validation failed");
            return false;
        }

        // Check if already correctly assigned
        if (ReferenceEquals(presenter.Content, wrapper))
        {
            Debug.WriteLine($"[NavigationView] SafelyAssignWrapperToPresenter {wrapperName} already assigned to presenter — no action needed");
            return true;
        }

        // First, safely detach the wrapper from any current visual parent
        if (!SafelyDetachWrapperFromVisualParent(wrapper, presenter, wrapperName))
        {
            Debug.WriteLine($"[NavigationView] SafelyAssignWrapperToPresenter {wrapperName} detachment failed — aborting assignment");
            return false;
        }

        // Now safely assign the wrapper to the presenter
        try
        {
            presenter.Content = wrapper;
            Debug.WriteLine($"[NavigationView] SafelyAssignWrapperToPresenter {wrapperName} successfully assigned to presenter");
            return true;
        }
        catch (InvalidOperationException ex)
        {
            Debug.WriteLine($"[NavigationView] SafelyAssignWrapperToPresenter {wrapperName} assignment FAILED with InvalidOperationException: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NavigationView] SafelyAssignWrapperToPresenter {wrapperName} assignment FAILED with unexpected exception: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Safely detaches content control from any current visual parent before assigning to a wrapper.
    /// This prevents visual tree violations when content is moved between wrappers.
    /// </summary>
    /// <param name="content">The content control to detach.</param>
    /// <param name="targetWrapper">The target wrapper we want to assign the content to.</param>
    /// <param name="contentDescription">Description of the content for logging.</param>
    /// <returns>True if detachment was successful or not needed, false if an error occurred.</returns>
    private bool SafelyDetachContentFromVisualParent(Control? content, Border? targetWrapper, string contentDescription)
    {
        if (content is null)
        {
            Debug.WriteLine($"[NavigationView] SafelyDetachContentFromVisualParent {contentDescription} is null — skipping");
            return true;
        }

        if (targetWrapper is null)
        {
            Debug.WriteLine($"[NavigationView] SafelyDetachContentFromVisualParent target wrapper is null for {contentDescription} — skipping");
            return false;
        }

        // Check if content has a visual parent
        var currentVisualParent = content.GetVisualParent();
        
        if (currentVisualParent is null)
        {
            Debug.WriteLine($"[NavigationView] SafelyDetachContentFromVisualParent {contentDescription} has no visual parent — safe to assign");
            return true;
        }

        // Check if the current visual parent is already the target wrapper
        if (ReferenceEquals(currentVisualParent, targetWrapper))
        {
            Debug.WriteLine($"[NavigationView] SafelyDetachContentFromVisualParent {contentDescription} already parented to target wrapper — no action needed");
            return true;
        }

        // Check if the current visual parent is a Border (wrapper)
        if (currentVisualParent is Border currentWrapper)
        {
            Debug.WriteLine($"[NavigationView] SafelyDetachContentFromVisualParent {contentDescription} currently parented to Border wrapper — detaching");
            
            try
            {
                // Clear the child of the current wrapper to detach the content
                currentWrapper.Child = null;
                Debug.WriteLine($"[NavigationView] SafelyDetachContentFromVisualParent {contentDescription} successfully detached from old Border wrapper");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NavigationView] SafelyDetachContentFromVisualParent {contentDescription} FAILED to detach from Border wrapper: {ex.Message}");
                return false;
            }
        }

        // If the parent is not a Border, we cannot safely detach it using this method
        // This is an unexpected state - log it but don't throw
        Debug.WriteLine($"[NavigationView] SafelyDetachContentFromVisualParent {contentDescription} has unexpected visual parent type {currentVisualParent.GetType().Name} — cannot safely detach");
        return false;
    }

    /// <summary>
    /// Safely assigns content control to a wrapper Border with full validation.
    /// Performs comprehensive checks to prevent visual tree violations.
    /// Null wrappers are handled gracefully - assignment is skipped but validation passes
    /// to allow early initialization before template is applied.
    /// </summary>
    /// <param name="wrapper">The target wrapper Border.</param>
    /// <param name="content">The content control to assign.</param>
    /// <param name="contentDescription">Description of the content for logging.</param>
    /// <returns>True if assignment was successful or skipped (null wrapper), false otherwise.</returns>
    private bool SafelyAssignContentToWrapper(Border? wrapper, Control? content, string contentDescription)
    {
        // Validate UI thread access
        if (!ValidateUiThreadAccess($"SafelyAssignContentToWrapper({contentDescription})"))
        {
            Debug.WriteLine($"[NavigationView] SafelyAssignContentToWrapper {contentDescription} — UI thread validation failed");
            return false;
        }

        // Validate controls are not disposed (null is now treated as valid)
        if (!ValidateControlNotDisposed(wrapper, $"wrapper for {contentDescription}"))
        {
            Debug.WriteLine($"[NavigationView] SafelyAssignContentToWrapper {contentDescription} — wrapper validation failed");
            return false;
        }

        if (!ValidateControlNotDisposed(content, contentDescription))
        {
            Debug.WriteLine($"[NavigationView] SafelyAssignContentToWrapper {contentDescription} — content validation failed");
            return false;
        }

        if (wrapper is null)
        {
            Debug.WriteLine($"[NavigationView] SafelyAssignContentToWrapper wrapper is null for {contentDescription} — skipping assignment (early initialization)");
            return true; // Return true to indicate this is acceptable (not a failure)
        }

        if (content is null)
        {
            Debug.WriteLine($"[NavigationView] SafelyAssignContentToWrapper {contentDescription} is null — skipping assignment");
            return false;
        }

        // Validate content and wrapper compatibility
        if (!ValidateContentWrapperCompatibility(content, wrapper, contentDescription))
        {
            Debug.WriteLine($"[NavigationView] SafelyAssignContentToWrapper {contentDescription} — compatibility validation failed");
            return false;
        }

        // Check if already correctly assigned
        if (ReferenceEquals(wrapper.Child, content))
        {
            Debug.WriteLine($"[NavigationView] SafelyAssignContentToWrapper {contentDescription} already assigned to wrapper — no action needed");
            return true;
        }

        // First, safely detach the content from any current visual parent
        if (!SafelyDetachContentFromVisualParent(content, wrapper, contentDescription))
        {
            Debug.WriteLine($"[NavigationView] SafelyAssignContentToWrapper {contentDescription} detachment failed — aborting assignment");
            return false;
        }

        // Now safely assign the content to the wrapper
        try
        {
            wrapper.Child = content;
            Debug.WriteLine($"[NavigationView] SafelyAssignContentToWrapper {contentDescription} successfully assigned to wrapper");
            return true;
        }
        catch (InvalidOperationException ex)
        {
            Debug.WriteLine($"[NavigationView] SafelyAssignContentToWrapper {contentDescription} assignment FAILED with InvalidOperationException: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NavigationView] SafelyAssignContentToWrapper {contentDescription} assignment FAILED with unexpected exception: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Detaches <paramref name="contentVisual"/> from its current visual parent presenter
    /// <summary>
    /// Routes content into the correct wrapper for the current position.
    /// The wrappers are permanently assigned to their presenters — content moves between
    /// wrappers (Border.Child), never between ContentPresenters. This eliminates all
    /// single-visual-parent conflicts because Border has no parent validator.
    /// Uses comprehensive safe assignment to prevent visual tree violations.
    /// </summary>
    private void RouteContentToWrapper(object? content)
    {
        if (content is null) return;

        bool isLeft = Position == NavigationViewPosition.Left;
        var activeWrapper   = isLeft ? _leftWrapper      : _topBottomWrapper;
        var inactiveWrapper = isLeft ? _topBottomWrapper : _leftWrapper;

        // Clear inactive wrapper child with comprehensive validation.
        if (!ReferenceEquals(inactiveWrapper.Child, null))
        {
            Debug.WriteLine($"[NavigationView] RouteContentToWrapper clearing inactive wrapper child");
            inactiveWrapper.Child = null;
        }
        
        // Assign content to active wrapper with comprehensive safety checks.
        // This ensures content is safely detached from any existing parent before assignment.
        if (!ReferenceEquals(activeWrapper.Child, content as Control))
        {
            Debug.WriteLine($"[NavigationView] RouteContentToWrapper safely assigning content to active wrapper");
            bool contentAssigned = SafelyAssignContentToWrapper(activeWrapper, content as Control, $"content for {(isLeft ? "Left" : "TopBottom")} position");
            Debug.WriteLine($"[NavigationView] RouteContentToWrapper content assignment result: {contentAssigned}");
        }

        // Ensure wrappers are assigned to their presenters (survives template re-application).
        // Use comprehensive safe assignment to prevent visual tree violations.
        Debug.WriteLine($"[NavigationView] RouteContentToWrapper ensuring wrappers are assigned to presenters");
        bool leftAssigned = SafelyAssignWrapperToPresenter(_contentPresenter, _leftWrapper, "_leftWrapper");
        bool topBottomAssigned = SafelyAssignWrapperToPresenter(_topBottomContentPresenter, _topBottomWrapper, "_topBottomWrapper");
        Debug.WriteLine($"[NavigationView] RouteContentToWrapper wrapper assignment results: left={leftAssigned}, topBottom={topBottomAssigned}");

        Debug.WriteLine($"[NavigationView] RouteContentToWrapper isLeft={isLeft}");
    }

    private void UpdateTitleAndSelectedContent()
    {
        if (SelectedItem is not NavigationViewItem item) return;
        Debug.WriteLine($"[NavigationView] UpdateTitleAndSelectedContent selectedItem header={item.Header} hasContent={item.Content is not null}");
        if (item.Content is null) return;

        SelectedContent = item.Content;
        _cachedContent = item.Content;
        _cachedContentPosition = Position;

        RouteContentToWrapper(item.Content);
    }

    /// <summary>
    /// Restores the cached content after a template re-application.
    /// Re-assigns wrappers to the new presenter instances and re-routes cached content.
    /// Uses comprehensive safe assignment to prevent visual tree violations.
    /// </summary>
    private void RestoreCachedContent()
    {
        // Always re-assign wrappers to the (possibly new) presenter instances.
        // Use comprehensive safe assignment to prevent visual tree violations.
        Debug.WriteLine($"[NavigationView] RestoreCachedContent safely assigning wrappers to presenters");
        bool leftAssigned = SafelyAssignWrapperToPresenter(_contentPresenter, _leftWrapper, "_leftWrapper");
        bool topBottomAssigned = SafelyAssignWrapperToPresenter(_topBottomContentPresenter, _topBottomWrapper, "_topBottomWrapper");
        Debug.WriteLine($"[NavigationView] RestoreCachedContent wrapper assignment results: left={leftAssigned}, topBottom={topBottomAssigned}");

        if (_cachedContent is null) return;

        // Always use the current Position, not the stale cached position.
        // The cached position can be out-of-date after a theme switch that triggers
        // OnApplyTemplate while the nav layout has already changed (e.g. Top→Left).
        _cachedContentPosition = Position;
        Debug.WriteLine($"[NavigationView] RestoreCachedContent cachedPosition={_cachedContentPosition}");

        bool isLeft = Position == NavigationViewPosition.Left;
        var activeWrapper   = isLeft ? _leftWrapper      : _topBottomWrapper;
        var inactiveWrapper = isLeft ? _topBottomWrapper : _leftWrapper;

        // Clear inactive wrapper child with comprehensive validation.
        if (!ReferenceEquals(inactiveWrapper.Child, null))
        {
            Debug.WriteLine($"[NavigationView] RestoreCachedContent clearing inactive wrapper child");
            inactiveWrapper.Child = null;
        }
        
        // Assign cached content to active wrapper with comprehensive safety checks.
        // This ensures content is safely detached from any existing parent before assignment.
        if (!ReferenceEquals(activeWrapper.Child, _cachedContent as Control))
        {
            Debug.WriteLine($"[NavigationView] RestoreCachedContent safely assigning cached content to active wrapper");
            bool contentAssigned = SafelyAssignContentToWrapper(activeWrapper, _cachedContent as Control, $"cached content for {(isLeft ? "Left" : "TopBottom")} position");
            Debug.WriteLine($"[NavigationView] RestoreCachedContent content assignment result: {contentAssigned}");
        }

        Debug.WriteLine($"[NavigationView] RestoreCachedContent done isLeft={isLeft}");
    }

    private void OnSelectedItemChanged()
    {
        Debug.WriteLine($"[NavigationView] OnSelectedItemChanged → SelectedItem={(SelectedItem as NavigationViewItem)?.Header}");

        if (SelectedItem is null && _cachedContent is not null)
        {
            Debug.WriteLine("[NavigationView] OnSelectedItemChanged SelectedItem=null but cache exists — restoring");
            RestoreCachedContent();

            // After a theme switch that doesn't trigger OnApplyTemplate, the presenters may be
            // stale (pointing to the old template's instances). Schedule a deferred re-selection
            // so that once the new template is fully applied and items are re-attached, the
            // correct item gets selected and content is routed through live presenters.
            Dispatcher.UIThread.Post(() =>
            {
                if (SelectedItem is not null) return; // already re-selected by something else

                // Re-sync layout visibility in case style invalidation reset it.
                UpdateLayoutVisibility(Position);

                if (Position == NavigationViewPosition.Left)
                {
                    if (Items.Count > 0 && Items[0] is ISelectable first)
                    {
                        Debug.WriteLine($"[NavigationView] OnSelectedItemChanged deferred re-select header={(first as NavigationViewItem)?.Header}");
                        SelectSingleItem(first, false);
                    }
                }
                else
                {
                    var collection = Position == NavigationViewPosition.Top ? _topItems : _bottomItems;
                    if (collection.Count > 0)
                        SelectTopBottomItem(collection[0]);
                }
            }, DispatcherPriority.Loaded);

            return;
        }

        UpdateTitleAndSelectedContent();
    }

    /// <summary>
    /// Ensures cached content is visible — with wrapper pattern, just calls RestoreCachedContent.
    /// </summary>
    private void EnsureCachedContentVisible() => RestoreCachedContent();

    /// <summary>No-op — wrapper pattern makes direct detach unnecessary.</summary>
    private static void DetachFromPresenter(Visual contentVisual)
    {
        Debug.WriteLine("[NavigationView] DetachFromPresenter — no-op (wrapper pattern)");
    }

    /// <summary>
    /// Called when the pane IsOpen state changes.
    /// Saves IsExpanded state of all group items when collapsing to compact mode,
    /// and restores it when expanding back to full mode.
    /// </summary>
    private void OnIsOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        bool isNowOpen = (bool)(e.NewValue ?? false);
        Debug.WriteLine($"[NavigationView] OnIsOpenChanged isNowOpen={isNowOpen}");

        // Only group items (items that have NavigationViewItem children) participate in expand/collapse
        var groupItems = this.GetLogicalDescendants()
            .OfType<NavigationViewItem>()
            .Where(item => item.Items.OfType<NavigationViewItem>().Any())
            .ToList();

        if (!isNowOpen)
        {
            // Pane collapsing → save IsExpanded for each group item, then collapse it
            _expandedStates.Clear();
            foreach (var item in groupItems)
            {
                _expandedStates[item] = item.IsExpanded;
                Debug.WriteLine($"[NavigationView] OnIsOpenChanged saving header={item.Header} IsExpanded={item.IsExpanded}");
                if (item.IsExpanded)
                    item.IsExpanded = false;
            }
        }
        else
        {
            // Pane opening → restore saved IsExpanded for each group item
            foreach (var item in groupItems)
            {
                if (_expandedStates.TryGetValue(item, out bool wasExpanded))
                {
                    Debug.WriteLine($"[NavigationView] OnIsOpenChanged restoring header={item.Header} wasExpanded={wasExpanded}");
                    item.IsExpanded = wasExpanded;
                }
            }
            _expandedStates.Clear();
        }
    }

    /// <summary>
    /// Closes all open submenu popups in the navigation view.
    /// Called when a selection is made to ensure popups are dismissed.
    /// </summary>
    private void CloseAllSubMenuPopups()
    {
        foreach (var item in this.GetLogicalDescendants().OfType<NavigationViewItem>())
        {
            if (item.IsSubMenuOpen)
            {
                Debug.WriteLine($"[NavigationView] CloseAllSubMenuPopups closing popup on header={item.Header}");
                item.IsSubMenuOpen = false;
            }
        }
    }

    /// <summary>
    /// Subscribes to pointer-released events on all items inside a top/bottom <see cref="ItemsControl"/>
    /// so that clicking an item triggers selection and content update.
    /// Items are owned by <see cref="TopItems"/> or <see cref="BottomItems"/> — never by <see cref="ItemsControl.Items"/>.
    /// </summary>
    private void WireTopBottomItemSelection(ItemsControl? control)
    {
        if (control is null) return;

        // Use AddHandler with handledEventsToo=false so we catch the bubble from NavigationViewItem children.
        control.AddHandler(PointerReleasedEvent, (_, args) =>
        {
            // Walk up from the original source to find the NavigationViewItem that was clicked.
            var source = args.Source as Avalonia.Visual;
            while (source is not null)
            {
                if (source is NavigationViewItem navItem && control.Items.Contains(navItem))
                {
                    SelectTopBottomItem(navItem);
                    break;
                }
                source = source.GetVisualParent();
            }
        }, handledEventsToo: false);
    }

    private void SelectTopBottomItem(NavigationViewItem item)
    {
        Debug.WriteLine($"[NavigationView] SelectTopBottomItem header={item.Header}");

        // Deselect all top+bottom items except the clicked one.
        foreach (var i in _topItems)
            i.IsSelected = !ReferenceEquals(i, item) ? false : true;
        foreach (var i in _bottomItems)
            i.IsSelected = !ReferenceEquals(i, item) ? false : true;

        if (item.Content is not null)
        {
            if (TransitionAnimation is not null && _topBottomContentPresenter is not null)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                TransitionAnimation.RunAsync(_topBottomContentPresenter, _cancellationTokenSource.Token);
            }
            SelectedContent = item.Content;
            _cachedContent = item.Content;
            _cachedContentPosition = Position;

            RouteContentToWrapper(item.Content);
        }
    }
}
