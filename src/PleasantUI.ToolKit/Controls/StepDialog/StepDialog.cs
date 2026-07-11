using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using PleasantUI.Controls;

namespace PleasantUI.ToolKit.Controls;

/// <summary>
/// A modal dialog that presents a sequence of numbered <see cref="StepItem"/> steps.
/// Supports a title, description, status message, primary/secondary action buttons,
/// and open/close animations consistent with <see cref="ContentDialog"/>.
/// </summary>
[TemplatePart(PART_CloseButton, typeof(Button))]
[TemplatePart(PART_PrimaryButton, typeof(Button))]
[TemplatePart(PART_SecondaryButton, typeof(Button))]
[TemplatePart(PART_StepsHost, typeof(ItemsControl))]
[PseudoClasses(PC_Open, PC_HasStatus, PC_HasPrimary, PC_HasSecondary)]
public class StepDialog : ContentDialog
{
    private const string PART_CloseButton = "PART_CloseButton";
    private const string PART_PrimaryButton = "PART_PrimaryButton";
    private const string PART_SecondaryButton = "PART_SecondaryButton";
    private const string PART_StepsHost = "PART_StepsHost";

    private const string PC_Open = ":open";
    private const string PC_HasStatus = ":hasStatus";
    private const string PC_HasPrimary = ":hasPrimary";
    private const string PC_HasSecondary = ":hasSecondary";

    private Button? _closeButton;
    private Button? _primaryButton;
    private Button? _secondaryButton;
    private ItemsControl? _stepsHost;

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

    /// <summary>Defines the <see cref="Steps"/> direct property.</summary>
    public static readonly DirectProperty<StepDialog, AvaloniaList<StepItem>> StepsProperty =
        AvaloniaProperty.RegisterDirect<StepDialog, AvaloniaList<StepItem>>(
            nameof(Steps), o => o.Steps);

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

    /// <summary>Gets the collection of steps displayed in the dialog.</summary>
    [Content]
    public AvaloniaList<StepItem> Steps { get; } = [];  

    /// <summary>Raised when the primary button is clicked.</summary>
    public event EventHandler? PrimaryButtonClicked;

    /// <summary>Raised when the secondary button is clicked.</summary>
    public event EventHandler? SecondaryButtonClicked;

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(StepDialog);

    public StepDialog()
    {
        Steps.CollectionChanged += OnStepsChanged;
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        DetachHandlers();

        _closeButton = e.NameScope.Find<Button>(PART_CloseButton);
        _primaryButton = e.NameScope.Find<Button>(PART_PrimaryButton);
        _secondaryButton = e.NameScope.Find<Button>(PART_SecondaryButton);
        _stepsHost = e.NameScope.Find<ItemsControl>(PART_StepsHost);

        AttachHandlers();

        _stepsHost?.ItemsSource = Steps;

        RenumberSteps();
        UpdatePseudoClasses();
    }

    /// <inheritdoc />
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

    private void AttachHandlers()
    {
        if (_closeButton is not null) _closeButton.Click += OnCloseClicked;
        if (_primaryButton is not null) _primaryButton.Click += OnPrimaryClicked;
        if (_secondaryButton is not null) _secondaryButton.Click += OnSecondaryClicked;
    }

    private void DetachHandlers()
    {
        if (_closeButton is not null) _closeButton.Click -= OnCloseClicked;
        if (_primaryButton is not null) _primaryButton.Click -= OnPrimaryClicked;
        if (_secondaryButton is not null) _secondaryButton.Click -= OnSecondaryClicked;
    }

    private void OnCloseClicked(object? s, RoutedEventArgs e) => _ = CloseAsync();
    private void OnPrimaryClicked(object? s, RoutedEventArgs e) => PrimaryButtonClicked?.Invoke(this, EventArgs.Empty);

    private void OnSecondaryClicked(object? s, RoutedEventArgs e) =>
        SecondaryButtonClicked?.Invoke(this, EventArgs.Empty);

    private void OnStepsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RenumberSteps();
        _stepsHost?.ItemsSource = Steps;
    }

    private void RenumberSteps()
    {
        for (int i = 0; i < Steps.Count; i++)
            Steps[i].StepNumber = i + 1;
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(PC_HasStatus, StatusMessage is not null);
        PseudoClasses.Set(PC_HasPrimary, PrimaryButtonText is not null);
        PseudoClasses.Set(PC_HasSecondary, SecondaryButtonText is not null);
    }
}