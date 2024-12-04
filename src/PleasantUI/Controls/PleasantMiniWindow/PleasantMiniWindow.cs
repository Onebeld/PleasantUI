using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using PleasantUI.Core;
using PleasantUI.Reactive;

namespace PleasantUI.Controls;

/// <summary>
/// Represents a custom window with pleasant features such as blur, custom title bar, and modal windows.
/// </summary>
public class PleasantMiniWindow : PleasantWindowBase
{
    private Button? _closeButton;
    private Panel? _dragWindowPanel;

    private Button? _hiddenButton;
    
    /// <summary>
    /// Defines the <see cref="EnableBlur"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> EnableBlurProperty =
        AvaloniaProperty.Register<PleasantMiniWindow, bool>(nameof(EnableBlur));

    /// <summary>
    /// Defines the <see cref="ShowPinButton"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowPinButtonProperty =
        AvaloniaProperty.Register<PleasantMiniWindow, bool>(nameof(ShowPinButton), true);

    /// <summary>
    /// Defines the <see cref="ShowHiddenButton"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowHiddenButtonProperty =
        AvaloniaProperty.Register<PleasantMiniWindow, bool>(nameof(ShowHiddenButton));

    /// <summary>
    /// Defines the <see cref="EnableCustomTitleBar"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> EnableCustomTitleBarProperty =
        AvaloniaProperty.Register<PleasantMiniWindow, bool>(nameof(EnableCustomTitleBar), true);

    /// <summary>
    /// Gets or sets a value indicating whether the blur effect is enabled.
    /// </summary>
    /// <value>
    /// <c>true</c> if the blur effect is enabled; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// The blur effect can be applied to enhance the visual aesthetics of an element.
    /// By default, the blur effect is disabled.
    /// </remarks>
    public bool EnableBlur
    {
        get => GetValue(EnableBlurProperty);
        set => SetValue(EnableBlurProperty, value);
    }

    /// <summary>
    /// Shows a spear that allows you to dock a window on top of all other windows on the desktop.
    /// </summary>
    public bool ShowPinButton
    {
        get => GetValue(ShowPinButtonProperty);
        set => SetValue(ShowPinButtonProperty, value);
    }

    /// <summary>
    /// Shows a spear that allows you to hide the window.
    /// </summary>
    public bool ShowHiddenButton
    {
        get => GetValue(ShowHiddenButtonProperty);
        set => SetValue(ShowHiddenButtonProperty, value);
    }

    /// <summary>
    /// Shows the self-created TitleBar and hides the system TitleBar.
    /// </summary>
    public bool EnableCustomTitleBar
    {
        get => GetValue(EnableCustomTitleBarProperty);
        set => SetValue(EnableCustomTitleBarProperty, value);
    }

    /// <inheritdoc cref="StyleKeyOverride" />
    protected override Type StyleKeyOverride => typeof(PleasantMiniWindow);
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PleasantMiniWindow" /> class.
    /// </summary>
    /// <remarks>
    /// This constructor sets up the <see cref="SnackbarQueueManager" /> property.
    /// </remarks>
    public PleasantMiniWindow()
    {
        SnackbarQueueManager = new SnackbarQueueManager<PleasantSnackbar>(this);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _closeButton = e.NameScope.Find<Button>("PART_CloseButton");
        _hiddenButton = e.NameScope.Find<Button>("PART_HiddenButton");
        _dragWindowPanel = e.NameScope.Find<Panel>("PART_DragWindow");

        if (_closeButton is not null)
            _closeButton.Click += (_, _) => Close();
        if (_hiddenButton is not null)
            _hiddenButton.Click += (_, _) => WindowState = WindowState.Minimized;

        ExtendClientAreaToDecorationsHint = PleasantSettings.Instance.WindowSettings.EnableCustomTitleBar;

        if (_dragWindowPanel is not null)
            _dragWindowPanel.PointerPressed += OnDragWindowBorderOnPointerPressed;

        this.GetObservable(EnableCustomTitleBarProperty)
            .Subscribe(val => { ExtendClientAreaToDecorationsHint = val; });
        this.GetObservable(CanResizeProperty).Subscribe(canResize =>
        {
            ExtendClientAreaTitleBarHeightHint = canResize ? 8 : 1;
        });
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property != EnableBlurProperty)
            return;
        
        if (EnableBlur)
            TransparencyLevelHint =
            [
                WindowTransparencyLevel.AcrylicBlur,
                WindowTransparencyLevel.Blur,
                WindowTransparencyLevel.None
            ];
        else
            TransparencyLevelHint =
            [
                WindowTransparencyLevel.None
            ];
    }

    private void OnDragWindowBorderOnPointerPressed(object? _, PointerPressedEventArgs args)
    {
        BeginMoveDrag(args);
    }
}