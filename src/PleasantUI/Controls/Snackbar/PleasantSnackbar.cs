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
    private ContentPresenter? _contentPresenter;
    private Grid? _grid;
    private Border? _snackbarBorder;

    private IDisposable? _closingTimer;

    private readonly SnackbarQueueManager<PleasantSnackbar>? _queueManager;

    /// <summary>
    /// Defines the <see cref="OpenAnimation" /> property.
    /// </summary>
    public static readonly StyledProperty<Animation?> OpenAnimationProperty =
        AvaloniaProperty.Register<PleasantSnackbar, Animation?>(nameof(OpenAnimation));

    /// <summary>
    /// Defines the <see cref="CloseAnimation" /> property.
    /// </summary>
    public static readonly StyledProperty<Animation?> CloseAnimationProperty =
        AvaloniaProperty.Register<PleasantSnackbar, Animation?>(nameof(CloseAnimation));

    /// <summary>
    /// Defines the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<Geometry?> IconProperty =
        AvaloniaProperty.Register<PleasantSnackbar, Geometry?>(nameof(Icon));

    /// <summary>
    /// Defines the <see cref="NotificationType" /> property.
    /// </summary>
    public static readonly StyledProperty<NotificationType> NotificationTypeProperty =
        AvaloniaProperty.Register<NotificationCard, NotificationType>(nameof(NotificationType));

    /// <summary>
    /// Defines the <see cref="Command" /> property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<Button, ICommand?>(nameof(Command), enableDataValidation: true);

    /// <summary>
    /// Gets or sets the animation to use when the Snackbar opens.
    /// </summary>
    public Animation? OpenAnimation
    {
        get => GetValue(OpenAnimationProperty);
        set => SetValue(OpenAnimationProperty, value);
    }

    /// <summary>
    /// Gets or sets the animation to use when the Snackbar closes.
    /// </summary>
    public Animation? CloseAnimation
    {
        get => GetValue(CloseAnimationProperty);
        set => SetValue(CloseAnimationProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon to display in the Snackbar.
    /// </summary>
    public Geometry? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets the type of notification to display.
    /// </summary>
    public NotificationType NotificationType
    {
        get => GetValue(NotificationTypeProperty);
        set => SetValue(NotificationTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets an <see cref="ICommand" /> to be invoked when the button is clicked.
    /// </summary>
    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PleasantSnackbar"/> class.
    /// Retrieves the Snackbar queue manager from the application helper.
    /// </summary>
    public PleasantSnackbar(TopLevel? topLevel)
    {
        _queueManager = ApplicationHelper.GetSnackbarQueueManager(topLevel);
    }

    /// <summary>
    /// Shows the Snackbar with the given options.
    /// </summary>
    /// <param name="options">The options to use when showing the Snackbar.</param>
    public static void Show(PleasantSnackbarOptions options) => ShowCoreForTopLevel(null, options);

    /// <summary>
    /// Shows the Snackbar with the given options on the specified window.
    /// </summary>
    /// <param name="window">The window to show the Snackbar on.</param>
    /// <param name="options">The options to use when showing the Snackbar.</param>
    public static void Show(Window window, PleasantSnackbarOptions options) => ShowCoreForTopLevel(window, options);

    /// <summary>
    /// Shows the Snackbar with the given options on the specified window.
    /// </summary>
    /// <param name="pleasantWindow">The window to show the Snackbar on.</param>
    /// <param name="options">The options to use when showing the Snackbar.</param>
    public static void Show(IPleasantWindow pleasantWindow, PleasantSnackbarOptions options) => ShowCoreForTopLevel(pleasantWindow as TopLevel, options);

    /// <summary>
    /// Shows the Snackbar with the given options on the specified window.
    /// </summary>
    /// <param name="topLevel">The window to show the Snackbar on.</param>
    /// <param name="options">The options to use when showing the Snackbar.</param>
    public static void Show(TopLevel topLevel, PleasantSnackbarOptions options) => ShowCoreForTopLevel(topLevel, options);

    internal void CreateHost()
    {
        Host ??= new ModalWindowHost();
        Host.Content = this;

        ShowCoreForTopLevel(TopLevel);
    }

    internal void DeleteHost()
    {
        base.DeleteCoreForTopLevel();
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _snackbarBorder = e.NameScope.Find<Border>("PART_Snackbar");
        _contentPresenter = e.NameScope.Find<ContentPresenter>("PART_ContentPresenter");
        _button = e.NameScope.Find<Button>("PART_Button");
        _grid = e.NameScope.Find<Grid>("PART_Grid");

        if (_snackbarBorder is null || _contentPresenter is null)
            throw new NullReferenceException("Snackbar border or content presenter not found");
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == NotificationTypeProperty)
            UpdateNotificationType();
    }

    private static void ShowCoreForTopLevel(TopLevel? parent, PleasantSnackbarOptions options)
    {
        if (options.TimeSpan == TimeSpan.Zero)
            options.TimeSpan = TimeSpan.FromSeconds(3);

        PleasantSnackbar pleasantSnackbar = new(parent)
        {
            Content = options.Message,
            Icon = options.Icon,
            NotificationType = options.NotificationType,

            TopLevel = parent
        };

        pleasantSnackbar.TemplateApplied += (_, _) =>
        {
            pleasantSnackbar.DefineButton(options.ButtonText, options.ButtonAction);
        };

        pleasantSnackbar.Loaded += async (_, _) =>
        {
            await pleasantSnackbar.Open();

            pleasantSnackbar._closingTimer = DispatcherTimer.RunOnce(() => _ = pleasantSnackbar.Close(), options.TimeSpan);

            pleasantSnackbar.PointerPressed += (_, _) =>
            {
                pleasantSnackbar._closingTimer.Dispose();

                _ = pleasantSnackbar.Close();
            };
        };

        pleasantSnackbar._queueManager?.Enqueue(pleasantSnackbar);

        if (pleasantSnackbar._queueManager is null)
        {
            pleasantSnackbar.CreateHost();
            return;
        }

        if (pleasantSnackbar._queueManager.Count <= 1)
            pleasantSnackbar.CreateHost();
    }

    private async Task Close(Action? action = null)
    {
        IsHitTestVisible = false;

        await RunCloseAnimation();
        action?.Invoke();

        _queueManager?.Dequeue();
    }

    private async Task Open()
    {
        double maxSnackbarWidth = _snackbarBorder!.Bounds.Width;

        _snackbarBorder.Width = MinWidth;

        await OpenAnimation?.RunAsync(this)!;

        Animation animation = new()
        {
            Easing = new CubicEaseInOut(),
            Duration = TimeSpan.FromSeconds(0.3),
            FillMode = FillMode.Both,
            Children =
            {
                new KeyFrame
                {
                    KeyTime = TimeSpan.FromSeconds(0),
                    Setters =
                    {
                        new Setter(WidthProperty, MinWidth)
                    }
                },
                new KeyFrame
                {
                    KeyTime = TimeSpan.FromSeconds(0.3),
                    Setters =
                    {
                        new Setter(WidthProperty, maxSnackbarWidth)
                    }
                }
            }
        };

        await animation.RunAsync(_snackbarBorder);

        _snackbarBorder.Width = double.NaN;

        if (_contentPresenter != null)
            _contentPresenter.TextWrapping = TextWrapping.WrapWithOverflow;

        if (_grid != null)
            _grid.Opacity = 1;
    }

    private void DefineButton(string? content, Action? action)
    {
        if (_button is null)
            return;

        if (string.IsNullOrWhiteSpace(content) || action is null)
            _button.IsVisible = false;

        _button.Content = content;
        _button.Click += async (_, _) =>
        {
            _closingTimer?.Dispose();

            await Close(action);
        };
    }

    private async Task RunCloseAnimation() => await CloseAnimation?.RunAsync(this)!;

    private void UpdateNotificationType()
    {
        switch (NotificationType)
        {
            case NotificationType.Error:
                PseudoClasses.Add(":error");
                break;

            case NotificationType.Information:
                PseudoClasses.Add(":information");
                break;

            case NotificationType.Success:
                PseudoClasses.Add(":success");
                break;

            case NotificationType.Warning:
                PseudoClasses.Add(":warning");
                break;
        }
    }
}