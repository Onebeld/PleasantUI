using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PleasantUI.Controls.Chrome;
using PleasantUI.Reactive;

namespace PleasantUI.Controls;

/// <summary>
/// Represents a custom window with enhanced features and styling capabilities.
/// </summary>
public class PleasantWindow : PleasantWindowBase
{
    /// <summary>
    /// Defines the <see cref="EnableCustomTitleBar"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> EnableCustomTitleBarProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(EnableCustomTitleBar), true);

    /// <summary>
    /// Defines the <see cref="TitleContent"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> TitleContentProperty =
        AvaloniaProperty.Register<PleasantWindow, object?>(nameof(TitleContent));

    /// <summary>
    /// Defines the <see cref="LeftTitleBarContent"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> LeftTitleBarContentProperty =
        AvaloniaProperty.Register<PleasantWindow, object?>(nameof(LeftTitleBarContent));

    /// <summary>
    /// Defines the <see cref="CaptionButtons"/> property.
    /// </summary>
    public static readonly StyledProperty<PleasantCaptionButtons.Type> CaptionButtonsProperty =
        AvaloniaProperty.Register<PleasantWindow, PleasantCaptionButtons.Type>(nameof(CaptionButtons));

    /// <summary>
    /// Defines the <see cref="Subtitle"/> property.
    /// </summary>
    public static readonly StyledProperty<string> SubtitleProperty =
        AvaloniaProperty.Register<PleasantWindow, string>(nameof(Subtitle));

    /// <summary>
    /// Defines the <see cref="DisplayIcon"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> DisplayIconProperty =
        AvaloniaProperty.Register<PleasantWindow, object?>(nameof(DisplayIcon));

    /// <summary>
    /// Defines the <see cref="DisplayTitle"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> DisplayTitleProperty =
        AvaloniaProperty.Register<PleasantWindow, object?>(nameof(DisplayTitle));

    /// <summary>
    /// Defines the <see cref="TitleBarType"/> property.
    /// </summary>
    public static readonly StyledProperty<PleasantTitleBar.Type> TitleBarTypeProperty =
        AvaloniaProperty.Register<PleasantWindow, PleasantTitleBar.Type>(nameof(TitleBarType));

    /// <summary>
    /// Defines the <see cref="EnableBlur"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> EnableBlurProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(EnableBlur));

    /// <summary>
    /// Defines the <see cref="TitleBarHeight"/> property.
    /// </summary>
    public static readonly StyledProperty<double> TitleBarHeightProperty =
        AvaloniaProperty.Register<PleasantWindow, double>(nameof(TitleBarHeight), 32);

    /// <summary>
    /// Defines the <see cref="ExtendsContentIntoTitleBar"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ExtendsContentIntoTitleBarProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(ExtendsContentIntoTitleBar));

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
    public object? TitleContent
    {
        get => GetValue(TitleContentProperty);
        set => SetValue(TitleContentProperty, value);
    }

    /// <summary>
    /// Indicates content to the left of the window title.
    /// </summary>
    public object? LeftTitleBarContent
    {
        get => GetValue(LeftTitleBarContentProperty);
        set => SetValue(LeftTitleBarContentProperty, value);
    }

    /// <summary>
    /// Specifies which buttons will be displayed in the window.
    /// </summary>
    public PleasantCaptionButtons.Type CaptionButtons
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
    /// Gets or sets the icon used to render the title bar icon.
    /// </summary>
    public object? DisplayIcon
    {
        get => GetValue(DisplayIconProperty);
        set => SetValue(DisplayIconProperty, value);
    }

    /// <summary>
    /// Gets or sets the geometry used to render the title bar background.
    /// </summary>
    public object? DisplayTitle
    {
        get => GetValue(DisplayTitleProperty);
        set => SetValue(DisplayTitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the type of the title bar.
    /// </summary>
    public PleasantTitleBar.Type TitleBarType
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
    /// Gets or sets the height of the title bar.
    /// </summary>
    /// <remarks>
    /// This property is read-only and is automatically set by the control.
    /// </remarks>
    public double TitleBarHeight
    {
        get => GetValue(TitleBarHeightProperty);
        private set => SetValue(TitleBarHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the window's content extends into the title bar.
    /// </summary>
    /// <remarks>
    /// This property is only relevant when <see cref="EnableCustomTitleBar"/> is true.
    /// </remarks>
    public bool ExtendsContentIntoTitleBar
    {
        get => GetValue(ExtendsContentIntoTitleBarProperty);
        set => SetValue(ExtendsContentIntoTitleBarProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(PleasantWindow);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        this.GetObservable(EnableCustomTitleBarProperty)
        .Subscribe(val =>
        {
            ExtendClientAreaToDecorationsHint = val;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && EnableCustomTitleBar == true)
            {
                ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.OSXThickTitleBar;
            }
        });
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == EnableCustomTitleBarProperty)
            ExtendClientAreaToDecorationsHint = EnableCustomTitleBar;

        if (change.Property == EnableBlurProperty)
            SetTransparencyLevelHint();
    }

    private void SetTransparencyLevelHint()
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