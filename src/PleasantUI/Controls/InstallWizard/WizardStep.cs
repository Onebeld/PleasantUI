using Avalonia;
using Avalonia.Controls.Templates;

namespace PleasantUI.Controls;

/// <summary>
/// Completion state shown in the sidebar indicator when a step is marked completed.
/// </summary>
public enum WizardStepCompletionState
{
    /// <summary>Green checkmark — step completed successfully.</summary>
    Success,
    /// <summary>Yellow/orange warning icon — step completed with warnings.</summary>
    Warning,
    /// <summary>Red error icon — step completed with errors.</summary>
    Error
}

/// <summary>
/// Represents a single step inside an <see cref="InstallWizard"/>.
/// Acts as a lightweight data+content holder — the wizard template
/// renders each step's content directly, avoiding double-parenting.
/// </summary>
public class WizardStep : AvaloniaObject
{
    /// <summary>Defines the <see cref="Header"/> property.</summary>
    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<WizardStep, string?>(nameof(Header));

    /// <summary>Defines the <see cref="Description"/> property.</summary>
    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<WizardStep, string?>(nameof(Description));

    /// <summary>Defines the <see cref="IsCompleted"/> property.</summary>
    public static readonly StyledProperty<bool> IsCompletedProperty =
        AvaloniaProperty.Register<WizardStep, bool>(nameof(IsCompleted));

    /// <summary>Defines the <see cref="CompletionState"/> property.</summary>
    public static readonly StyledProperty<WizardStepCompletionState> CompletionStateProperty =
        AvaloniaProperty.Register<WizardStep, WizardStepCompletionState>(
            nameof(CompletionState), defaultValue: WizardStepCompletionState.Success);

    /// <summary>Defines the <see cref="IsActive"/> property.</summary>
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<WizardStep, bool>(nameof(IsActive));

    /// <summary>Defines the <see cref="StepNumber"/> property.</summary>
    public static readonly StyledProperty<int> StepNumberProperty =
        AvaloniaProperty.Register<WizardStep, int>(nameof(StepNumber));

    /// <summary>Defines the <see cref="Content"/> property.</summary>
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<WizardStep, object?>(nameof(Content));

    /// <summary>Defines the <see cref="ContentTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        AvaloniaProperty.Register<WizardStep, IDataTemplate?>(nameof(ContentTemplate));

    /// <summary>Gets or sets the step title shown in the sidebar and content header.</summary>
    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>Gets or sets an optional subtitle shown below the header in the content area.</summary>
    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>Gets or sets whether this step has been completed.</summary>
    public bool IsCompleted
    {
        get => GetValue(IsCompletedProperty);
        set => SetValue(IsCompletedProperty, value);
    }

    /// <summary>
    /// Gets or sets the visual state of the completion indicator.
    /// Only relevant when <see cref="IsCompleted"/> is <c>true</c>.
    /// </summary>
    public WizardStepCompletionState CompletionState
    {
        get => GetValue(CompletionStateProperty);
        set => SetValue(CompletionStateProperty, value);
    }

    /// <summary>Gets or sets whether this is the currently visible step.</summary>
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    /// <summary>Gets or sets the 1-based display number shown in the sidebar.</summary>
    public int StepNumber
    {
        get => GetValue(StepNumberProperty);
        set => SetValue(StepNumberProperty, value);
    }

    /// <summary>
    /// Gets or sets the body content for this step.
    /// Use <see cref="ContentTemplate"/> with a plain data object instead of
    /// setting this to a Control directly, to avoid double-parenting on reinit.
    /// </summary>
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Gets or sets a data template used to render <see cref="Content"/>.
    /// Preferred over setting <see cref="Content"/> to a Control directly.
    /// </summary>
    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }
}
