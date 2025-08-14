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
using Avalonia.Metadata;
using Avalonia.Reactive;
using Avalonia.Threading;

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
        base.OnApplyTemplate(e);

        _stackPanelButtons = e.NameScope.Find<StackPanel>("PART_StackPanelButtons");
        _headerItem = e.NameScope.Find<Button>("PART_HeaderItem");
        _backButton = e.NameScope.Find<Button>("PART_BackButton");
        _contentPresenter = e.NameScope.Find<ContentPresenter>("PART_SelectedContentPresenter");

        _container = e.NameScope.Find<Border>("PART_Container");
        _mainGrid = e.NameScope.Find<Grid>("PART_SplitViewGrid");
        _dockPanel = e.NameScope.Find<DockPanel>("PART_ItemsPresenterDockPanel");
        _marginPanel = e.NameScope.Find<Border>("PART_MarginPanel");

        if (_headerItem != null)
        {
            _headerItem.Click += (_, _) => IsOpen = AlwaysOpen || !IsOpen;
        }

        BackButtonCommandProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<ICommand?>>(x =>
        {
            if (_backButton != null)
                _backButton.IsVisible = x.NewValue.Value is not null;
        }));

        if (VisualRoot is PleasantWindow window)
        {
            titleBarHeight = window.TitleBarHeight;

            UpdateMacNavigationLayout(window);
            UpdateContainerTitleHeight(window);
        }

        UpdateTitleAndSelectedContent();
    }
    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        // Hack. For some reason it does not highlight the first item in the list after running the program
        if (Items.Count > 0)
        {
            if (Items[0] is ISelectable selectableItem)
            {
                SelectSingleItem(selectableItem, false);
            }
        }
    }

    /// <inheritdoc />
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);

        if (Items.Count > 0 && Items[0] is ISelectable selectableItem)
        {
            SelectSingleItem(selectableItem);
        }
    }

    internal void SelectSingleItem(ISelectable item, bool runAnimation = true)
    {
        if (item.IsSelected)
            return;

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
        if (_container == null)
            return;

        // Determine margin based on custom title bar setting.
        Thickness margin = window.EnableCustomTitleBar ? new Thickness(8, titleBarHeight + 1, 8, 8) : new Thickness(0);

        _container.CornerRadius = new CornerRadius(8);
        
        _container.Margin = margin;
    }
    private void OnBoundsChanged(Rect rect)
    {
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
                foreach (NavigationViewItem navigationViewItem in this.GetLogicalDescendants()
                             .OfType<NavigationViewItem>()) navigationViewItem.IsExpanded = false;
            }
            else if (isLittle && isVeryLittle)
            {
                UpdatePseudoClasses(true);
                DisplayMode = SplitViewDisplayMode.Overlay;
                IsOpen = false;
                foreach (NavigationViewItem navigationViewItem in this.GetLogicalDescendants()
                             .OfType<NavigationViewItem>()) navigationViewItem.IsExpanded = false;
            }
        }
    }

    private void SelectSingleItemCore(object? item, bool runAnimation = true)
    {
        if (SelectedItem != item && TransitionAnimation is not null && _contentPresenter is not null && runAnimation)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            TransitionAnimation.RunAsync(_contentPresenter, _cancellationTokenSource.Token);
        }

        if (item is ISelectable selectableItem)
            selectableItem.IsSelected = true;

        SelectedItem = item;
    }

    private void UpdatePseudoClasses(bool isCompact)
    {
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
            
        if (item.Content is not null)
            SelectedContent = item.Content;
    }

    private void OnSelectedItemChanged()
    {
        UpdateTitleAndSelectedContent();
    }
}