using Avalonia;
using Avalonia.Controls.Templates;

namespace PleasantUI.ToolKit.Controls;

/// <summary>
/// Represents a single step inside a <see cref="SidebarWizard"/>.
/// </summary>
public class SidebarWizardStep : AvaloniaObject
{
    /// <summary>Defines the <see cref="Header"/> property.</summary>
    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<SidebarWizardStep, string?>(nameof(Header));

    /// <summary>Defines the <see cref="Description"/> property.</summary>
    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<SidebarWizardStep, string?>(nameof(Description));

    /// <summary>Defines the <see cref="IsCompleted"/> property.</summary>
    public static readonly StyledProperty<bool> IsCompletedProperty =
        AvaloniaProperty.Register<SidebarWizardStep, bool>(nameof(IsCompleted));

    /// <summary>Defines the <see cref="IsActive"/> property.</summary>
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<SidebarWizardStep, bool>(nameof(IsActive));

    /// <summary>Defines the <see cref="StepNumber"/> property.</summary>
    public static readonly StyledProperty<int> StepNumberProperty =
        AvaloniaProperty.Register<SidebarWizardStep, int>(nameof(StepNumber));

    /// <summary>Defines the <see cref="Content"/> property.</summary>
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<SidebarWizardStep, object?>(nameof(Content));

    /// <summary>Defines the <see cref="ContentTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        AvaloniaProperty.Register<SidebarWizardStep, IDataTemplate?>(nameof(ContentTemplate));

    /// <summary>Gets or sets the step title shown in the sidebar.</summary>
    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>Gets or sets an optional description for the step.</summary>
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

    /// <summary>Gets or sets the body content for this step.</summary>
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>Gets or sets a data template used to render <see cref="Content"/>.</summary>
    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }
}
