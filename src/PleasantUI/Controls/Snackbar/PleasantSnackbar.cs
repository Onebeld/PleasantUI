using System.Windows.Input;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using PleasantUI.Core.Helpers;
using PleasantUI.Core.Interfaces;

namespace PleasantUI.Controls;

/// <summary>
/// A modern and customizable Snackbar control for displaying notifications.
/// </summary>
public class PleasantSnackbar : PleasantPopupElement
{
    private Button? _button;
    private Button? _closeButton;
    private ContentPresenter? _contentPresenter;
    private Grid? _grid;
    private Control? _snackbarBorder;

    private IDisposable? _closingTimer;
    private SnackbarCloseReason _closeReason = SnackbarCloseReason.Programmatic;

    private EventHandler<SnackbarClosingEventArgs>? _closingHandler;
    private EventHandler<SnackbarClosedEventArgs>? _closedHandler;

    private readonly SnackbarQueueManager<PleasantSnackbar>? _queueManager;

    // ── Avalonia properties ───────────────────────────────────────────────────

    public static readonly StyledProperty<Animation?> OpenAnimationProperty =
        AvaloniaProperty.Register<PleasantSnackbar, Animation?>(nameof(OpenAnimation));

    public static readonly StyledProperty<Animation?> CloseAnimationProperty =
        AvaloniaProperty.Register<PleasantSnackbar, Animation?>(nameof(CloseAnimation));

    public static readonly StyledProperty<Geometry?> IconProperty =
        AvaloniaProperty.Register<PleasantSnackbar, Geometry?>(nameof(Icon));

    public static readonly StyledProperty<NotificationType> NotificationTypeProperty =
        AvaloniaProperty.Register<NotificationCard, NotificationType>(nameof(NotificationType));

    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<Button, ICommand?>(nameof(Command), enableDataValidation: true);

    /// <summary>Optional bold title shown above the message.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<PleasantSnackbar, string?>(nameof(Title));

    /// <summary>Whether a close (×) button is shown.</summary>
    public static readonly StyledProperty<bool> IsClosableProperty =
        AvaloniaProperty.Register<PleasantSnackbar, bool>(nameof(IsClosable), false);

    /// <summary>Arbitrary action control placed in the snackbar.</summary>
    public static readonly StyledProperty<Control?> ActionButtonProperty =
        AvaloniaProperty.Register<PleasantSnackbar, Control?>(nameof(ActionButton));

    // ── CLR properties ────────────────────────────────────────────────────────

    public Animation? OpenAnimation  { get => GetValue(OpenAnimationProperty);  set => SetValue(OpenAnimationProperty, value); }
    public Animation? CloseAnimation { get => GetValue(CloseAnimationProperty); set => SetValue(CloseAnimationProperty, value); }
    public Geometry?  Icon           { get => GetValue(IconProperty);           set => SetValue(IconProperty, value); }
    public NotificationType NotificationType { get => GetValue(NotificationTypeProperty); set => SetValue(NotificationTypeProperty, value); }
    public ICommand?  Command        { get => GetValue(CommandProperty);        set => SetValue(CommandProperty, value); }
    public string?    Title          { get => GetValue(TitleProperty);          set => SetValue(TitleProperty, value); }
    public bool       IsClosable     { get => GetValue(IsClosableProperty);     set => SetValue(IsClosableProperty, value); }
    public Control?   ActionButton   { get => GetValue(ActionButtonProperty);   set => SetValue(ActionButtonProperty, value); }

    // ── Constructor ───────────────────────────────────────────────────────────

    public PleasantSnackbar(TopLevel? topLevel)
    {
        _queueManager = WindowHelper.GetSnackbarQueueManager(topLevel);
    }

    // ── Static Show overloads ─────────────────────────────────────────────────

    public static void Show(PleasantSnackbarOptions options)                          => ShowCoreForTopLevel(null, options);
    public static void Show(Window window, PleasantSnackbarOptions options)           => ShowCoreForTopLevel(window, options);
    public static void Show(IPleasantWindow pleasantWindow, PleasantSnackbarOptions options) => ShowCoreForTopLevel(pleasantWindow as TopLevel, options);
    public static void Show(TopLevel topLevel, PleasantSnackbarOptions options)       => ShowCoreForTopLevel(topLevel, options);

    // ── Internal host management ──────────────────────────────────────────────

    internal void CreateHost()
    {
        Host ??= new ModalWindowHost();
        Host.Content = this;
        ShowCoreForTopLevel(TopLevel);
    }

    internal void DeleteHost() => base.DeleteCoreForTopLevel();

    // ── Template ──────────────────────────────────────────────────────────────

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _snackbarBorder  = e.NameScope.Find<Control>("PART_Snackbar");
        _contentPresenter = e.NameScope.Find<ContentPresenter>("PART_ContentPresenter");
        _button          = e.NameScope.Find<Button>("PART_Button");
        _grid            = e.NameScope.Find<Grid>("PART_Grid");
        _closeButton     = e.NameScope.Find<Button>("PART_CloseButton");

        if (_snackbarBorder is null || _contentPresenter is null)
            throw new NullReferenceException("Snackbar border or content presenter not found");

        if (_closeButton is not null)
            _closeButton.Click += (_, _) => OnCloseButtonClicked();

        UpdateClosableState();
        UpdateTitleState();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == NotificationTypeProperty)
            UpdateNotificationType();
        else if (e.Property == IsClosableProperty)
            UpdateClosableState();
        else if (e.Property == TitleProperty)
            UpdateTitleState();
    }

    // ── Core show logic ───────────────────────────────────────────────────────

    private static void ShowCoreForTopLevel(TopLevel? parent, PleasantSnackbarOptions options)
    {
        if (options.TimeSpan == TimeSpan.Zero)
            options.TimeSpan = TimeSpan.FromSeconds(3);

        PleasantSnackbar snackbar = new(parent)
        {
            Content          = options.Message,
            Title            = options.Title,
            Icon             = options.Icon,
            NotificationType = options.NotificationType,
            IsClosable       = options.IsClosable,
            TopLevel         = parent,
            _closingHandler  = options.Closing,
            _closedHandler   = options.Closed
        };

        // Wire action button
        snackbar.TemplateApplied += (_, _) =>
        {
            if (options.ActionButton is not null)
                snackbar.DefineActionButton(options.ActionButton);
            else
                snackbar.DefineButton(options.ButtonText, options.ButtonAction);
        };

        snackbar.Loaded += async (_, _) =>
        {
            await snackbar.Open();

            snackbar._closingTimer = DispatcherTimer.RunOnce(() =>
            {
                snackbar._closeReason = SnackbarCloseReason.Timeout;
                _ = snackbar.Close();
            }, options.TimeSpan);

            snackbar.PointerPressed += (_, _) =>
            {
                snackbar._closingTimer?.Dispose();
                snackbar._closeReason = SnackbarCloseReason.UserDismiss;
                _ = snackbar.Close();
            };
        };

        snackbar._queueManager?.Enqueue(snackbar);

        if (snackbar._queueManager is null)
        {
            snackbar.CreateHost();
            return;
        }

        if (snackbar._queueManager.Count <= 1)
            snackbar.CreateHost();
    }

    // ── Close logic with Closing/Closed events ────────────────────────────────

    private async Task Close(Action? action = null)
    {
        // Fire Closing — allow cancellation
        if (_closingHandler is not null)
        {
            var args = new SnackbarClosingEventArgs(_closeReason);
            _closingHandler.Invoke(this, args);
            if (args.Cancel) return;
        }

        IsHitTestVisible = false;
        await RunCloseAnimation();
        action?.Invoke();

        _closedHandler?.Invoke(this, new SnackbarClosedEventArgs(_closeReason));

        _queueManager?.Dequeue();
    }

    private void OnCloseButtonClicked()
    {
        _closingTimer?.Dispose();
        _closeReason = SnackbarCloseReason.CloseButton;
        _ = Close();
    }

    // ── Open animation ────────────────────────────────────────────────────────

    private async Task Open()
    {
        double maxSnackbarWidth = _snackbarBorder!.Bounds.Width;
        _snackbarBorder.Width = MinWidth;

        await OpenAnimation?.RunAsync(this)!;

        Animation animation = new()
        {
            Easing    = new CubicEaseInOut(),
            Duration  = TimeSpan.FromSeconds(0.3),
            FillMode  = FillMode.Both,
            Children  =
            {
                new KeyFrame { KeyTime = TimeSpan.Zero,               Setters = { new Setter(WidthProperty, MinWidth) } },
                new KeyFrame { KeyTime = TimeSpan.FromSeconds(0.3),   Setters = { new Setter(WidthProperty, maxSnackbarWidth) } }
            }
        };

        await animation.RunAsync(_snackbarBorder);
        _snackbarBorder.Width = double.NaN;

        if (_contentPresenter != null)
            _contentPresenter.TextWrapping = TextWrapping.WrapWithOverflow;

        if (_grid != null)
            _grid.Opacity = 1;
    }

    private async Task RunCloseAnimation() => await CloseAnimation?.RunAsync(this)!;

    // ── Button helpers ────────────────────────────────────────────────────────

    private void DefineButton(string? content, Action? action)
    {
        if (_button is null) return;

        if (string.IsNullOrWhiteSpace(content) || action is null)
        {
            _button.IsVisible = false;
            return;
        }

        _button.Content = content;
        _button.Click += async (_, _) =>
        {
            _closingTimer?.Dispose();
            _closeReason = SnackbarCloseReason.UserDismiss;
            await Close(action);
        };
    }

    private void DefineActionButton(Control actionButton)
    {
        if (_button is null) return;
        _button.IsVisible = false;

        // Place the custom control in the ActionButton slot if the template supports it,
        // otherwise fall back to replacing the button content
        ActionButton = actionButton;
    }

    // ── Pseudo-class helpers ──────────────────────────────────────────────────

    private void UpdateNotificationType()
    {
        PseudoClasses.Set(":error",       NotificationType == NotificationType.Error);
        PseudoClasses.Set(":information", NotificationType == NotificationType.Information);
        PseudoClasses.Set(":success",     NotificationType == NotificationType.Success);
        PseudoClasses.Set(":warning",     NotificationType == NotificationType.Warning);
    }

    private void UpdateClosableState()
    {
        PseudoClasses.Set(":closable", IsClosable);
    }

    private void UpdateTitleState()
    {
        PseudoClasses.Set(":titled", !string.IsNullOrEmpty(Title));
    }
}
