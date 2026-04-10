using System.Collections.Specialized;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Metadata;

namespace PleasantUI.ToolKit.Controls;

/// <summary>
/// A modal dialog that presents a sequence of numbered <see cref="StepItem"/> steps.
/// Supports a title, description, status message, primary/secondary action buttons,
/// and open/close animations consistent with <see cref="ContentDialog"/>.
/// </summary>
[TemplatePart(PART_CloseButton,    typeof(Button))]
[TemplatePart(PART_PrimaryButton,  typeof(Button))]
[TemplatePart(PART_SecondaryButton, typeof(Button))]
[TemplatePart(PART_StepsHost,      typeof(ItemsControl))]
[PseudoClasses(PC_Open, PC_HasStatus, PC_HasPrimary, PC_HasSecondary)]
public class StepDialog : PleasantPopupElement
{
    // ── Template part names ───────────────────────────────────────────────────

    internal const string PART_CloseButton     = "PART_CloseButton";
    internal const string PART_PrimaryButton   = "PART_PrimaryButton";
    internal const string PART_SecondaryButton = "PART_SecondaryButton";
    internal const string PART_StepsHost       = "PART_StepsHost";

    // ── Pseudo-class names ────────────────────────────────────────────────────

    private const string PC_Open         = ":open";
    private const string PC_HasStatus    = ":hasStatus";
    private const string PC_HasPrimary   = ":hasPrimary";
    private const string PC_HasSecondary = ":hasSecondary";

    // ── Styled properties ─────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<StepDialog, string?>(nameof(Title));

    /// <summary>Defines the <see cref="Description"/> property.</summary>
    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<StepDialog, string?>(nameof(Description));

    /// <summary>Defines the <see cref="StatusMessage"/> property.</summary>
    public static readonly StyledProperty<string?> StatusMessageProperty =
        AvaloniaProperty.Register<StepDialog, string?>(nameof(StatusMessage));

    /// <summary>Defines the <see cref="PrimaryButtonText"/> property.</summary>
    public static readonly StyledProperty<string?> PrimaryButtonTextProperty =
        AvaloniaProperty.Register<StepDialog, string?>(nameof(PrimaryButtonText));

    /// <summary>Defines the <see cref="SecondaryButtonText"/> property.</summary>
    public static readonly StyledProperty<string?> SecondaryButtonTextProperty =
        AvaloniaProperty.Register<StepDialog, string?>(nameof(SecondaryButtonText));

    /// <summary>Defines the <see cref="MinDialogWidth"/> property.</summary>
    public static readonly StyledProperty<double> MinDialogWidthProperty =
        AvaloniaProperty.Register<StepDialog, double>(nameof(MinDialogWidth), defaultValue: 420);

    /// <summary>Defines the <see cref="OpenAnimation"/> property.</summary>
    public static readonly StyledProperty<Animation?> OpenAnimationProperty =
        AvaloniaProperty.Register<StepDialog, Animation?>(nameof(OpenAnimation));

    /// <summary>Defines the <see cref="CloseAnimation"/> property.</summary>
    public static readonly StyledProperty<Animation?> CloseAnimationProperty =
        AvaloniaProperty.Register<StepDialog, Animation?>(nameof(CloseAnimation));

    // ── Direct properties ─────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="Steps"/> direct property.</summary>
    public static readonly DirectProperty<StepDialog, AvaloniaList<StepItem>> StepsProperty =
        AvaloniaProperty.RegisterDirect<StepDialog, AvaloniaList<StepItem>>(
            nameof(Steps), o => o.Steps);

    // ── CLR accessors ─────────────────────────────────────────────────────────

    /// <summary>Gets or sets the dialog title.</summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets or sets an optional description shown below the title.</summary>
    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>Gets or sets a status message shown at the bottom of the steps (e.g. "Waiting for authentication…").</summary>
    public string? StatusMessage
    {
        get => GetValue(StatusMessageProperty);
        set => SetValue(StatusMessageProperty, value);
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

    /// <summary>Gets or sets the minimum width of the dialog card.</summary>
    public double MinDialogWidth
    {
        get => GetValue(MinDialogWidthProperty);
        set => SetValue(MinDialogWidthProperty, value);
    }

    /// <summary>Gets or sets the animation played when the dialog opens.</summary>
    public Animation? OpenAnimation
    {
        get => GetValue(OpenAnimationProperty);
        set => SetValue(OpenAnimationProperty, value);
    }

    /// <summary>Gets or sets the animation played when the dialog closes.</summary>
    public Animation? CloseAnimation
    {
        get => GetValue(CloseAnimationProperty);
        set => SetValue(CloseAnimationProperty, value);
    }

    /// <summary>Gets the collection of steps displayed in the dialog.</summary>
    [Content]
    public AvaloniaList<StepItem> Steps { get; } = new();

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Raised when the primary button is clicked.</summary>
    public event EventHandler? PrimaryButtonClicked;

    /// <summary>Raised when the secondary button is clicked.</summary>
    public event EventHandler? SecondaryButtonClicked;

    /// <summary>Raised when the dialog is closed.</summary>
    public event EventHandler? Closed;

    // ── Private state ─────────────────────────────────────────────────────────

    private Button?       _closeButton;
    private Button?       _primaryButton;
    private Button?       _secondaryButton;
    private ItemsControl? _stepsHost;
    private Border?       _modalBackground;
    private Panel?        _panel;
    private bool          _isClosing;

    // ── Constructor ───────────────────────────────────────────────────────────

    public StepDialog()
    {
        Steps.CollectionChanged += OnStepsChanged;
    }

    // ── Template ──────────────────────────────────────────────────────────────

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        DetachHandlers();

        _closeButton     = e.NameScope.Find<Button>(PART_CloseButton);
        _primaryButton   = e.NameScope.Find<Button>(PART_PrimaryButton);
        _secondaryButton = e.NameScope.Find<Button>(PART_SecondaryButton);
        _stepsHost       = e.NameScope.Find<ItemsControl>(PART_StepsHost);

        AttachHandlers();

        if (_stepsHost is not null)
            _stepsHost.ItemsSource = Steps;

        RenumberSteps();
        UpdatePseudoClasses();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == StatusMessageProperty)
            PseudoClasses.Set(PC_HasStatus, change.NewValue is not null);
        else if (change.Property == PrimaryButtonTextProperty)
            PseudoClasses.Set(PC_HasPrimary, change.NewValue is not null);
        else if (change.Property == SecondaryButtonTextProperty)
            PseudoClasses.Set(PC_HasSecondary, change.NewValue is not null);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Shows the dialog on the specified <see cref="TopLevel"/>.</summary>
    public async Task ShowAsync(TopLevel? topLevel = null)
    {
        _panel = new Panel();

        _modalBackground = new Border
        {
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#3A000000")),
            Opacity    = 0
        };

        _panel.Children.Add(_modalBackground);
        _panel.Children.Add(this);

        Host ??= new ModalWindowHost();
        Host.Content = _panel;

        base.ShowCoreForTopLevel(topLevel);

        PseudoClasses.Set(PC_Open, true);

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

        await RunOpenAnimation();
    }

    /// <summary>Closes the dialog.</summary>
    public async Task CloseAsync()
    {
        if (_isClosing) return;
        _isClosing = true;

        await RunCloseAnimation();

        PseudoClasses.Set(PC_Open, false);
        base.DeleteCoreForTopLevel();

        _isClosing = false;
        Closed?.Invoke(this, EventArgs.Empty);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void AttachHandlers()
    {
        if (_closeButton     is not null) _closeButton.Click     += OnCloseClicked;
        if (_primaryButton   is not null) _primaryButton.Click   += OnPrimaryClicked;
        if (_secondaryButton is not null) _secondaryButton.Click += OnSecondaryClicked;
    }

    private void DetachHandlers()
    {
        if (_closeButton     is not null) _closeButton.Click     -= OnCloseClicked;
        if (_primaryButton   is not null) _primaryButton.Click   -= OnPrimaryClicked;
        if (_secondaryButton is not null) _secondaryButton.Click -= OnSecondaryClicked;
    }

    private void OnCloseClicked(object? s, RoutedEventArgs e)     => _ = CloseAsync();
    private void OnPrimaryClicked(object? s, RoutedEventArgs e)   => PrimaryButtonClicked?.Invoke(this, EventArgs.Empty);
    private void OnSecondaryClicked(object? s, RoutedEventArgs e) => SecondaryButtonClicked?.Invoke(this, EventArgs.Empty);

    private void OnStepsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RenumberSteps();
        if (_stepsHost is not null)
            _stepsHost.ItemsSource = Steps;
    }

    private void RenumberSteps()
    {
        for (int i = 0; i < Steps.Count; i++)
            Steps[i].StepNumber = i + 1;
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(PC_HasStatus,    StatusMessage is not null);
        PseudoClasses.Set(PC_HasPrimary,   PrimaryButtonText is not null);
        PseudoClasses.Set(PC_HasSecondary, SecondaryButtonText is not null);
    }

    private async Task RunOpenAnimation()
    {
        if (OpenAnimation is not null)
            await OpenAnimation.RunAsync(this);
    }

    private async Task RunCloseAnimation()
    {
        if (CloseAnimation is not null)
            await CloseAnimation.RunAsync(this);
    }
}
