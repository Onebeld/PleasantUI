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
using PleasantUI.Core.Interfaces;

namespace PleasantUI.Controls;

/// <summary>
/// A modern and customizable Snackbar control for displaying notifications.
/// </summary>
public class PleasantSnackbar : ContentControl
{
    private Button? _button;
    private ContentPresenter? _contentPresenter;
    private Grid? _grid;
    private Border? _snackbarBorder;
    
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
    /// Shows a Snackbar with the specified message, icon, and notification type.
    /// </summary>
    /// <param name="parent">The parent window to which the Snackbar will be added.</param>
    /// <param name="message">The message to display in the Snackbar.</param>
    /// <param name="icon">The icon to display in the Snackbar (optional).</param>
    /// <param name="notificationType">The type of notification (optional, defaults to Information).</param>
    /// <param name="timeSpan">The duration the Snackbar will be displayed (optional, defaults to 3 seconds).</param>
    public static void Show(
        IPleasantWindow parent,
        string message,
        string? buttonText = null,
        Action? buttonAction = null,
        Geometry? icon = null,
        NotificationType notificationType = NotificationType.Information,
        TimeSpan timeSpan = default)
    {
        if (timeSpan == default)
            timeSpan = TimeSpan.FromSeconds(3);

        PleasantSnackbar pleasantSnackbar = new()
        {
            Content = message,
            Icon = icon,
            NotificationType = notificationType
        };

        pleasantSnackbar.Loaded += async (_, _) =>
        {
            await pleasantSnackbar.Open();

            async void CloseSnackbar()
            {
                pleasantSnackbar.IsHitTestVisible = false;

                await pleasantSnackbar.Close();

                parent.SnackbarQueueManager.Dequeue();
            }

            IDisposable timer = DispatcherTimer.RunOnce(CloseSnackbar, timeSpan);

            pleasantSnackbar.PointerPressed += (_, _) =>
            {
                timer.Dispose();

                CloseSnackbar();
            };
        };

        parent.SnackbarQueueManager.Enqueue(pleasantSnackbar);

        if (parent.SnackbarQueueManager.Count <= 1)
            parent.AddControl(pleasantSnackbar);
    }

    /// <summary>
    /// Opens the Snackbar with an animation.
    /// </summary>
    public async Task Open()
    {
        double maxSnackbarWidth = _snackbarBorder!.Bounds.Width;

        _snackbarBorder.Width = MinWidth;

        await OpenAnimation?.RunAsync(this);

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
        _contentPresenter.TextWrapping = TextWrapping.WrapWithOverflow;
        _contentPresenter.Opacity = 1;
    }

    /// <summary>
    /// Sets the text and action for the button. If <paramref name="content" /> is <see langword="null" /> or empty, or
    /// if <paramref name="action" /> is <see langword="null" />, the button will be hidden.
    /// </summary>
    /// <param name="content">The text to display on the button.</param>
    /// <param name="action">The action to perform when the button is clicked.</param>
    public void DefineButton(string? content, Action? action)
    {
        if (_button is null)
            return;

        if (string.IsNullOrWhiteSpace(content) || action is null)
            _button.IsVisible = false;

        _button.Content = content;
        _button.Command = Core.Helpers.Command.Create(action);
    }

    /// <summary>
    /// Closes the Snackbar with an animation.
    /// </summary>
    public async Task Close()
    {
        await CloseAnimation?.RunAsync(this);
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