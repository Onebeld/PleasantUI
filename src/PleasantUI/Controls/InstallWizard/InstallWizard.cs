using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using PleasantUI.Core.Localization;

namespace PleasantUI.Controls;

/// <summary>
/// Display mode for an install wizard.
/// </summary>
public enum WizardDisplayMode
{
    /// <summary>Embedded directly inside a parent layout.</summary>
    Embedded,
    /// <summary>Shown as a <see cref="ContentDialog"/> modal overlay.</summary>
    Modal,
    /// <summary>Shown inside a standalone <see cref="PleasantWindow"/>.</summary>
    Window
}

/// <summary>
/// A multi-step installation wizard control with a sidebar step list,
/// content area, progress bar, and Back / Next / Cancel navigation.
/// </summary>
[TemplatePart(PART_BackButton,   typeof(Button))]
[TemplatePart(PART_NextButton,   typeof(Button))]
[TemplatePart(PART_CancelButton, typeof(Button))]
public class InstallWizard : TemplatedControl
{
    /// <summary>Template part name for the Back button.</summary>
    public const string PART_BackButton   = "PART_BackButton";
    /// <summary>Template part name for the Next/Finish button.</summary>
    public const string PART_NextButton   = "PART_NextButton";
    /// <summary>Template part name for the Cancel button.</summary>
    public const string PART_CancelButton = "PART_CancelButton";
    private Button? _backButton;
    private Button? _nextButton;
    private Button? _cancelButton;

    // ── Properties ───────────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="Steps"/> property.</summary>
    public static readonly StyledProperty<IList<WizardStep>> StepsProperty =
        AvaloniaProperty.Register<InstallWizard, IList<WizardStep>>(nameof(Steps));

    /// <summary>Defines the <see cref="CurrentStepIndex"/> property.</summary>
    public static readonly StyledProperty<int> CurrentStepIndexProperty =
        AvaloniaProperty.Register<InstallWizard, int>(nameof(CurrentStepIndex), defaultValue: 0);
    /// <summary>Defines the <see cref="AppName"/> property.</summary>
    public static readonly StyledProperty<string?> AppNameProperty =
        AvaloniaProperty.Register<InstallWizard, string?>(nameof(AppName));

    /// <summary>Defines the <see cref="AppIcon"/> property.</summary>
    public static readonly StyledProperty<object?> AppIconProperty =
        AvaloniaProperty.Register<InstallWizard, object?>(nameof(AppIcon));

    /// <summary>Defines the <see cref="FooterText"/> property.</summary>
    public static readonly StyledProperty<string?> FooterTextProperty =
        AvaloniaProperty.Register<InstallWizard, string?>(nameof(FooterText));

    /// <summary>Defines the <see cref="NextButtonText"/> property.</summary>
    public static readonly StyledProperty<string> NextButtonTextProperty =
        AvaloniaProperty.Register<InstallWizard, string>(nameof(NextButtonText), defaultValue: "Next");

    /// <summary>Defines the <see cref="BackButtonText"/> property.</summary>
    public static readonly StyledProperty<string> BackButtonTextProperty =
        AvaloniaProperty.Register<InstallWizard, string>(nameof(BackButtonText), defaultValue: "Back");

    /// <summary>Defines the <see cref="CancelButtonText"/> property.</summary>
    public static readonly StyledProperty<string> CancelButtonTextProperty =
        AvaloniaProperty.Register<InstallWizard, string>(nameof(CancelButtonText), defaultValue: "Cancel");

    /// <summary>Defines the <see cref="ShowCancelButton"/> property.</summary>
    public static readonly StyledProperty<bool> ShowCancelButtonProperty =
        AvaloniaProperty.Register<InstallWizard, bool>(nameof(ShowCancelButton), defaultValue: true);

    // ── Routed events ────────────────────────────────────────────────────────

    /// <summary>Raised when the user clicks Next on the last step.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> FinishedEvent =
        RoutedEvent.Register<InstallWizard, RoutedEventArgs>(nameof(Finished), RoutingStrategies.Bubble);

    /// <summary>Raised when the user clicks Cancel.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> CancelledEvent =
        RoutedEvent.Register<InstallWizard, RoutedEventArgs>(nameof(Cancelled), RoutingStrategies.Bubble);

    /// <summary>Raised whenever the active step changes.</summary>
    public static readonly RoutedEvent<WizardStepChangedEventArgs> StepChangedEvent =
        RoutedEvent.Register<InstallWizard, WizardStepChangedEventArgs>(nameof(StepChanged), RoutingStrategies.Bubble);

    // ── CLR accessors ────────────────────────────────────────────────────────

    /// <summary>The ordered list of wizard steps.</summary>
    public IList<WizardStep> Steps
    {
        get
        {
            var list = GetValue(StepsProperty);
            if (list is null)
            {
                var avList = new AvaloniaList<WizardStep>();
                avList.CollectionChanged += OnStepsCollectionChanged;
                SetValue(StepsProperty, avList);
                return avList;
            }
            return list;
        }
        set
        {
            // Unsubscribe from old list
            if (GetValue(StepsProperty) is AvaloniaList<WizardStep> old)
                old.CollectionChanged -= OnStepsCollectionChanged;

            if (value is AvaloniaList<WizardStep> newList)
                newList.CollectionChanged += OnStepsCollectionChanged;

            SetValue(StepsProperty, value);
        }
    }

    private void OnStepsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (sender is not IList<WizardStep> list) return;

        // Deduplicate: if the list now contains duplicate headers (reinit scenario),
        // keep only the LAST occurrence of each header — i.e. the freshly-added ones.
        // This fires after each Add, so we check on every addition.
        var seen = new HashSet<string?>();
        var toRemove = new List<WizardStep>();

        // Walk backwards — keep the last occurrence of each header
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var header = list[i].Header;
            if (!seen.Add(header))
                toRemove.Add(list[i]);
        }

        if (toRemove.Count == 0) return;

        // Remove duplicates (the earlier/stale ones)
        foreach (var step in toRemove)
            list.Remove(step);

        RefreshComputedProperties();
        UpdateButtonStates();
    }    /// <summary>Zero-based index of the currently visible step.</summary>
    public int CurrentStepIndex
    {
        get => GetValue(CurrentStepIndexProperty);
        set => SetValue(CurrentStepIndexProperty, value);
    }

    /// <summary>Application name shown in the sidebar header.</summary>
    public string? AppName
    {
        get => GetValue(AppNameProperty);
        set => SetValue(AppNameProperty, value);
    }

    /// <summary>Icon shown in the sidebar header (e.g. a PathIcon or Image).</summary>
    public object? AppIcon
    {
        get => GetValue(AppIconProperty);
        set => SetValue(AppIconProperty, value);
    }

    /// <summary>Optional copyright / footer text shown at the bottom of the sidebar.</summary>
    public string? FooterText
    {
        get => GetValue(FooterTextProperty);
        set => SetValue(FooterTextProperty, value);
    }

    /// <summary>Label for the Next/Finish button.</summary>
    public string NextButtonText
    {
        get => GetValue(NextButtonTextProperty);
        set => SetValue(NextButtonTextProperty, value);
    }

    /// <summary>Label for the Back button.</summary>
    public string BackButtonText
    {
        get => GetValue(BackButtonTextProperty);
        set => SetValue(BackButtonTextProperty, value);
    }

    /// <summary>Label for the Cancel button.</summary>
    public string CancelButtonText
    {
        get => GetValue(CancelButtonTextProperty);
        set => SetValue(CancelButtonTextProperty, value);
    }

    /// <summary>Whether the Cancel button is visible.</summary>
    public bool ShowCancelButton
    {
        get => GetValue(ShowCancelButtonProperty);
        set => SetValue(ShowCancelButtonProperty, value);
    }

    // ── Events ───────────────────────────────────────────────────────────────

    /// <summary>Raised when the user completes the last step.</summary>
    public event EventHandler<RoutedEventArgs>? Finished
    {
        add    => AddHandler(FinishedEvent, value);
        remove => RemoveHandler(FinishedEvent, value);
    }

    /// <summary>Raised when the user cancels the wizard.</summary>
    public event EventHandler<RoutedEventArgs>? Cancelled
    {
        add    => AddHandler(CancelledEvent, value);
        remove => RemoveHandler(CancelledEvent, value);
    }

    /// <summary>Raised when the active step changes.</summary>
    public event EventHandler<WizardStepChangedEventArgs>? StepChanged
    {
        add    => AddHandler(StepChangedEvent, value);
        remove => RemoveHandler(StepChangedEvent, value);
    }

    // ── Computed helpers (used by bindings in the template) ──────────────────

    /// <summary>Progress value 0–100 based on current step.</summary>
    public static readonly DirectProperty<InstallWizard, double> ProgressProperty =
        AvaloniaProperty.RegisterDirect<InstallWizard, double>(nameof(Progress),
            o => o.Progress);

    /// <summary>The currently active step, or null.</summary>
    public static readonly DirectProperty<InstallWizard, WizardStep?> CurrentStepProperty =
        AvaloniaProperty.RegisterDirect<InstallWizard, WizardStep?>(nameof(CurrentStep),
            o => o.CurrentStep);

    private double _progress;
    private WizardStep? _currentStep;

    /// <summary>Progress value 0–100 based on current step.</summary>
    public double Progress
    {
        get => _progress;
        private set => SetAndRaise(ProgressProperty, ref _progress, value);
    }

    /// <summary>The currently active step, or null.</summary>
    public WizardStep? CurrentStep
    {
        get => _currentStep;
        private set => SetAndRaise(CurrentStepProperty, ref _currentStep, value);
    }

    private void RefreshComputedProperties()
    {
        Progress = Steps.Count <= 1 ? 100 : (double)CurrentStepIndex / (Steps.Count - 1) * 100;
        CurrentStep = CurrentStepIndex >= 0 && CurrentStepIndex < Steps.Count
            ? Steps[CurrentStepIndex]
            : null;

        // Update IsActive and StepNumber on each step so the DataTemplate
        // bindings in the AXAML template can show/hide and number correctly.
        for (int i = 0; i < Steps.Count; i++)
        {
            Steps[i].StepNumber = i + 1;
            Steps[i].IsActive   = i == CurrentStepIndex;
        }
    }

    // ── Static constructor ───────────────────────────────────────────────────

    static InstallWizard()
    {
        CurrentStepIndexProperty.Changed.AddClassHandler<InstallWizard, int>(
            (w, _) => w.OnStepIndexChanged());

        StepsProperty.Changed.AddClassHandler<InstallWizard, IList<WizardStep>>(
            (w, _) => w.OnStepsReplaced());
    }

    // ── Template ─────────────────────────────────────────────────────────────

    private bool _stepsLoadPending;

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == StepsProperty)
            OnStepsReplaced();
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_backButton is not null)   _backButton.Click   -= OnBackClicked;
        if (_nextButton is not null)   _nextButton.Click   -= OnNextClicked;
        if (_cancelButton is not null) _cancelButton.Click -= OnCancelClicked;

        _backButton   = e.NameScope.Find<Button>(PART_BackButton);
        _nextButton   = e.NameScope.Find<Button>(PART_NextButton);
        _cancelButton = e.NameScope.Find<Button>(PART_CancelButton);

        if (_backButton is not null)   _backButton.Click   += OnBackClicked;
        if (_nextButton is not null)   _nextButton.Click   += OnNextClicked;
        if (_cancelButton is not null) _cancelButton.Click += OnCancelClicked;

        Localizer.Instance.LocalizationChanged += OnLocalizationChanged;

        RefreshComputedProperties();
        UpdateButtonStates();
    }

    private void OnLocalizationChanged(string _) => UpdateButtonStates();

    // ── Navigation ───────────────────────────────────────────────────────────

    /// <summary>Moves to the next step, or raises <see cref="Finished"/> on the last step.</summary>
    public void GoNext()
    {
        if (CurrentStepIndex >= Steps.Count - 1)
        {
            RaiseEvent(new RoutedEventArgs(FinishedEvent));
            return;
        }

        if (CurrentStep is not null)
            CurrentStep.IsCompleted = true;

        CurrentStepIndex++;
    }

    /// <summary>Moves to the previous step.</summary>
    public void GoBack()
    {
        if (CurrentStepIndex > 0)
            CurrentStepIndex--;
    }

    // ── Private ──────────────────────────────────────────────────────────────

    private void OnBackClicked(object? sender, RoutedEventArgs e)   => GoBack();
    private void OnNextClicked(object? sender, RoutedEventArgs e)   => GoNext();
    private void OnCancelClicked(object? sender, RoutedEventArgs e) =>
        RaiseEvent(new RoutedEventArgs(CancelledEvent));

    private void OnStepIndexChanged()
    {
        RefreshComputedProperties();
        RaiseEvent(new WizardStepChangedEventArgs(StepChangedEvent, CurrentStepIndex, CurrentStep));
        UpdateButtonStates();
    }

    private void OnStepsReplaced()
    {
        // When AXAML reinit replaces the Steps collection with a new one,
        // reset the index so we don't point past the end of the new list.
        SetCurrentValue(CurrentStepIndexProperty, 0);
        RefreshComputedProperties();
        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        if (_backButton is not null)
            _backButton.IsEnabled = CurrentStepIndex > 0;

        if (_nextButton is not null)
        {
            bool isLast = CurrentStepIndex >= Steps.Count - 1;
            _nextButton.Content = isLast
                ? Localizer.Instance.TryGetString("InstallWizard/BtnFinish", out var finish) ? finish : "Finish"
                : Localizer.Instance.TryGetString("InstallWizard/BtnNext",   out var next)   ? next   : NextButtonText;
        }
    }

    // ── Display mode factory methods ─────────────────────────────────────────

    /// <summary>
    /// Shows the wizard as a <see cref="ContentDialog"/> modal overlay on the given top-level.
    /// The dialog closes automatically when the wizard raises <see cref="Finished"/> or <see cref="Cancelled"/>.
    /// </summary>
    /// <param name="wizard">The configured wizard instance to show.</param>
    /// <param name="topLevel">The window or top-level to host the dialog on.</param>
    /// <returns>A task that completes when the dialog is closed. Returns <c>true</c> if finished, <c>false</c> if cancelled.</returns>
    public static async Task<bool> ShowAsModalAsync(InstallWizard wizard, TopLevel topLevel)
    {
        var tcs = new TaskCompletionSource<bool>();

        var dialog = new ContentDialog
        {
            Content = wizard,
            Padding = new Thickness(0)
        };

        wizard.Finished   += async (_, _) => { await dialog.CloseAsync(true);  tcs.TrySetResult(true); };
        wizard.Cancelled  += async (_, _) => { await dialog.CloseAsync(false); tcs.TrySetResult(false); };

        await dialog.ShowAsync(topLevel);
        return await tcs.Task;
    }

    /// <summary>
    /// Shows the wizard inside a standalone <see cref="PleasantWindow"/>.
    /// The window closes automatically when the wizard raises <see cref="Finished"/> or <see cref="Cancelled"/>.
    /// </summary>
    /// <param name="wizard">The configured wizard instance to show.</param>
    /// <param name="owner">Optional owner window for positioning.</param>
    /// <param name="width">Window width. Defaults to 760.</param>
    /// <param name="height">Window height. Defaults to 500.</param>
    /// <returns>A task that completes when the window is closed. Returns <c>true</c> if finished, <c>false</c> if cancelled.</returns>
    public static Task<bool> ShowAsWindowAsync(
        InstallWizard wizard,
        Window? owner = null,
        double width = 760,
        double height = 500)
    {
        var tcs = new TaskCompletionSource<bool>();

        var window = new PleasantWindow
        {
            Title                  = wizard.AppName ?? "Setup",
            Width                  = width,
            Height                 = height,
            CanResize              = false,
            WindowStartupLocation  = owner is not null
                                        ? WindowStartupLocation.CenterOwner
                                        : WindowStartupLocation.CenterScreen,
            Content                = wizard,
            EnableCustomTitleBar   = true,
        };

        wizard.Finished  += (_, _) => { tcs.TrySetResult(true);  window.Close(); };
        wizard.Cancelled += (_, _) => { tcs.TrySetResult(false); window.Close(); };

        window.Closed += (_, _) => tcs.TrySetResult(false);

        if (owner is not null)
            window.ShowDialog(owner);
        else
            window.Show();

        return tcs.Task;
    }
}

/// <summary>Event args for <see cref="InstallWizard.StepChanged"/>.</summary>
public class WizardStepChangedEventArgs : RoutedEventArgs
{
    /// <summary>Zero-based index of the new active step.</summary>
    public int StepIndex { get; }

    /// <summary>The new active step, or null.</summary>
    public WizardStep? Step { get; }

    /// <summary>Initializes a new instance of <see cref="WizardStepChangedEventArgs"/>.</summary>
    public WizardStepChangedEventArgs(RoutedEvent routedEvent, int index, WizardStep? step)
        : base(routedEvent)
    {
        StepIndex = index;
        Step = step;
    }
}
