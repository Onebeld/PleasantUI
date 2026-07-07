/*
 * SPDX-FileCopyrightText: 2026 Dmitry Zhutkov (Onebeld) <onebeld@gmail.com>
 * SPDX-FileCopyrightText: 2024 PieroCastillo <https://github.com/PieroCastillo>
 * SPDX-License-Identifier: MIT
 *
 * Modified from original source:
 * https://github.com/PieroCastillo/Aura.UI/blob/master/src/Aura.UI/Controls/Navigation/NavigationView/NavigationView.Properties.cs
 * https://github.com/PieroCastillo/Aura.UI/blob/master/src/Aura.UI/Controls/Navigation/NavigationView/NavigationView.cs
 */

using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Reactive;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PleasantUI.Controls.Chrome;
using PleasantUI.Core.Internal.Reactive;

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
public class NavigationView : TreeView, ICustomKeyboardNavigation
{
    private const double LittleWidth = 1005;
    private const double VeryLittleWidth = 650;
    private double _titleBarHeight;

    private Border? _container;
    private Grid? _mainGrid;
    private DockPanel? _dockPanel;
    private Border? _marginPanel;
    private StackPanel? _stackPanelButtons;
    private DockPanel? _topBottomLayout;
    private Border? _bottomBar;

    private Button? _backButton;

    private PleasantWindow? _window;

    private CancellationTokenSource? _cancellationTokenSource;
    private ContentPresenter? _contentPresenter;
    
    private CompositeDisposable? _windowDisposables;
    private readonly List<IDisposable> _proxySubscriptions = [];

    private Button? _headerItem;

    private readonly AvaloniaList<NavigationViewItem> _topItems = [];
    private readonly AvaloniaList<NavigationViewItem> _bottomItems = [];

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
        AvaloniaProperty.Register<NavigationView, bool>(nameof(ButtonsPanelOffset));

    /// <summary>
    /// Defines the <see cref="Position" /> property.
    /// </summary>
    public static readonly StyledProperty<NavigationViewPosition> PositionProperty =
        AvaloniaProperty.Register<NavigationView, NavigationViewPosition>(nameof(Position));
    
    public static readonly DirectProperty<NavigationView, AvaloniaList<NavigationViewItem>> TopItemsProperty =
        AvaloniaProperty.RegisterDirect<NavigationView, AvaloniaList<NavigationViewItem>>(
            nameof(TopItems), o => o.TopItems);

    /// <summary>Small icon buttons in the footer bar.</summary>
    public static readonly DirectProperty<NavigationView, AvaloniaList<NavigationViewItem>> BottomItemsProperty =
        AvaloniaProperty.RegisterDirect<NavigationView, AvaloniaList<NavigationViewItem>>(
            nameof(BottomItems), o => o.BottomItems);

    public AvaloniaList<NavigationViewItem> TopItems => _topItems;
    public AvaloniaList<NavigationViewItem> BottomItems => _bottomItems;

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
        get;
        private set => SetAndRaise(SelectedContentProperty, ref field, value);
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
        get;
        private set => SetAndRaise(ItemsAsStringsProperty, ref field, value);
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
        get;
        set => SetAndRaise(BackButtonCommandProperty, ref field, value);
    }

    static NavigationView()
    {
        SelectionModeProperty.OverrideDefaultValue<NavigationView>(SelectionMode.Single);
        SelectedItemProperty.Changed.AddClassHandler<NavigationView>((x, _) => x.OnSelectedItemChanged());
        IsOpenProperty.Changed.AddClassHandler<NavigationView>((x, e) => x.OnIsOpenChanged(e));
        ShowBackButtonProperty.Changed.AddClassHandler<NavigationView>((x, _) => x.UpdateMarginPanel());
        DisplayModeProperty.Changed.AddClassHandler<NavigationView>((x, _) => x.UpdateMarginPanel());
        ButtonsPanelOffsetProperty.Changed.AddClassHandler<NavigationView>((x, _) => x.OnButtonsPanelOffsetChanged());
        CompactPaneLengthProperty.Changed.AddClassHandler<NavigationView>((x, _) => x.OnCompactPaneLengthChanged());
        PositionProperty.Changed.AddClassHandler<NavigationView>((x, e) => x.OnPositionChanged(e));
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationView"/> class.
    /// </summary>
    public NavigationView()
    {
        PseudoClasses.Add(":normal");
        PseudoClasses.Add(":left");
        
        KeyboardNavigation.SetTabNavigation(this, KeyboardNavigationMode.Continue);
        
        _topItems.ForEachItem(item => item.NavigationView = this, item => item.NavigationView = null, () => { });
        _bottomItems.ForEachItem(item => item.NavigationView = this, item => item.NavigationView = null, () => { });
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _stackPanelButtons = e.NameScope.Find<StackPanel>("PART_StackPanelButtons");
        _headerItem = e.NameScope.Find<Button>("PART_HeaderItem");
        _backButton = e.NameScope.Find<Button>("PART_BackButton");
        _contentPresenter = e.NameScope.Find<ContentPresenter>("PART_SelectedContentPresenter");
        _container = e.NameScope.Find<Border>("PART_Container");
        _mainGrid = e.NameScope.Find<Grid>("PART_SplitViewGrid");
        _dockPanel = e.NameScope.Find<DockPanel>("PART_ItemsPresenterDockPanel");
        _marginPanel = e.NameScope.Find<Border>("PART_MarginPanel");
        _topBottomLayout = e.NameScope.Find<DockPanel>("PART_TopBottomLayout");
        _bottomBar = e.NameScope.Find<Border>("PART_BottomBar");

        // Explicitly sync layout panel visibility with current Position (survives style invalidation).
        UpdateLayoutVisibility(Position);

        if (_headerItem != null)
        {
            _headerItem.Click += (_, _) => IsOpen = AlwaysOpen || !IsOpen;
        }

        BackButtonCommandProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<ICommand?>>(x =>
        {
            _backButton?.IsVisible = x.NewValue.Value is not null;
        }));

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
            AvaloniaList<NavigationViewItem> firstItems = Position == NavigationViewPosition.Top ? _topItems : _bottomItems;
            if (firstItems.Count > 0)
                SelectTopBottomItem(firstItems[0]);
        }
    }
    
    (bool handled, IInputElement? next) ICustomKeyboardNavigation.GetNext(IInputElement element, NavigationDirection direction)
    {
        return (false, null);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        
        if (BindWindowSettings && TopLevel.GetTopLevel(this) is PleasantWindow window)
        {
            _window = window;
            _titleBarHeight = window.TitleBarHeight;
        
            _windowDisposables?.Dispose();
            _windowDisposables = new CompositeDisposable();
        
            UpdateMacNavigationLayout(window);
            UpdateContainerTitleHeight(window);
            UpdateMarginPanel();
        
            Dispatcher.UIThread.Post(() => UpdateTitleBarOffset(window), DispatcherPriority.Render);
        
            _windowDisposables.Add(window.GetObservable(PleasantWindow.TitleBarHeightProperty)
                .Subscribe(new AnonymousObserver<double>(h =>
                {
                    _titleBarHeight = h;
                    UpdateContainerTitleHeight(window);
                    UpdateMarginPanel();
                })));

            _windowDisposables.Add(window.GetObservable(PleasantWindow.TitleBarTypeProperty)
                .Subscribe(new AnonymousObserver<PleasantTitleBar.Type>(type =>
                {
                    ButtonsPanelOffset = type == PleasantTitleBar.Type.Compact;
                })));
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        
        if (_window != null)
        {
            ResetTitleBarOffset(_window);
        }
        
        _windowDisposables?.Dispose();
        _windowDisposables = null;
        _window = null;
        
        _cancellationTokenSource?.Cancel();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        
        if (e.NewSize.Width <= 0) return;

        OnBoundsChanged(new Rect(e.NewSize));
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
        if (item is not NavigationViewItem navItem) return;
        
        CloseAllSubMenuPopups();
        
        foreach (var currentItem in this.GetLogicalDescendants().OfType<NavigationViewItem>())
        {
            if (currentItem != navItem && currentItem.IsSelected)
                currentItem.IsSelected = false;
        }
        foreach (var currentItem in _topItems.Concat(_bottomItems))
        {
            if (currentItem != navItem && currentItem.IsSelected)
                currentItem.IsSelected = false;
        }

        navItem.IsSelected = true;
        
        SelectedItem = navItem;
    }
    
    private void OnButtonsPanelOffsetChanged()
    {
        UpdateMarginPanel();
        if (_window is not null) UpdateTitleBarOffset(_window);
    }

    private void OnCompactPaneLengthChanged()
    {
        if (_window is not null) UpdateTitleBarOffset(_window);
    }

    private void UpdateMacNavigationLayout(PleasantWindow window)
    {
        if (!window.EnableCustomTitleBar || !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return;

        window.GetObservable(Window.WindowStateProperty).Subscribe(new AnonymousObserver<WindowState>(state =>
        {
            if (state == WindowState.FullScreen)
            {
                if (_marginPanel != null)
                    Grid.SetRow(_marginPanel, 1);
                if (_dockPanel != null)
                    Grid.SetRow(_dockPanel, 2);
            }
            else if (_mainGrid != null && _marginPanel != null && _dockPanel != null && _headerItem != null)
            {
                if (_mainGrid.RowDefinitions.Count == 0 ||
                    Math.Abs(_mainGrid.RowDefinitions[0].Height.Value - _headerItem.Height) > 0.01)
                {
                    _mainGrid.RowDefinitions.Insert(0, new RowDefinition { Height = new GridLength(_headerItem.Height, GridUnitType.Pixel) });
                }
                Grid.SetRow(_marginPanel, 2);
                Grid.SetRow(_dockPanel, 3);
            }
        }));
    }

    private void UpdateContainerTitleHeight(PleasantWindow window)
    {
        if (_container == null) return;

        Thickness margin = window.EnableCustomTitleBar ? new Thickness(5, _titleBarHeight + 1, 5, 5) : new Thickness(0);

        _container.CornerRadius = new CornerRadius(8);
        _container.Margin = margin;
    }

    private void UpdateTitleBarOffset(PleasantWindow window)
    {
        if (!window.EnableCustomTitleBar) return;

        // Find the PleasantTitleBar in the window's template
        PleasantTitleBar? titleBar = window.GetTemplateDescendants().OfType<PleasantTitleBar>().FirstOrDefault();
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
            titleBar.LeftClearance = 45;
        }
    }
    
    private void ResetTitleBarOffset(PleasantWindow window)
    {
        if (!window.EnableCustomTitleBar) return;

        PleasantTitleBar? titleBar = window.GetTemplateDescendants().OfType<PleasantTitleBar>().FirstOrDefault();
        titleBar?.LeftClearance = 0;
    }

    private void UpdateLayoutVisibility(NavigationViewPosition position)
    {
        // Find the SplitView named "split" in the template
        SplitView? splitView = this.GetTemplateDescendants().OfType<SplitView>().FirstOrDefault(x => x.Name == "split");
        StackPanel? stackPanelButtons = _stackPanelButtons;

        bool isLeft = position == NavigationViewPosition.Left;
        bool isTop = position == NavigationViewPosition.Top;
        bool isBottom = position == NavigationViewPosition.Bottom;

        splitView?.IsVisible = isLeft;
        stackPanelButtons?.IsVisible = isLeft;
        _topBottomLayout?.IsVisible = isTop || isBottom;

        _bottomBar?.IsVisible = isBottom;

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
            result = _titleBarHeight + 5 + buttonsHeight + 5; // top margin + buttons + gap
        }
        else
        {
            // Original behavior: fixed heights from the AXAML template scaled to titleBarHeight.
            const double baselineTitleBarHeight = 44.0;
            double baseHeight = noBackButton ? 60.0 : 90.0;
            double delta = baseHeight - baselineTitleBarHeight;
            result = Math.Max(_titleBarHeight + delta, baseHeight);
        }

        _marginPanel.Height = result;

        Debug.WriteLine($"[NavigationView] UpdateMarginPanel titleBarHeight={_titleBarHeight} noBack={noBackButton} offset={ButtonsPanelOffset} → {result}");
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
                UpdatePseudoClasses(false);
                DisplayMode = SplitViewDisplayMode.CompactInline;
            }
            else if (isLittle && !isVeryLittle)
            {
                UpdatePseudoClasses(false);
                DisplayMode = SplitViewDisplayMode.CompactOverlay;
                IsOpen = false;
            }
            else if (isLittle && isVeryLittle)
            {
                UpdatePseudoClasses(true);
                DisplayMode = SplitViewDisplayMode.Overlay;
                IsOpen = false;
            }
        }
    }
    
    private void ChangeNavigationContent(object? newContent, bool playAnimation = true)
    {
        if (newContent is null) return;
        
        if (ReferenceEquals(SelectedContent, newContent))
            return;
        
        if (SelectedContent is ILogical oldLogical)
            LogicalChildren.Remove(oldLogical);

        if (newContent is ILogical newLogical)
        {
            if (!LogicalChildren.Contains(newLogical))
                LogicalChildren.Add(newLogical);
        }

        SelectedContent = newContent;

        if (!playAnimation || TransitionAnimation is null || _contentPresenter is null)
            return;
        
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        
        CancellationToken token = _cancellationTokenSource.Token;

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (token.IsCancellationRequested)
                return;
            
            try
            {
                await TransitionAnimation.RunAsync(_contentPresenter, token);
            }
            catch (TaskCanceledException)
            {
            }
        }, DispatcherPriority.Send);
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
        if (e.NewValue is not NavigationViewPosition position)
            return;
        
        PseudoClasses.Set(":left", position == NavigationViewPosition.Left);
        PseudoClasses.Set(":top", position == NavigationViewPosition.Top);
        PseudoClasses.Set(":bottom", position == NavigationViewPosition.Bottom);

        UpdateLayoutVisibility(position);
        UpdateMarginPanel();
    }
    
    private object? FindContentInHierarchy(NavigationViewItem? item)
    {
        while (item is not null)
        {
            if (item.Content is not null)
                return item.Content;

            item = item.GetLogicalParent() as NavigationViewItem 
                   ?? item.GetVisualParent() as NavigationViewItem;
        }

        return null;
    }

    private void UpdateTitleAndSelectedContent()
    {
        if (SelectedItem is not NavigationViewItem item) return;
        
        if (item.Content is null) return;
        
        if (SelectedContent is ILogical oldLogical)
            LogicalChildren.Remove(oldLogical);
        if (item.Content is ILogical newLogical && !LogicalChildren.Contains(newLogical))
            LogicalChildren.Add(newLogical);

        SelectedContent = item.Content;
    }

    private void OnSelectedItemChanged()
    {
        if (SelectedItem is NavigationViewItem item)
        {
            object? targetContent = FindContentInHierarchy(item);

            if (targetContent is not null)
                ChangeNavigationContent(targetContent);
        }
        else
        {
            if (SelectedItem is not null && SelectedItem != SelectedContent)
                ChangeNavigationContent(SelectedItem);
        }
    }
    
    private void OnIsOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        bool isNowOpen = (bool)(e.NewValue ?? false);
        Debug.WriteLine($"[NavigationView] OnIsOpenChanged isNowOpen={isNowOpen}");

        // Only group items (items that have NavigationViewItem children) participate in expand/collapse
        List<NavigationViewItem> groupItems = this.GetLogicalDescendants()
            .OfType<NavigationViewItem>()
            .Where(item => item.Items.OfType<NavigationViewItem>().Any())
            .ToList();

        if (!isNowOpen)
        {
            // Pane collapsing → save IsExpanded for each group item, then collapse it
            _expandedStates.Clear();
            foreach (NavigationViewItem item in groupItems)
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
            foreach (NavigationViewItem item in groupItems)
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

    private void CloseAllSubMenuPopups()
    {
        foreach (NavigationViewItem item in this.GetLogicalDescendants().OfType<NavigationViewItem>())
        {
            if (item.IsSubMenuOpen)
            {
                Debug.WriteLine($"[NavigationView] CloseAllSubMenuPopups closing popup on header={item.Header}");
                item.IsSubMenuOpen = false;
            }
        }
    }

    private void SelectTopBottomItem(NavigationViewItem item)
    {
        Debug.WriteLine($"[NavigationView] SelectTopBottomItem header={item.Header}");

        // Deselect all top+bottom items except the clicked one.
        foreach (NavigationViewItem? i in _topItems)
            i.IsSelected = ReferenceEquals(i, item);
        foreach (NavigationViewItem? i in _bottomItems)
            i.IsSelected = ReferenceEquals(i, item);

        if (!Equals(SelectedItem, item))
            SelectedItem = item;
        
        object? targetContent = FindContentInHierarchy(item);

        if (targetContent is not null)
            ChangeNavigationContent(item.Content);
        else
            Debug.WriteLine(
                $"[NavigationView] Top/Bottom item '{item.Header}' selected via pointer, keeping current content.");
    }
}
