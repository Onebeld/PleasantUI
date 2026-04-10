using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using PleasantUI.Controls;

namespace PleasantUI.ToolKit.Controls;

/// <summary>
/// Severity level for the notice dialog.
/// </summary>
public enum NoticeSeverity
{
    /// <summary>Informational notice (blue).</summary>
    Info,
    /// <summary>Warning notice (yellow/orange).</summary>
    Warning,
    /// <summary>Error notice (red).</summary>
    Error,
    /// <summary>Success notice (green).</summary>
    Success,
    /// <summary>Work in progress notice (yellow).</summary>
    WorkInProgress
}

/// <summary>
/// A modal dialog for displaying notices, warnings, or work-in-progress messages.
/// Features a header with icon, message body, optional footer text, and customizable action buttons.
/// </summary>
[TemplatePart(PART_PrimaryButton, typeof(Button))]
[TemplatePart(PART_SecondaryButton, typeof(Button))]
[PseudoClasses(PC_HasSecondary, PC_HasFooter)]
public class NoticeDialog : PleasantPopupElement
{
    // ── Template part names ───────────────────────────────────────────────────

    internal const string PART_PrimaryButton = "PART_PrimaryButton";
    internal const string PART_SecondaryButton = "PART_SecondaryButton";

    // ── Pseudo-class names ────────────────────────────────────────────────────

    private const string PC_HasSecondary = ":hasSecondary";
    private const string PC_HasFooter = ":hasFooter";

    // ── Styled properties ─────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<NoticeDialog, string?>(nameof(Title));

    /// <summary>Defines the <see cref="Message"/> property.</summary>
    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<NoticeDialog, string?>(nameof(Message));

    /// <summary>Defines the <see cref="FooterText"/> property.</summary>
    public static readonly StyledProperty<string?> FooterTextProperty =
        AvaloniaProperty.Register<NoticeDialog, string?>(nameof(FooterText));

    /// <summary>Defines the <see cref="PrimaryButtonText"/> property.</summary>
    public static readonly StyledProperty<string?> PrimaryButtonTextProperty =
        AvaloniaProperty.Register<NoticeDialog, string?>(nameof(PrimaryButtonText));

    /// <summary>Defines the <see cref="SecondaryButtonText"/> property.</summary>
    public static readonly StyledProperty<string?> SecondaryButtonTextProperty =
        AvaloniaProperty.Register<NoticeDialog, string?>(nameof(SecondaryButtonText));

    /// <summary>Defines the <see cref="Severity"/> property.</summary>
    public static readonly StyledProperty<NoticeSeverity> SeverityProperty =
        AvaloniaProperty.Register<NoticeDialog, NoticeSeverity>(nameof(Severity), defaultValue: NoticeSeverity.Info);

    /// <summary>Defines the <see cref="MinDialogWidth"/> property.</summary>
    public static readonly StyledProperty<double> MinDialogWidthProperty =
        AvaloniaProperty.Register<NoticeDialog, double>(nameof(MinDialogWidth), defaultValue: 400);

    /// <summary>Defines the <see cref="MinDialogHeight"/> property.</summary>
    public static readonly StyledProperty<double> MinDialogHeightProperty =
        AvaloniaProperty.Register<NoticeDialog, double>(nameof(MinDialogHeight), defaultValue: 300);

    // ── CLR accessors ─────────────────────────────────────────────────────────

    /// <summary>Gets or sets the dialog title.</summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets or sets the main message text.</summary>
    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>Gets or sets optional footer text (e.g., attribution or additional info).</summary>
    public string? FooterText
    {
        get => GetValue(FooterTextProperty);
        set => SetValue(FooterTextProperty, value);
    }

    /// <summary>Gets or sets the text of the primary action button. Null hides the button.</summary>
    public string? PrimaryButtonText
    {
        get => GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    /// <summary>Gets or sets the text of the secondary action button. Null hides the button.</summary>
    public string? SecondaryButtonText
    {
        get => GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    /// <summary>Gets or sets the severity level, which affects the icon and header color.</summary>
    public NoticeSeverity Severity
    {
        get => GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    /// <summary>Gets or sets the minimum width of the dialog.</summary>
    public double MinDialogWidth
    {
        get => GetValue(MinDialogWidthProperty);
        set => SetValue(MinDialogWidthProperty, value);
    }

    /// <summary>Gets or sets the minimum height of the dialog.</summary>
    public double MinDialogHeight
    {
        get => GetValue(MinDialogHeightProperty);
        set => SetValue(MinDialogHeightProperty, value);
    }

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Raised when the primary button is clicked.</summary>
    public event EventHandler? PrimaryButtonClicked;

    /// <summary>Raised when the secondary button is clicked.</summary>
    public event EventHandler? SecondaryButtonClicked;

    /// <summary>Raised when the dialog is closed.</summary>
    public event EventHandler? Closed;

    // ── Private state ─────────────────────────────────────────────────────────

    private Button? _primaryButton;
    private Button? _secondaryButton;
    private Border? _modalBackground;
    private Panel? _panel;
    private bool _isClosing;

    // ── Constructor ───────────────────────────────────────────────────────────

    public NoticeDialog()
    {
    }

    // ── Template ──────────────────────────────────────────────────────────────

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        DetachHandlers();

        _primaryButton = e.NameScope.Find<Button>(PART_PrimaryButton);
        _secondaryButton = e.NameScope.Find<Button>(PART_SecondaryButton);

        AttachHandlers();
        UpdatePseudoClasses();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SecondaryButtonTextProperty)
            PseudoClasses.Set(PC_HasSecondary, change.NewValue is not null);
        else if (change.Property == FooterTextProperty)
            PseudoClasses.Set(PC_HasFooter, change.NewValue is not null);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Shows the dialog on the specified <see cref="TopLevel"/>.</summary>
    public async Task ShowAsync(TopLevel? topLevel = null)
    {
        _panel = new Panel();

        _modalBackground = new Border
        {
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#3A000000")),
            Opacity = 0
        };

        _panel.Children.Add(_modalBackground);
        _panel.Children.Add(this);

        Host ??= new ModalWindowHost();
        Host.Content = _panel;

        base.ShowCoreForTopLevel(topLevel);

        PseudoClasses.Set(":open", true);

        // Fade in modal background
        if (_modalBackground is not null)
        {
            var bgAnim = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(200),
                FillMode = Avalonia.Animation.FillMode.Forward
            };
            var kf = new KeyFrame { Cue = new Cue(1.0) };
            kf.Setters.Add(new Setter(OpacityProperty, 1.0));
            bgAnim.Children.Add(kf);
            await bgAnim.RunAsync(_modalBackground);
        }
    }

    /// <summary>Closes the dialog.</summary>
    public async Task CloseAsync()
    {
        if (_isClosing) return;
        _isClosing = true;

        PseudoClasses.Set(":open", false);
        base.DeleteCoreForTopLevel();

        _isClosing = false;
        Closed?.Invoke(this, EventArgs.Empty);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void AttachHandlers()
    {
        if (_primaryButton is not null) _primaryButton.Click += OnPrimaryClicked;
        if (_secondaryButton is not null) _secondaryButton.Click += OnSecondaryClicked;
    }

    private void DetachHandlers()
    {
        if (_primaryButton is not null) _primaryButton.Click -= OnPrimaryClicked;
        if (_secondaryButton is not null) _secondaryButton.Click -= OnSecondaryClicked;
    }

    private void OnPrimaryClicked(object? s, RoutedEventArgs e) => PrimaryButtonClicked?.Invoke(this, EventArgs.Empty);
    private void OnSecondaryClicked(object? s, RoutedEventArgs e) => SecondaryButtonClicked?.Invoke(this, EventArgs.Empty);

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(PC_HasSecondary, SecondaryButtonText is not null);
        PseudoClasses.Set(PC_HasFooter, FooterText is not null);
    }
}
