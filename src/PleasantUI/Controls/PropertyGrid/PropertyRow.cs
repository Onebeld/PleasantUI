using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;

namespace PleasantUI.Controls;

/// <summary>
/// Defines how the value of a <see cref="PropertyRow"/> is rendered.
/// </summary>
public enum PropertyRowValueKind
{
    /// <summary>Plain text label.</summary>
    Text,
    /// <summary>Clickable hyperlink-style button.</summary>
    Link,
    /// <summary>Colored status text (uses <see cref="PropertyRow.ValueBrush"/>).</summary>
    Status,
    /// <summary>Arbitrary content via <see cref="PropertyRow.ValueTemplate"/>.</summary>
    Custom
}

/// <summary>
/// Represents a single label/value row inside a <see cref="PropertyGrid"/>.
/// </summary>
[PseudoClasses(PC_Link, PC_Status, PC_Custom)]
public class PropertyRow : TemplatedControl
{
    // ── Pseudo-class names ────────────────────────────────────────────────────

    internal const string PC_Link   = ":link";
    internal const string PC_Status = ":status";
    internal const string PC_Custom = ":custom";

    // ── Styled properties ─────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="Label"/> property.</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<PropertyRow, string?>(nameof(Label));

    /// <summary>Defines the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<object?> ValueProperty =
        AvaloniaProperty.Register<PropertyRow, object?>(nameof(Value));

    /// <summary>Defines the <see cref="ValueTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> ValueTemplateProperty =
        AvaloniaProperty.Register<PropertyRow, IDataTemplate?>(nameof(ValueTemplate));

    /// <summary>Defines the <see cref="ValueKind"/> property.</summary>
    public static readonly StyledProperty<PropertyRowValueKind> ValueKindProperty =
        AvaloniaProperty.Register<PropertyRow, PropertyRowValueKind>(nameof(ValueKind));

    /// <summary>Defines the <see cref="ValueBrush"/> property.</summary>
    public static readonly StyledProperty<IBrush?> ValueBrushProperty =
        AvaloniaProperty.Register<PropertyRow, IBrush?>(nameof(ValueBrush));

    /// <summary>Defines the <see cref="LinkCommand"/> property.</summary>
    public static readonly StyledProperty<ICommand?> LinkCommandProperty =
        AvaloniaProperty.Register<PropertyRow, ICommand?>(nameof(LinkCommand));

    /// <summary>Defines the <see cref="LinkCommandParameter"/> property.</summary>
    public static readonly StyledProperty<object?> LinkCommandParameterProperty =
        AvaloniaProperty.Register<PropertyRow, object?>(nameof(LinkCommandParameter));

    /// <summary>Defines the <see cref="ToolTipText"/> property.</summary>
    public static readonly StyledProperty<string?> ToolTipTextProperty =
        AvaloniaProperty.Register<PropertyRow, string?>(nameof(ToolTipText));

    // ── CLR accessors ─────────────────────────────────────────────────────────

    /// <summary>Gets or sets the label shown in the left column.</summary>
    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Gets or sets the value shown in the right column.</summary>
    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>Gets or sets a data template used when <see cref="ValueKind"/> is <see cref="PropertyRowValueKind.Custom"/>.</summary>
    public IDataTemplate? ValueTemplate
    {
        get => GetValue(ValueTemplateProperty);
        set => SetValue(ValueTemplateProperty, value);
    }

    /// <summary>Gets or sets how the value is rendered.</summary>
    public PropertyRowValueKind ValueKind
    {
        get => GetValue(ValueKindProperty);
        set => SetValue(ValueKindProperty, value);
    }

    /// <summary>Gets or sets the brush used when <see cref="ValueKind"/> is <see cref="PropertyRowValueKind.Status"/>.</summary>
    public IBrush? ValueBrush
    {
        get => GetValue(ValueBrushProperty);
        set => SetValue(ValueBrushProperty, value);
    }

    /// <summary>Gets or sets the command invoked when the value is clicked (link kind).</summary>
    public ICommand? LinkCommand
    {
        get => GetValue(LinkCommandProperty);
        set => SetValue(LinkCommandProperty, value);
    }

    /// <summary>Gets or sets the parameter passed to <see cref="LinkCommand"/>.</summary>
    public object? LinkCommandParameter
    {
        get => GetValue(LinkCommandParameterProperty);
        set => SetValue(LinkCommandParameterProperty, value);
    }

    /// <summary>Gets or sets the tooltip text shown on the value.</summary>
    public string? ToolTipText
    {
        get => GetValue(ToolTipTextProperty);
        set => SetValue(ToolTipTextProperty, value);
    }

    // ── Overrides ─────────────────────────────────────────────────────────────

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ValueKindProperty)
        {
            var kind = change.GetNewValue<PropertyRowValueKind>();
            PseudoClasses.Set(PC_Link,   kind == PropertyRowValueKind.Link);
            PseudoClasses.Set(PC_Status, kind == PropertyRowValueKind.Status);
            PseudoClasses.Set(PC_Custom, kind == PropertyRowValueKind.Custom);
        }
    }
}
