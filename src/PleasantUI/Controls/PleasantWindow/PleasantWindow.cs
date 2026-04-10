using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Reactive;
using Avalonia.Styling;
using Avalonia.Threading;
using PleasantUI.Controls.Chrome;

namespace PleasantUI.Controls;

/// <summary>
/// Represents a custom window with enhanced features and styling capabilities.
/// </summary>
public class PleasantWindow : PleasantWindowBase
{
    private PleasantSplashScreen? _splashOverlay;
    private CancellationTokenSource? _splashCts;

    static PleasantWindow()
    {
        CornerRadiusProperty.Changed.AddClassHandler<PleasantWindow>((x, e) => x.OnCornerRadiusChanged(e));
    }

    private void OnCornerRadiusChanged(AvaloniaPropertyChangedEventArgs e)
    {
        try
        {
            // Apply VGUI corner radius override if VGUI theme is active
            if (IsVGUIActive())
            {
                System.Diagnostics.Debug.WriteLine("[PleasantWindow] VGUI theme active, setting CornerRadius to 0");
                SetValue(CornerRadiusProperty, new CornerRadius(0));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PleasantWindow] Error in OnCornerRadiusChanged: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if the VGUI theme is currently active.
    /// </summary>
    /// <returns>True if VGUI theme is active, false otherwise.</returns>
    protected static bool IsVGUIActive()
    {
        try
        {
            var isActive = PleasantUI.Core.PleasantSettings.Current?.Theme == "VGUI";
            System.Diagnostics.Debug.WriteLine($"[PleasantWindow] IsVGUIActive: {isActive}");
            return isActive;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PleasantWindow] Error checking VGUI status: {ex.Message}");
            return false;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        try
        {
            // Apply VGUI corner radius override when template is applied
            if (IsVGUIActive())
            {
                System.Diagnostics.Debug.WriteLine("[PleasantWindow] OnApplyTemplate: VGUI theme active, setting CornerRadius to 0");
                CornerRadius = new CornerRadius(0);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PleasantWindow] Error in OnApplyTemplate: {ex.Message}");
        }
    }

    /// <summary>
    /// Defines the <see cref="EnableCustomTitleBar"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> EnableCustomTitleBarProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(EnableCustomTitleBar), true);

    /// <summary>
    /// Defines the <see cref="OverrideMacOSCaption"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> OverrideMacOSCaptionProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(OverrideMacOSCaptionProperty), true);

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
    /// Defines the <see cref="SplashScreen"/> property.
    /// </summary>
    public static readonly StyledProperty<IPleasantSplashScreen?> SplashScreenProperty =
        AvaloniaProperty.Register<PleasantWindow, IPleasantSplashScreen?>(nameof(SplashScreen));

    /// <summary>
    /// Defines the <see cref="IsCloseButtonVisible"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsCloseButtonVisibleProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(IsCloseButtonVisible), true);

    /// <summary>
    /// Defines the <see cref="IsMinimizeButtonVisible"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMinimizeButtonVisibleProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(IsMinimizeButtonVisible), true);

    /// <summary>
    /// Defines the <see cref="IsRestoreButtonVisible"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsRestoreButtonVisibleProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(IsRestoreButtonVisible), true);

    /// <summary>
    /// Defines the <see cref="IsFullScreenButtonVisible"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsFullScreenButtonVisibleProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(IsFullScreenButtonVisible), false);

    /// <summary>
    /// Shows the self-created TitleBar and hides the system TitleBar.
    /// </summary>
    public bool EnableCustomTitleBar
    {
        get => GetValue(EnableCustomTitleBarProperty);
        set => SetValue(EnableCustomTitleBarProperty, value);
    }

    /// <summary>
    /// Overrides Native MacOS caption buttons
    /// </summary>
    public bool OverrideMacOSCaption
    {
        get => GetValue(OverrideMacOSCaptionProperty);
        set => SetValue(OverrideMacOSCaptionProperty, value);
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

    /// <summary>
    /// Gets or sets the splash screen shown during window startup.
    /// Set this before the window is shown. The splash screen is automatically
    /// removed after <see cref="IPleasantSplashScreen.RunTasks"/> completes and
    /// <see cref="IPleasantSplashScreen.MinimumShowTime"/> has elapsed.
    /// </summary>
    public IPleasantSplashScreen? SplashScreen
    {
        get => GetValue(SplashScreenProperty);
        set => SetValue(SplashScreenProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the close button is visible.
    /// </summary>
    public bool IsCloseButtonVisible
    {
        get => GetValue(IsCloseButtonVisibleProperty);
        set => SetValue(IsCloseButtonVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the minimize button is visible.
    /// </summary>
    public bool IsMinimizeButtonVisible
    {
        get => GetValue(IsMinimizeButtonVisibleProperty);
        set => SetValue(IsMinimizeButtonVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the restore/maximize button is visible.
    /// </summary>
    public bool IsRestoreButtonVisible
    {
        get => GetValue(IsRestoreButtonVisibleProperty);
        set => SetValue(IsRestoreButtonVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the full-screen toggle button is visible.
    /// </summary>
    public bool IsFullScreenButtonVisible
    {
        get => GetValue(IsFullScreenButtonVisibleProperty);
        set => SetValue(IsFullScreenButtonVisibleProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(PleasantWindow);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _splashLayer = e.NameScope.Find<Panel>("PART_SplashLayer");

        this.GetObservable(EnableCustomTitleBarProperty)
            .Subscribe(new AnonymousObserver<bool>(x => ChangeDecorations(x, WindowState)));

        this.GetObservable(WindowStateProperty)
            .Subscribe(new AnonymousObserver<WindowState>(x => ChangeDecorations(EnableCustomTitleBar, x)));
    }

    /// <inheritdoc />
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        if (SplashScreen is not null)
            _ = RunSplashScreenAsync(SplashScreen);
    }

    /// <inheritdoc />
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _splashCts?.Cancel();
        _splashCts?.Dispose();
        _splashCts = null;
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

    private Panel? _splashLayer;

    private async Task RunSplashScreenAsync(IPleasantSplashScreen splash)
    {
        _splashCts = new CancellationTokenSource();

        // Wait for template to be applied so PART_SplashLayer is available
        if (_splashLayer is null) return;

        _splashOverlay = new PleasantSplashScreen
        {
            SplashScreen             = splash,
            HorizontalAlignment      = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment        = Avalonia.Layout.VerticalAlignment.Stretch,
            IsHitTestVisible         = true
        };

        _splashLayer.Children.Add(_splashOverlay);
        _splashLayer.IsVisible = true;

        var startTime = Environment.TickCount64;

        try
        {
            await splash.RunTasks(_splashCts.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        // Honour minimum show time
        long elapsed   = Environment.TickCount64 - startTime;
        int  remaining = splash.MinimumShowTime - (int)elapsed;
        if (remaining > 0)
            await Task.Delay(remaining, _splashCts.Token).ContinueWith(_ => { });

        if (_splashCts.IsCancellationRequested) return;

        // Fade out on the UI thread
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (_splashOverlay is null) return;

            var fadeOut = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(350),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame { KeyTime = TimeSpan.Zero,                    Setters = { new Setter(OpacityProperty, 1.0) } },
                    new KeyFrame { KeyTime = TimeSpan.FromMilliseconds(350),   Setters = { new Setter(OpacityProperty, 0.0) } }
                }
            };

            await fadeOut.RunAsync(_splashOverlay);

            _splashLayer.Children.Clear();
            _splashLayer.IsVisible = false;
            _splashOverlay = null;
        });
    }

    private void ChangeDecorations(bool enableCustomTitleBar, WindowState windowState)
    {
        ExtendClientAreaToDecorationsHint = enableCustomTitleBar;
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || !EnableCustomTitleBar) return;

        if (windowState == WindowState.FullScreen)
        {
            // Restore system decorations when in fullscreen.
            WindowDecorations = WindowDecorations.Full;
        }
        else
        {
            if (OverrideMacOSCaption)
            {
                WindowDecorations = WindowDecorations.None;
            }
            else
            {
                WindowDecorations = TitleBarType == PleasantTitleBar.Type.Classic
                    ? WindowDecorations.Full
                    : WindowDecorations.None;
            }
        }
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