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
using PleasantUI.Controls.Chrome;

namespace PleasantUI.Controls;

/// <summary>
/// Represents a navigation view control that displays a tree of items.
/// </summary>
/// <remarks>
/// The <c>NavigationView</c> control inherits from the <see cref="TreeView" /> control and adds additional
/// properties for customizing the appearance and behavior of the navigation view.
/// </remarks>
[PseudoClasses(":normal", ":compact")]
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

    private Button? _backButton;
    private ICommand? _backButtonCommand;

    private CancellationTokenSource? _cancellationTokenSource;
    private ContentPresenter? _contentPresenter;

    private Button? _headerItem;

    private IEnumerable<string>? _itemsAsStrings;

    private object? _selectedContent;
        
    private ILogical? _logicalSelectedContent;

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
        AvaloniaProperty.Register<NavigationView, bool>(nameof(ButtonsPanelOffset), true);

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
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationView"/> class.
    /// </summary>
    public NavigationView()
    {
        PseudoClasses.Add(":normal");
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
        _container = e.NameScope.Find<Border>("PART_Container");
        _mainGrid = e.NameScope.Find<Grid>("PART_SplitViewGrid");
        _dockPanel = e.NameScope.Find<DockPanel>("PART_ItemsPresenterDockPanel");
        _marginPanel = e.NameScope.Find<Border>("PART_MarginPanel");

        Debug.WriteLine($"[NavigationView] OnApplyTemplate parts: headerItem={_headerItem is not null} backButton={_backButton is not null} contentPresenter={_contentPresenter is not null}");

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
            titleBarHeight = window.TitleBarHeight;
            Debug.WriteLine($"[NavigationView] OnApplyTemplate PleasantWindow found titleBarHeight={titleBarHeight}");
            UpdateMacNavigationLayout(window);
            UpdateContainerTitleHeight(window);
            UpdateMarginPanel();
            UpdateTitleBarOffset(window);

            window.GetObservable(PleasantWindow.TitleBarHeightProperty)
                .Subscribe(new AnonymousObserver<double>(h =>
                {
                    titleBarHeight = h;
                    UpdateContainerTitleHeight(window);
                    UpdateMarginPanel();
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

        if (Items.Count > 0)
        {
            if (Items[0] is ISelectable selectableItem)
            {
                Debug.WriteLine($"[NavigationView] OnLoaded selecting first item header={(selectableItem as NavigationViewItem)?.Header}");
                SelectSingleItem(selectableItem, false);
            }
        }
    }

    /// <inheritdoc />
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        Debug.WriteLine($"[NavigationView] OnAttachedToLogicalTree itemCount={Items.Count}");

        if (Items.Count > 0 && Items[0] is ISelectable selectableItem)
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

        if (ButtonsPanelOffset)
        {
            // Hamburger is below the titlebar — remove the inbuilt 40px clearance so logo hugs left
            titleBar.LeftClearance = 0;
        }
        else
        {
            // Hamburger overlaps the titlebar — restore the inbuilt clearance so logo clears it
            titleBar.LeftClearance = 40;
        }
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
        if (SelectedItem != item && TransitionAnimation is not null && _contentPresenter is not null && runAnimation)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            TransitionAnimation.RunAsync(_contentPresenter, _cancellationTokenSource.Token);
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

    private void UpdateTitleAndSelectedContent()
    {
        if (SelectedItem is not NavigationViewItem item) return;
        Debug.WriteLine($"[NavigationView] UpdateTitleAndSelectedContent selectedItem header={item.Header} hasContent={item.Content is not null}");
        if (item.Content is not null)
            SelectedContent = item.Content;
    }

    private void OnSelectedItemChanged()
    {
        Debug.WriteLine($"[NavigationView] OnSelectedItemChanged → SelectedItem={(SelectedItem as NavigationViewItem)?.Header}");
        UpdateTitleAndSelectedContent();
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
}