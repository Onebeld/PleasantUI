using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using PleasantUI.Core.Localization;

namespace PleasantUI.ToolKit.Controls;

/// <summary>
/// A multi-step wizard with a prominent sidebar for step navigation,
/// content area, progress bar, and Back / Next / Cancel navigation.
/// Suitable for authentication flows, setup wizards, and guided processes.
/// </summary>
[TemplatePart(PART_BackButton, typeof(Button))]
[TemplatePart(PART_NextButton, typeof(Button))]
[TemplatePart(PART_CancelButton, typeof(Button))]
[TemplatePart(PART_StepsHost, typeof(ItemsControl))]
[TemplatePart(PART_ContentPresenter, typeof(ContentPresenter))]
[PseudoClasses(PC_HasSystemInfo, PC_HasStatus)]
public class SidebarWizard : TemplatedControl
{
    // ── Template part names ───────────────────────────────────────────────────

    internal const string PART_BackButton = "PART_BackButton";
    internal const string PART_NextButton = "PART_NextButton";
    internal const string PART_CancelButton = "PART_CancelButton";
    internal const string PART_StepsHost = "PART_StepsHost";
    internal const string PART_ContentPresenter = "PART_ContentPresenter";

    // ── Pseudo-class names ────────────────────────────────────────────────────

    private const string PC_HasSystemInfo = ":hasSystemInfo";
    private const string PC_HasStatus = ":hasStatus";

    // ── Properties ───────────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="Steps"/> property.</summary>
    public static readonly StyledProperty<IList<SidebarWizardStep>> StepsProperty =
        AvaloniaProperty.Register<SidebarWizard, IList<SidebarWizardStep>>(nameof(Steps));

    /// <summary>Defines the <see cref="CurrentStepIndex"/> property.</summary>
    public static readonly StyledProperty<int> CurrentStepIndexProperty =
        AvaloniaProperty.Register<SidebarWizard, int>(nameof(CurrentStepIndex), defaultValue: 0);

    /// <summary>Defines the <see cref="AppName"/> property.</summary>
    public static readonly StyledProperty<string?> AppNameProperty =
        AvaloniaProperty.Register<SidebarWizard, string?>(nameof(AppName));

    /// <summary>Defines the <see cref="AppIcon"/> property.</summary>
    public static readonly StyledProperty<object?> AppIconProperty =
        AvaloniaProperty.Register<SidebarWizard, object?>(nameof(AppIcon));

    /// <summary>Defines the <see cref="SystemInfo"/> property.</summary>
    public static readonly StyledProperty<string?> SystemInfoProperty =
        AvaloniaProperty.Register<SidebarWizard, string?>(nameof(SystemInfo));

    /// <summary>Defines the <see cref="StatusMessage"/> property.</summary>
    public static readonly StyledProperty<string?> StatusMessageProperty =
        AvaloniaProperty.Register<SidebarWizard, string?>(nameof(StatusMessage));

    /// <summary>Defines the <see cref="NextButtonText"/> property.</summary>
    public static readonly StyledProperty<string> NextButtonTextProperty =
        AvaloniaProperty.Register<SidebarWizard, string>(nameof(NextButtonText), defaultValue: "Next");

    /// <summary>Defines the <see cref="BackButtonText"/> property.</summary>
    public static readonly StyledProperty<string> BackButtonTextProperty =
        AvaloniaProperty.Register<SidebarWizard, string>(nameof(BackButtonText), defaultValue: "Back");

    /// <summary>Defines the <see cref="CancelButtonText"/> property.</summary>
    public static readonly StyledProperty<string> CancelButtonTextProperty =
        AvaloniaProperty.Register<SidebarWizard, string>(nameof(CancelButtonText), defaultValue: "Cancel");

    /// <summary>Defines the <see cref="ShowCancelButton"/> property.</summary>
    public static readonly StyledProperty<bool> ShowCancelButtonProperty =
        AvaloniaProperty.Register<SidebarWizard, bool>(nameof(ShowCancelButton), defaultValue: true);

    /// <summary>Defines the <see cref="SidebarWidth"/> property.</summary>
    public static readonly StyledProperty<double> SidebarWidthProperty =
        AvaloniaProperty.Register<SidebarWizard, double>(nameof(SidebarWidth), defaultValue: 220);

    // ── Routed events ────────────────────────────────────────────────────────

    /// <summary>Raised when the user clicks Next on the last step.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> FinishedEvent =
        RoutedEvent.Register<SidebarWizard, RoutedEventArgs>(nameof(Finished), RoutingStrategies.Bubble);

    /// <summary>Raised when the user clicks Cancel.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> CancelledEvent =
        RoutedEvent.Register<SidebarWizard, RoutedEventArgs>(nameof(Cancelled), RoutingStrategies.Bubble);

    /// <summary>Raised whenever the active step changes.</summary>
    public static readonly RoutedEvent<SidebarWizardStepChangedEventArgs> StepChangedEvent =
        RoutedEvent.Register<SidebarWizard, SidebarWizardStepChangedEventArgs>(nameof(StepChanged), RoutingStrategies.Bubble);

    // ── CLR accessors ────────────────────────────────────────────────────────

    /// <summary>The ordered list of wizard steps.</summary>
    public IList<SidebarWizardStep> Steps
    {
        get
        {
            var list = GetValue(StepsProperty);
            if (list is null)
            {
                var avList = new AvaloniaList<SidebarWizardStep>();
                avList.CollectionChanged += OnStepsCollectionChanged;
                SetValue(StepsProperty, avList);
                return avList;
            }
            return list;
        }
        set
        {
            if (GetValue(StepsProperty) is AvaloniaList<SidebarWizardStep> old)
                old.CollectionChanged -= OnStepsCollectionChanged;

            if (value is AvaloniaList<SidebarWizardStep> newList)
                newList.CollectionChanged += OnStepsCollectionChanged;

            SetValue(StepsProperty, value);
        }
    }

    /// <summary>Zero-based index of the currently visible step.</summary>
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

    /// <summary>Optional system information shown at the bottom of the sidebar.</summary>
    public string? SystemInfo
    {
        get => GetValue(SystemInfoProperty);
        set => SetValue(SystemInfoProperty, value);
    }

    /// <summary>Status message shown below the content area.</summary>
    public string? StatusMessage
    {
        get => GetValue(StatusMessageProperty);
        set => SetValue(StatusMessageProperty, value);
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

    /// <summary>Width of the sidebar.</summary>
    public double SidebarWidth
    {
        get => GetValue(SidebarWidthProperty);
        set => SetValue(SidebarWidthProperty, value);
    }

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Raised when the user completes the last step.</summary>
    public event EventHandler<RoutedEventArgs>? Finished
    {
        add => AddHandler(FinishedEvent, value);
        remove => RemoveHandler(FinishedEvent, value);
    }

    /// <summary>Raised when the user cancels the wizard.</summary>
    public event EventHandler<RoutedEventArgs>? Cancelled
    {
        add => AddHandler(CancelledEvent, value);
        remove => RemoveHandler(CancelledEvent, value);
    }

    /// <summary>Raised when the active step changes.</summary>
    public event EventHandler<SidebarWizardStepChangedEventArgs>? StepChanged
    {
        add => AddHandler(StepChangedEvent, value);
        remove => RemoveHandler(StepChangedEvent, value);
    }

    // ── Computed helpers (used by bindings in the template) ──────────────────

    /// <summary>Progress value 0–100 based on current step.</summary>
    public static readonly DirectProperty<SidebarWizard, double> ProgressProperty =
        AvaloniaProperty.RegisterDirect<SidebarWizard, double>(nameof(Progress),
            o => o.Progress);

    /// <summary>The currently active step, or null.</summary>
    public static readonly DirectProperty<SidebarWizard, SidebarWizardStep?> CurrentStepProperty =
        AvaloniaProperty.RegisterDirect<SidebarWizard, SidebarWizardStep?>(nameof(CurrentStep),
            o => o.CurrentStep);

    /// <summary>Progress value 0–100 based on current step.</summary>
    public double Progress
    {
        get;
        private set => SetAndRaise(ProgressProperty, ref field, value);
    }

    /// <summary>The currently active step, or null.</summary>
    public SidebarWizardStep? CurrentStep
    {
        get;
        private set => SetAndRaise(CurrentStepProperty, ref field, value);
    }

    private void RefreshComputedProperties()
    {
        Progress = Steps.Count <= 1 ? 100 : (double)CurrentStepIndex / (Steps.Count - 1) * 100;
        CurrentStep = CurrentStepIndex >= 0 && CurrentStepIndex < Steps.Count
            ? Steps[CurrentStepIndex]
            : null;

        for (int i = 0; i < Steps.Count; i++)
        {
            Steps[i].StepNumber = i + 1;
            Steps[i].IsActive = i == CurrentStepIndex;
        }
    }

    // ── Static constructor ───────────────────────────────────────────────────

    static SidebarWizard()
    {
        CurrentStepIndexProperty.Changed.AddClassHandler<SidebarWizard, int>(
            (w, _) => w.OnStepIndexChanged());

        StepsProperty.Changed.AddClassHandler<SidebarWizard, IList<SidebarWizardStep>>(
            (w, _) => w.OnStepsReplaced());

        SystemInfoProperty.Changed.AddClassHandler<SidebarWizard, string?>(
            (w, e) => w.PseudoClasses.Set(PC_HasSystemInfo, e.NewValue is not null));

        StatusMessageProperty.Changed.AddClassHandler<SidebarWizard, string?>(
            (w, e) => w.PseudoClasses.Set(PC_HasStatus, e.NewValue is not null));
    }

    // ── Template ─────────────────────────────────────────────────────────────

    private Button? _backButton;
    private Button? _nextButton;
    private Button? _cancelButton;
    private ItemsControl? _stepsHost;
    private ContentPresenter? _contentPresenter;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_backButton is not null) _backButton.Click -= OnBackClicked;
        if (_nextButton is not null) _nextButton.Click -= OnNextClicked;
        if (_cancelButton is not null) _cancelButton.Click -= OnCancelClicked;

        _backButton = e.NameScope.Find<Button>(PART_BackButton);
        _nextButton = e.NameScope.Find<Button>(PART_NextButton);
        _cancelButton = e.NameScope.Find<Button>(PART_CancelButton);
        _stepsHost = e.NameScope.Find<ItemsControl>(PART_StepsHost);
        _contentPresenter = e.NameScope.Find<ContentPresenter>(PART_ContentPresenter);

        if (_backButton is not null) _backButton.Click += OnBackClicked;
        if (_nextButton is not null) _nextButton.Click += OnNextClicked;
        if (_cancelButton is not null) _cancelButton.Click += OnCancelClicked;

        if (_stepsHost is not null)
            _stepsHost.ItemsSource = Steps;

        Localizer.Instance.LocalizationChanged += OnLocalizationChanged;

        RefreshComputedProperties();
        UpdateButtonStates();
        UpdatePseudoClasses();
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

    private void OnBackClicked(object? sender, RoutedEventArgs e) => GoBack();
    private void OnNextClicked(object? sender, RoutedEventArgs e) => GoNext();
    private void OnCancelClicked(object? sender, RoutedEventArgs e) =>
        RaiseEvent(new RoutedEventArgs(CancelledEvent));

    private void OnStepsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is not IList<SidebarWizardStep> list) return;

        var seen = new HashSet<string?>();
        var toRemove = new List<SidebarWizardStep>();

        for (int i = list.Count - 1; i >= 0; i--)
        {
            var header = list[i].Header;
            if (!seen.Add(header))
                toRemove.Add(list[i]);
        }

        if (toRemove.Count > 0)
        {
            foreach (var step in toRemove)
                list.Remove(step);
        }

        RefreshComputedProperties();
        UpdateButtonStates();
    }

    private void OnStepIndexChanged()
    {
        RefreshComputedProperties();
        RaiseEvent(new SidebarWizardStepChangedEventArgs(StepChangedEvent, CurrentStepIndex, CurrentStep));
        UpdateButtonStates();
    }

    private void OnStepsReplaced()
    {
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
                ? Localizer.Instance.TryGetString("SidebarWizard/BtnFinish", out var finish) ? finish : "Finish"
                : Localizer.Instance.TryGetString("SidebarWizard/BtnNext", out var next) ? next : NextButtonText;
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(PC_HasSystemInfo, SystemInfo is not null);
        PseudoClasses.Set(PC_HasStatus, StatusMessage is not null);
    }
}

/// <summary>Event args for <see cref="SidebarWizard.StepChanged"/>.</summary>
public class SidebarWizardStepChangedEventArgs : RoutedEventArgs
{
    /// <summary>Zero-based index of the new active step.</summary>
    public int StepIndex { get; }

    /// <summary>The new active step, or null.</summary>
    public SidebarWizardStep? Step { get; }

    /// <summary>Initializes a new instance of <see cref="SidebarWizardStepChangedEventArgs"/>.</summary>
    public SidebarWizardStepChangedEventArgs(RoutedEvent routedEvent, int index, SidebarWizardStep? step)
        : base(routedEvent)
    {
        StepIndex = index;
        Step = step;
    }
}
