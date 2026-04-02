using System.ComponentModel;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using PleasantUI.Core.Helpers;

namespace PleasantUI.Controls;

/// <summary>
/// A panel that slides in from an edge of the screen, overlaying the main content.
/// Supports light-dismiss (click outside to close), optional title, close button,
/// and all four positions (Left, Right, Top, Bottom).
/// </summary>
[TemplatePart(PART_CloseButton,   typeof(Button))]
[TemplatePart(PART_Overlay,       typeof(Border))]
[TemplatePart(PART_DrawerPanel,   typeof(ShadowBorder))]
[PseudoClasses(PC_Left, PC_Right, PC_Top, PC_Bottom, PC_Open, PC_Closing)]
public class PleasantDrawer : PleasantPopupElement
{
    /// <summary>Template part name for the close button.</summary>
    public const string PART_CloseButton = "PART_CloseButton";
    /// <summary>Template part name for the overlay background border.</summary>
    public const string PART_Overlay     = "PART_Overlay";
    /// <summary>Template part name for the drawer panel.</summary>
    public const string PART_DrawerPanel = "PART_DrawerPanel";

    /// <summary>Pseudo-class applied when <see cref="Position"/> is <see cref="DrawerPosition.Left"/>.</summary>
    public const string PC_Left    = ":left";
    /// <summary>Pseudo-class applied when <see cref="Position"/> is <see cref="DrawerPosition.Right"/>.</summary>
    public const string PC_Right   = ":right";
    /// <summary>Pseudo-class applied when <see cref="Position"/> is <see cref="DrawerPosition.Top"/>.</summary>
    public const string PC_Top     = ":top";
    /// <summary>Pseudo-class applied when <see cref="Position"/> is <see cref="DrawerPosition.Bottom"/>.</summary>
    public const string PC_Bottom  = ":bottom";
    /// <summary>Pseudo-class applied while the drawer is open.</summary>
    public const string PC_Open    = ":open";
    /// <summary>Pseudo-class applied while the drawer is closing.</summary>
    public const string PC_Closing = ":closing";

    private Button?       _closeButton;
    private Border?       _overlay;
    private ShadowBorder? _drawerPanel;

    private bool _isClosing;
    private AvaloniaList<PleasantPopupElement>? _modalWindows;
    private IInputElement? _lastFocus;
    private object? _result;

    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<PleasantDrawer, string?>(nameof(Title));

    /// <summary>Defines the <see cref="Position"/> property.</summary>
    public static readonly StyledProperty<DrawerPosition> PositionProperty =
        AvaloniaProperty.Register<PleasantDrawer, DrawerPosition>(nameof(Position), DrawerPosition.Right);

    /// <summary>Defines the <see cref="CanLightDismiss"/> property.</summary>
    public static readonly StyledProperty<bool> CanLightDismissProperty =
        AvaloniaProperty.Register<PleasantDrawer, bool>(nameof(CanLightDismiss), defaultValue: true);

    /// <summary>Defines the <see cref="IsModal"/> property.</summary>
    public static readonly StyledProperty<bool> IsModalProperty =
        AvaloniaProperty.Register<PleasantDrawer, bool>(nameof(IsModal), defaultValue: false);

    /// <summary>Defines the <see cref="OpenAnimation"/> property.</summary>
    public static readonly StyledProperty<Animation?> OpenAnimationProperty =
        AvaloniaProperty.Register<PleasantDrawer, Animation?>(nameof(OpenAnimation));

    /// <summary>Defines the <see cref="CloseAnimation"/> property.</summary>
    public static readonly StyledProperty<Animation?> CloseAnimationProperty =
        AvaloniaProperty.Register<PleasantDrawer, Animation?>(nameof(CloseAnimation));

    /// <summary>Defines the <see cref="ShowOverlayAnimation"/> property.</summary>
    public static readonly StyledProperty<Animation?> ShowOverlayAnimationProperty =
        AvaloniaProperty.Register<PleasantDrawer, Animation?>(nameof(ShowOverlayAnimation));

    /// <summary>Defines the <see cref="HideOverlayAnimation"/> property.</summary>
    public static readonly StyledProperty<Animation?> HideOverlayAnimationProperty =
        AvaloniaProperty.Register<PleasantDrawer, Animation?>(nameof(HideOverlayAnimation));

    /// <summary>Defines the <see cref="FooterContent"/> property.</summary>
    public static readonly StyledProperty<object?> FooterContentProperty =
        AvaloniaProperty.Register<PleasantDrawer, object?>(nameof(FooterContent));

    // ── CLR accessors ─────────────────────────────────────────────────────────

    /// <summary>Gets or sets the title shown in the drawer header.</summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets or sets which edge the drawer slides in from.</summary>
    public DrawerPosition Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    /// <summary>Gets or sets whether clicking the overlay background closes the drawer.</summary>
    public bool CanLightDismiss
    {
        get => GetValue(CanLightDismissProperty);
        set => SetValue(CanLightDismissProperty, value);
    }

    /// <summary>Gets or sets whether the drawer blocks interaction with the rest of the UI.</summary>
    public bool IsModal
    {
        get => GetValue(IsModalProperty);
        set => SetValue(IsModalProperty, value);
    }

    /// <summary>Gets or sets the slide-in animation applied to the drawer panel.</summary>
    public Animation? OpenAnimation
    {
        get => GetValue(OpenAnimationProperty);
        set => SetValue(OpenAnimationProperty, value);
    }

    /// <summary>Gets or sets the slide-out animation applied to the drawer panel.</summary>
    public Animation? CloseAnimation
    {
        get => GetValue(CloseAnimationProperty);
        set => SetValue(CloseAnimationProperty, value);
    }

    /// <summary>Gets or sets the fade-in animation for the overlay background.</summary>
    public Animation? ShowOverlayAnimation
    {
        get => GetValue(ShowOverlayAnimationProperty);
        set => SetValue(ShowOverlayAnimationProperty, value);
    }

    /// <summary>Gets or sets the fade-out animation for the overlay background.</summary>
    public Animation? HideOverlayAnimation
    {
        get => GetValue(HideOverlayAnimationProperty);
        set => SetValue(HideOverlayAnimationProperty, value);
    }

    /// <summary>Gets or sets optional footer content (e.g. action buttons) shown at the bottom of the drawer.</summary>
    public object? FooterContent
    {
        get => GetValue(FooterContentProperty);
        set => SetValue(FooterContentProperty, value);
    }

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Raised when the drawer has fully closed.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClosedEvent =
        RoutedEvent.Register<PleasantDrawer, RoutedEventArgs>(nameof(Closed), RoutingStrategies.Direct);

    /// <summary>Raised when the drawer has fully opened.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> OpenedEvent =
        RoutedEvent.Register<PleasantDrawer, RoutedEventArgs>(nameof(Opened), RoutingStrategies.Direct);

    /// <summary>Raised when the drawer has fully closed.</summary>
    public event EventHandler<RoutedEventArgs>? Closed
    {
        add    => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    /// <summary>Raised when the drawer has fully opened.</summary>
    public event EventHandler<RoutedEventArgs>? Opened
    {
        add    => AddHandler(OpenedEvent, value);
        remove => RemoveHandler(OpenedEvent, value);
    }

    // ── Static constructor ────────────────────────────────────────────────────

    static PleasantDrawer()
    {
        PositionProperty.Changed.AddClassHandler<PleasantDrawer, DrawerPosition>(
            (d, e) => d.UpdatePositionPseudoClasses(e.NewValue.Value));
    }

    /// <summary>Initializes a new instance of <see cref="PleasantDrawer"/>.</summary>
    public PleasantDrawer()
    {
        // Apply pseudo-classes for the default position immediately so the
        // AXAML styles that drive HorizontalAlignment/VerticalAlignment are
        // active before the first layout pass.
        UpdatePositionPseudoClasses(Position);
    }

    // ── Show API ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Shows the drawer on the specified <see cref="TopLevel"/>.
    /// </summary>
    public Task ShowAsync(TopLevel topLevel)
        => ShowCoreAsync<object>(topLevel);

    /// <summary>
    /// Shows the drawer and returns a typed result when it closes.
    /// </summary>
    public Task<T?> ShowAsync<T>(TopLevel topLevel)
        => ShowCoreAsync<T>(topLevel);

    /// <summary>
    /// Closes the drawer, optionally passing a result value.
    /// </summary>
    public Task CloseAsync(object? result = null)
    {
        _result = result;
        return CloseDrawerAsync();
    }

    // ── Template ──────────────────────────────────────────────────────────────

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // Unsubscribe from previous template parts
        if (_closeButton is not null) _closeButton.Click -= OnCloseButtonClick;

        _closeButton = e.NameScope.Find<Button>(PART_CloseButton);
        _overlay     = e.NameScope.Find<Border>(PART_Overlay);
        _drawerPanel = e.NameScope.Find<ShadowBorder>(PART_DrawerPanel);

        if (_closeButton is not null) _closeButton.Click += OnCloseButtonClick;
        // Overlay pointer is handled via OnPointerPressed override — no subscription needed

        UpdatePositionPseudoClasses(Position);
        ApplyPositionAlignment(Position);
    }

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        PseudoClasses.Set(PC_Open, true);

        // Run overlay and panel animations in parallel
        if (_overlay is not null)
            ShowOverlayAnimation?.RunAsync(_overlay);

        if (_drawerPanel is not null)
            OpenAnimation?.RunAsync(_drawerPanel);

        RaiseEvent(new RoutedEventArgs(OpenedEvent));
    }

    /// <inheritdoc />
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        // Light-dismiss: if the click landed on the overlay (i.e. outside the
        // drawer panel) and CanLightDismiss is true, close the drawer.
        // We check by hit-testing whether the press was inside PART_DrawerPanel.
        if (!CanLightDismiss || _drawerPanel is null) return;

        var pos = e.GetPosition(_drawerPanel);
        var bounds = new Rect(_drawerPanel.Bounds.Size);
        if (!bounds.Contains(pos))
        {
            e.Handled = true;
            _ = CloseDrawerAsync();
        }
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PositionProperty)
        {
            UpdatePositionPseudoClasses(Position);
            ApplyPositionAlignment(Position);
        }
    }

    private void ApplyPositionAlignment(DrawerPosition position)
    {
        if (_drawerPanel is null) return;

        switch (position)
        {
            case DrawerPosition.Right:
                _drawerPanel.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                _drawerPanel.VerticalAlignment   = Avalonia.Layout.VerticalAlignment.Stretch;
                break;
            case DrawerPosition.Left:
                _drawerPanel.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                _drawerPanel.VerticalAlignment   = Avalonia.Layout.VerticalAlignment.Stretch;
                break;
            case DrawerPosition.Top:
                _drawerPanel.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                _drawerPanel.VerticalAlignment   = Avalonia.Layout.VerticalAlignment.Top;
                break;
            case DrawerPosition.Bottom:
                _drawerPanel.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                _drawerPanel.VerticalAlignment   = Avalonia.Layout.VerticalAlignment.Bottom;
                break;
        }
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private async Task<T?> ShowCoreAsync<T>(TopLevel topLevel)
    {
        _modalWindows = WindowHelper.GetModalWindows(topLevel);

        var tcs = new TaskCompletionSource<T?>();

        // The drawer must stretch to fill the ModalWindowHost (which is full-screen)
        // so its internal Panel covers the entire overlay and the ShadowBorder
        // can align itself to the correct edge.
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        VerticalAlignment   = Avalonia.Layout.VerticalAlignment.Stretch;

        Host ??= new ModalWindowHost();
        Host.Content = this;

        // Set the Host's alignment based on Position so the OverlayLayer positions correctly
        SetHostAlignmentFromPosition();

        base.ShowCoreForTopLevel(topLevel);

        _modalWindows?.Add(this);
        _lastFocus = topLevel.FocusManager?.GetFocusedElement();

        Closed += (_, _) =>
        {
            try   { tcs.TrySetResult(_result is T typed ? typed : default); }
            catch { tcs.TrySetResult(default); }
        };

        return await tcs.Task;
    }

    private void SetHostAlignmentFromPosition()
    {
        if (Host is null) return;

        switch (Position)
        {
            case DrawerPosition.Right:
                Host.HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                Host.VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
                break;
            case DrawerPosition.Left:
                Host.HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                Host.VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
                break;
            case DrawerPosition.Top:
                Host.HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                Host.VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Top;
                break;
            case DrawerPosition.Bottom:
                Host.HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                Host.VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Bottom;
                break;
        }
    }

    private async Task CloseDrawerAsync()
    {
        if (_isClosing) return;
        _isClosing = true;

        PseudoClasses.Set(PC_Closing, true);
        PseudoClasses.Set(PC_Open, false);

        IsHitTestVisible = false;

        // Run hide animations in parallel, wait for both
        var tasks = new List<Task>();

        if (_overlay is not null)
        {
            _overlay.IsHitTestVisible = false;
            if (HideOverlayAnimation is not null)
                tasks.Add(HideOverlayAnimation.RunAsync(_overlay));
        }

        if (_drawerPanel is not null && CloseAnimation is not null)
            tasks.Add(CloseAnimation.RunAsync(_drawerPanel));

        await Task.WhenAll(tasks);

        _modalWindows?.Remove(this);

        if (_lastFocus is not null)
        {
            _lastFocus.Focus();
            _lastFocus = null;
        }

        RaiseEvent(new RoutedEventArgs(ClosedEvent));

        base.DeleteCoreForTopLevel();
    }

    private void OnCloseButtonClick(object? sender, RoutedEventArgs e)
        => _ = CloseDrawerAsync();

    private void UpdatePositionPseudoClasses(DrawerPosition position)
    {
        PseudoClasses.Set(PC_Left,   position == DrawerPosition.Left);
        PseudoClasses.Set(PC_Right,  position == DrawerPosition.Right);
        PseudoClasses.Set(PC_Top,    position == DrawerPosition.Top);
        PseudoClasses.Set(PC_Bottom, position == DrawerPosition.Bottom);
    }
}
