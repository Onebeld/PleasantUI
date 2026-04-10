using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

namespace PleasantUI.ToolKit.Controls;

/// <summary>
/// Represents a single numbered step inside a <see cref="StepDialog"/>.
/// </summary>
[PseudoClasses(PC_Completed, PC_Active)]
public class StepItem : HeaderedContentControl
{
    // ── Pseudo-class names ────────────────────────────────────────────────────

    internal const string PC_Completed = ":completed";
    internal const string PC_Active    = ":active";

    // ── Styled properties ─────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="StepNumber"/> property.</summary>
    public static readonly StyledProperty<int> StepNumberProperty =
        AvaloniaProperty.Register<StepItem, int>(nameof(StepNumber));

    /// <summary>Defines the <see cref="IsCompleted"/> property.</summary>
    public static readonly StyledProperty<bool> IsCompletedProperty =
        AvaloniaProperty.Register<StepItem, bool>(nameof(IsCompleted));

    /// <summary>Defines the <see cref="IsActive"/> property.</summary>
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<StepItem, bool>(nameof(IsActive));

    /// <summary>Defines the <see cref="Description"/> property.</summary>
    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<StepItem, string?>(nameof(Description));

    // ── CLR accessors ─────────────────────────────────────────────────────────

    /// <summary>Gets or sets the 1-based step number shown in the badge.</summary>
    public int StepNumber
    {
        get => GetValue(StepNumberProperty);
        set => SetValue(StepNumberProperty, value);
    }

    /// <summary>Gets or sets whether this step has been completed.</summary>
    public bool IsCompleted
    {
        get => GetValue(IsCompletedProperty);
        set => SetValue(IsCompletedProperty, value);
    }

    /// <summary>Gets or sets whether this step is the currently active one.</summary>
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    /// <summary>Gets or sets an optional description shown below the header.</summary>
    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    // ── Overrides ─────────────────────────────────────────────────────────────

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsCompletedProperty)
            PseudoClasses.Set(PC_Completed, change.GetNewValue<bool>());
        else if (change.Property == IsActiveProperty)
            PseudoClasses.Set(PC_Active, change.GetNewValue<bool>());
    }
}
