using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace PleasantUI.Controls;

/// <summary>
/// A generic borderless, transparent, topmost popup window for system-tray panels.
/// Provides an optional structured layout with header, status row, content area,
/// and footer — each independently hideable. Set <see cref="UseStructuredLayout"/>
/// to <c>false</c> to use fully custom <see cref="ContentControl.Content"/> instead.
/// </summary>
public class PleasantTrayPopup : Window
{
    // ── Dismiss ──────────────────────────────────────────────────────────────

    /// <summary>Raised when the popup is dismissed (hidden or closed).</summary>
    public event EventHandler? Dismissed;

    // ── Styled properties ─────────────────────────────────────────────────────

    public static readonly StyledProperty<CornerRadius> PopupCornerRadiusProperty =
        AvaloniaProperty.Register<PleasantTrayPopup, CornerRadius>(nameof(PopupCornerRadius), new CornerRadius(12));

    public static readonly StyledProperty<IBrush?> PopupBackgroundProperty =
        AvaloniaProperty.Register<PleasantTrayPopup, IBrush?>(nameof(PopupBackground));

    public static readonly StyledProperty<IBrush?> PopupBorderBrushProperty =
        AvaloniaProperty.Register<PleasantTrayPopup, IBrush?>(nameof(PopupBorderBrush));

    public static readonly StyledProperty<Thickness> PopupBorderThicknessProperty =
        AvaloniaProperty.Register<PleasantTrayPopup, Thickness>(nameof(PopupBorderThickness), new Thickness(1));

    public static readonly StyledProperty<bool> AutoDismissOnDeactivateProperty =
        AvaloniaProperty.Register<PleasantTrayPopup, bool>(nameof(AutoDismissOnDeactivate), true);

    public static readonly StyledProperty<double> PopupMarginProperty =
        AvaloniaProperty.Register<PleasantTrayPopup, double>(nameof(PopupMargin), 10.0);

    // ── Structured layout toggle ──────────────────────────────────────────────

    /// <summary>
    /// When <c>true</c> (default), the template renders the structured header/status/content/footer layout.
    /// Set to <c>false</c> to use fully custom <see cref="ContentControl.Content"/>.
    /// </summary>
    public static readonly StyledProperty<bool> UseStructuredLayoutProperty =
        AvaloniaProperty.Register<PleasantTrayPopup, bool>(nameof(UseStructuredLayout), true);

    // ── Header ────────────────────────────────────────────────────────────────

    /// <summary>Icon displayed in the header (e.g. a PathIcon or Image).</summary>
    public static readonly StyledProperty<object?> AppIconProperty =
        AvaloniaProperty.Register<PleasantTrayPopup, object?>(nameof(AppIcon));

    /// <summary>Application name shown in the header.</summary>
    public static readonly StyledProperty<string?> AppTitleProperty =
        AvaloniaProperty.Register<PleasantTrayPopup, string?>(nameof(AppTitle));

    /// <summary>Status indicator color (e.g. green = connected, red = disconnected).</summary>
    public static readonly StyledProperty<IBrush?> StatusColorProperty =
        AvaloniaProperty.Register<PleasantTrayPopup, IBrush?>(nameof(StatusColor));

    /// <summary>Status text shown next to the status dot.</summary>
    public static readonly StyledProperty<string?> StatusTextProperty =
        AvaloniaProperty.Register<PleasantTrayPopup, string?>(nameof(StatusText));

    /// <summary>Whether the header section is visible.</summary>
    public static readonly StyledProperty<bool> ShowHeaderProperty =
        AvaloniaProperty.Register<PleasantTrayPopup, bool>(nameof(ShowHeader), true);

    /// <summary>Whether the close button in the header is visible.</summary>
    public static readonly StyledProperty<bool> ShowCloseButtonProperty =
        AvaloniaProperty.Register<PleasantTrayPopup, bool>(nameof(ShowCloseButton), true);

    // ── Status row ────────────────────────────────────────────────────────────

    /// <summary>
    /// Optional key/value pairs shown in the status info row below the header.
    /// Each item is a (label, value) tuple.
    /// </summary>
    public static readonly StyledProperty<IEnumerable<(string Label, string Value)>?> StatusItemsProperty =
        AvaloniaProperty.Register<PleasantTrayPopup, IEnumerable<(string Label, string Value)>?>(nameof(StatusItems));

    /// <summary>Whether the status info row is visible.</summary>
    public static readonly StyledProperty<bool> ShowStatusRowProperty =
        AvaloniaProperty.Register<PleasantTrayPopup, bool>(nameof(ShowStatusRow), true);

    // ── Footer ────────────────────────────────────────────────────────────────

    /// <summary>Content placed in the footer area (e.g. action buttons).</summary>
    public static readonly StyledProperty<object?> FooterContentProperty =
        AvaloniaProperty.Register<PleasantTrayPopup, object?>(nameof(FooterContent));

    /// <summary>Whether the footer section is visible.</summary>
    public static readonly StyledProperty<bool> ShowFooterProperty =
        AvaloniaProperty.Register<PleasantTrayPopup, bool>(nameof(ShowFooter), true);

    // ── Properties ────────────────────────────────────────────────────────────

    public CornerRadius PopupCornerRadius
    {
        get => GetValue(PopupCornerRadiusProperty);
        set => SetValue(PopupCornerRadiusProperty, value);
    }

    public IBrush? PopupBackground
    {
        get => GetValue(PopupBackgroundProperty);
        set => SetValue(PopupBackgroundProperty, value);
    }

    public IBrush? PopupBorderBrush
    {
        get => GetValue(PopupBorderBrushProperty);
        set => SetValue(PopupBorderBrushProperty, value);
    }

    public Thickness PopupBorderThickness
    {
        get => GetValue(PopupBorderThicknessProperty);
        set => SetValue(PopupBorderThicknessProperty, value);
    }

    public bool AutoDismissOnDeactivate
    {
        get => GetValue(AutoDismissOnDeactivateProperty);
        set => SetValue(AutoDismissOnDeactivateProperty, value);
    }

    public double PopupMargin
    {
        get => GetValue(PopupMarginProperty);
        set => SetValue(PopupMarginProperty, value);
    }

    public bool UseStructuredLayout
    {
        get => GetValue(UseStructuredLayoutProperty);
        set => SetValue(UseStructuredLayoutProperty, value);
    }

    public object? AppIcon
    {
        get => GetValue(AppIconProperty);
        set => SetValue(AppIconProperty, value);
    }

    public string? AppTitle
    {
        get => GetValue(AppTitleProperty);
        set => SetValue(AppTitleProperty, value);
    }

    public IBrush? StatusColor
    {
        get => GetValue(StatusColorProperty);
        set => SetValue(StatusColorProperty, value);
    }

    public string? StatusText
    {
        get => GetValue(StatusTextProperty);
        set => SetValue(StatusTextProperty, value);
    }

    public bool ShowHeader
    {
        get => GetValue(ShowHeaderProperty);
        set => SetValue(ShowHeaderProperty, value);
    }

    public bool ShowCloseButton
    {
        get => GetValue(ShowCloseButtonProperty);
        set => SetValue(ShowCloseButtonProperty, value);
    }

    public IEnumerable<(string Label, string Value)>? StatusItems
    {
        get => GetValue(StatusItemsProperty);
        set => SetValue(StatusItemsProperty, value);
    }

    public bool ShowStatusRow
    {
        get => GetValue(ShowStatusRowProperty);
        set => SetValue(ShowStatusRowProperty, value);
    }

    public object? FooterContent
    {
        get => GetValue(FooterContentProperty);
        set => SetValue(FooterContentProperty, value);
    }

    public bool ShowFooter
    {
        get => GetValue(ShowFooterProperty);
        set => SetValue(ShowFooterProperty, value);
    }

    // ── Template parts ────────────────────────────────────────────────────────

    private Button? _closeButton;

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(PleasantTrayPopup);

    // ── Constructor ───────────────────────────────────────────────────────────

    public PleasantTrayPopup()
    {
        ShowInTaskbar                     = false;
        WindowDecorations                 = WindowDecorations.None;
        ExtendClientAreaToDecorationsHint = true;
        Topmost                           = true;
        CanResize                         = false;
        Background                        = Brushes.Transparent;
        TransparencyLevelHint             =
        [
            WindowTransparencyLevel.Transparent,
            WindowTransparencyLevel.None,
        ];
        SizeToContent = SizeToContent.Height;

        Deactivated += OnDeactivated;
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_closeButton is not null)
            _closeButton.Click -= OnCloseClicked;

        _closeButton = e.NameScope.Find<Button>("PART_CloseButton");

        if (_closeButton is not null)
            _closeButton.Click += OnCloseClicked;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Positions the popup near the system tray (bottom-right of the work area) and shows it.
    /// </summary>
    public void ShowNearTray()
    {
        Measure(new Size(Width > 0 ? Width : 300, double.PositiveInfinity));

        var screen = Screens.Primary;
        if (screen is null) { Show(); Activate(); return; }

        var    workArea = screen.WorkingArea;
        double scale    = screen.Scaling;
        double margin   = PopupMargin;
        double popupW   = Width > 0 ? Width : 300;
        double popupH   = DesiredSize.Height > 0 ? DesiredSize.Height : 480;

        double logicalX = workArea.Right  / scale - popupW - margin;
        double logicalY = workArea.Bottom / scale - popupH - margin;

        Position = new PixelPoint((int)(logicalX * scale), (int)(logicalY * scale));

        Show();
        Activate();
    }

    /// <summary>Hides the popup and raises <see cref="Dismissed"/>.</summary>
    public void Dismiss()
    {
        Hide();
        Dismissed?.Invoke(this, EventArgs.Empty);
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void OnCloseClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => Dismiss();

    private void OnDeactivated(object? sender, EventArgs e)
    {
        if (AutoDismissOnDeactivate)
            Dismiss();
    }
}
