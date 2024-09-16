using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using PleasantUI.Controls.Chrome;
using PleasantUI.Core.Enums;
using PleasantUI.Core.Interfaces;
using PleasantUI.Reactive;

namespace PleasantUI.Controls;

/// <summary>
/// Represents a custom window with enhanced features and styling capabilities.
/// </summary>
public class PleasantWindow : Window, IPleasantWindow
{
    private Panel _modalWindowsPanel = null!;
    
    /// <summary>
    /// Defines the <see cref="EnableCustomTitleBar"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> EnableCustomTitleBarProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(EnableCustomTitleBar), true);

    /// <summary>
    /// Defines the <see cref="TitleContent"/> property.
    /// </summary>
    public static readonly StyledProperty<Control?> TitleContentProperty =
        AvaloniaProperty.Register<PleasantWindow, Control?>(nameof(TitleContent));

    /// <summary>
    /// Defines the <see cref="LeftTitleContent"/> property.
    /// </summary>
    public static readonly StyledProperty<Control?> LeftTitleContentProperty =
        AvaloniaProperty.Register<PleasantWindow, Control?>(nameof(LeftTitleContent));

    /// <summary>
    /// Defines the <see cref="EnableTitleBarMargin"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> EnableTitleBarMarginProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(EnableTitleBarMargin), true);

    /// <summary>
    /// Defines the <see cref="TitleBarStyle"/> property.
    /// </summary>
    public static readonly StyledProperty<PleasantTitleBarStyle> TitleBarStyleProperty =
        AvaloniaProperty.Register<PleasantWindow, PleasantTitleBarStyle>(nameof(TitleBarStyle));

    /// <summary>
    /// Defines the <see cref="CaptionButtons"/> property.
    /// </summary>
    public static readonly StyledProperty<PleasantCaptionButtonsType> CaptionButtonsProperty =
        AvaloniaProperty.Register<PleasantWindow, PleasantCaptionButtonsType>(nameof(CaptionButtons));

    /// <summary>
    /// Defines the <see cref="Subtitle"/> property.
    /// </summary>
    public static readonly StyledProperty<string> SubtitleProperty =
        AvaloniaProperty.Register<PleasantWindow, string>(nameof(Subtitle));

    /// <summary>
    /// Defines the <see cref="IconImage"/> property.
    /// </summary>
    public static readonly StyledProperty<IImage?> IconImageProperty =
        AvaloniaProperty.Register<PleasantWindow, IImage?>(nameof(IconImage));

    /// <summary>
    /// Defines the <see cref="TitleGeometry"/> property.
    /// </summary>
    public static readonly StyledProperty<Geometry?> TitleGeometryProperty =
        AvaloniaProperty.Register<PleasantWindow, Geometry?>(nameof(TitleGeometry));

    /// <summary>
    /// Defines the <see cref="TitleBarType"/> property.
    /// </summary>
    public static readonly StyledProperty<PleasantTitleBarType> TitleBarTypeProperty =
        AvaloniaProperty.Register<PleasantWindow, PleasantTitleBarType>(nameof(TitleBarType));

    /// <summary>
    /// Defines the <see cref="EnableBlur"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> EnableBlurProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(EnableBlur));

    /// <summary>
    /// Defines the <see cref="ShowTitleBarContentAnyway"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowTitleBarContentAnywayProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(ShowTitleBarContentAnyway));

    /// <summary>
    /// Shows the self-created TitleBar and hides the system TitleBar.
    /// </summary>
    public bool EnableCustomTitleBar
    {
        get => GetValue(EnableCustomTitleBarProperty);
        set => SetValue(EnableCustomTitleBarProperty, value);
    }

    /// <summary>
    /// Gets or sets the content to display in the center of the title bar.
    /// </summary>
    public Control? TitleContent
    {
        get => GetValue(TitleContentProperty);
        set => SetValue(TitleContentProperty, value);
    }

    /// <summary>
    /// Indicates content to the left of the window title.
    /// </summary>
    /// <remarks>
    /// Will not display when <see cref="TitleBarStyle" /> equal MacOS.
    /// </remarks>
    public Control? LeftTitleContent
    {
        get => GetValue(LeftTitleContentProperty);
        set => SetValue(LeftTitleContentProperty, value);
    }

    /// <summary>
    /// Specifies whether TitleBar can indent the main window content. If False, TitleBar overlaps the window content.
    /// </summary>
    public bool EnableTitleBarMargin
    {
        get => GetValue(EnableTitleBarMarginProperty);
        set => SetValue(EnableTitleBarMarginProperty, value);
    }

    /// <summary>
    /// Gets or sets the style of the title bar.
    /// </summary>
    public PleasantTitleBarStyle TitleBarStyle
    {
        get => GetValue(TitleBarStyleProperty);
        set => SetValue(TitleBarStyleProperty, value);
    }

    /// <summary>
    /// Specifies which buttons will be displayed in the window.
    /// </summary>
    public PleasantCaptionButtonsType CaptionButtons
    {
        get => GetValue(CaptionButtonsProperty);
        set => SetValue(CaptionButtonsProperty, value);
    }

    /// <summary>
    /// Gets or sets the subtitle of the window.
    /// </summary>
    public string Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon image for the window.
    /// </summary>
    public IImage? IconImage
    {
        get => GetValue(IconImageProperty);
        set => SetValue(IconImageProperty, value);
    }

    /// <summary>
    /// Gets or sets the geometry used to render the title bar background.
    /// </summary>
    public Geometry? TitleGeometry
    {
        get => GetValue(TitleGeometryProperty);
        set => SetValue(TitleGeometryProperty, value);
    }

    /// <summary>
    /// Gets or sets the type of the title bar.
    /// </summary>
    public PleasantTitleBarType TitleBarType
    {
        get => GetValue(TitleBarTypeProperty);
        set => SetValue(TitleBarTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether background blur is enabled for the window.
    /// </summary>
    public bool EnableBlur
    {
        get => GetValue(EnableBlurProperty);
        set => SetValue(EnableBlurProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether title bar content should be shown even when the window is maximized.
    /// </summary>
    public bool ShowTitleBarContentAnyway
    {
        get => GetValue(ShowTitleBarContentAnywayProperty);
        set => SetValue(ShowTitleBarContentAnywayProperty, value);
    }

    /// <inheritdoc />
    public SnackbarQueueManager<PleasantSnackbar> SnackbarQueueManager { get; }
    
    /// <inheritdoc />
    public AvaloniaList<PleasantModalWindow> ModalWindows { get; } = [];
    
    /// <inheritdoc />
    public AvaloniaList<Control> Controls { get; } = [];

    /// <inheritdoc />
    public VisualLayerManager VisualLayerManager { get; private set; } = null!;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PleasantWindow"/> class.
    /// </summary>
    public PleasantWindow()
    {
        SnackbarQueueManager = new SnackbarQueueManager<PleasantSnackbar>(this);
    }

    /// <inheritdoc />
    public void AddModalWindow(PleasantModalWindow modalWindow)
    {
        Panel windowPanel = new()
        {
            IsHitTestVisible = modalWindow.IsHitTestVisible
        };
        windowPanel.Children.Add(modalWindow.ModalBackground);
        windowPanel.Children.Add(modalWindow);

        ModalWindows.Add(modalWindow);

        _modalWindowsPanel.Children.Add(windowPanel);
    }

    /// <inheritdoc />
    public void RemoveModalWindow(PleasantModalWindow modalWindow)
    {
        ModalWindows.Remove(modalWindow);
        _modalWindowsPanel.Children.Remove(modalWindow.Parent as Panel ?? throw new NullReferenceException());
    }

    /// <inheritdoc />
    public void AddControl(Control control)
    {
        Controls.Add(control);
        _modalWindowsPanel.Children.Add(control);
    }

    /// <inheritdoc />
    public void RemoveControl(Control control)
    {
        Controls.Remove(control);
        _modalWindowsPanel.Children.Remove(control);
    }
    
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(PleasantWindow);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _modalWindowsPanel = e.NameScope.Get<Panel>("PART_ModalWindowsPanel");

        e.NameScope.Get<PleasantTitleBar>("PART_PleasantTitleBar");
        VisualLayerManager = e.NameScope.Get<VisualLayerManager>("PART_VisualLayerManager");

        this.GetObservable(EnableCustomTitleBarProperty)
            .Subscribe(val => { ExtendClientAreaToDecorationsHint = val; });
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == EnableCustomTitleBarProperty) ExtendClientAreaToDecorationsHint = EnableCustomTitleBar;

        if (change.Property == EnableTitleBarMarginProperty)
            if (TitleBarStyle == PleasantTitleBarStyle.MacOs)
                EnableTitleBarMargin = true;

        if (change.Property == EnableBlurProperty)
        {
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
    }
}