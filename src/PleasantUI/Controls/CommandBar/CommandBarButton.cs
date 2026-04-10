using System.Diagnostics;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace PleasantUI.Controls;

/// <summary>
/// A button that can be placed in a <see cref="CommandBar"/> primary or secondary commands list.
/// Supports an icon, a text label, a keyboard shortcut hint, and an optional flyout.
/// </summary>
[PseudoClasses(PC_Icon, PC_Label, PC_Compact, PC_Overflow, PC_LabelRight, PC_LabelBottom, PC_LabelCollapsed, PC_Open)]
public class CommandBarButton : Button, ICommandBarElement
{
    // ── Pseudo-class constants ────────────────────────────────────────────────

    internal const string PC_Icon           = ":icon";
    internal const string PC_Label          = ":label";
    internal const string PC_Compact        = ":compact";
    internal const string PC_Overflow       = ":overflow";
    internal const string PC_LabelRight     = ":labelRight";
    internal const string PC_LabelBottom    = ":labelBottom";
    internal const string PC_LabelCollapsed = ":labelCollapsed";
    internal const string PC_Open           = ":open";

    // ── Styled properties ─────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<Geometry?> IconProperty =
        AvaloniaProperty.Register<CommandBarButton, Geometry?>(nameof(Icon));

    /// <summary>Defines the <see cref="Label"/> property.</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<CommandBarButton, string?>(nameof(Label));

    /// <summary>Defines the <see cref="KeyboardAcceleratorText"/> property.</summary>
    public static readonly StyledProperty<string?> KeyboardAcceleratorTextProperty =
        AvaloniaProperty.Register<CommandBarButton, string?>(nameof(KeyboardAcceleratorText));

    /// <summary>Defines the <see cref="IsCompact"/> property.</summary>
    public static readonly StyledProperty<bool> IsCompactProperty =
        AvaloniaProperty.Register<CommandBarButton, bool>(nameof(IsCompact));

    /// <summary>Defines the <see cref="IsInOverflow"/> direct property.</summary>
    public static readonly DirectProperty<CommandBarButton, bool> IsInOverflowProperty =
        AvaloniaProperty.RegisterDirect<CommandBarButton, bool>(nameof(IsInOverflow),
            o => o.IsInOverflow);

    /// <summary>Defines the <see cref="DynamicOverflowOrder"/> direct property.</summary>
    public static readonly DirectProperty<CommandBarButton, int> DynamicOverflowOrderProperty =
        AvaloniaProperty.RegisterDirect<CommandBarButton, int>(nameof(DynamicOverflowOrder),
            o => o.DynamicOverflowOrder, (o, v) => o.DynamicOverflowOrder = v);

    // ── CLR accessors ─────────────────────────────────────────────────────────

    /// <summary>Gets or sets the icon geometry displayed on the button.</summary>
    public Geometry? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Gets or sets the text label shown below or beside the icon.</summary>
    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Gets or sets the keyboard shortcut hint text shown in the overflow menu.</summary>
    public string? KeyboardAcceleratorText
    {
        get => GetValue(KeyboardAcceleratorTextProperty);
        set => SetValue(KeyboardAcceleratorTextProperty, value);
    }

    /// <summary>Gets or sets whether the button is shown with no label and reduced padding.</summary>
    public bool IsCompact
    {
        get => GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }

    /// <inheritdoc/>
    public bool IsInOverflow
    {
        get => _isInOverflow;
        internal set
        {
            if (SetAndRaise(IsInOverflowProperty, ref _isInOverflow, value))
                PseudoClasses.Set(PC_Overflow, value);
        }
    }

    /// <inheritdoc/>
    public int DynamicOverflowOrder
    {
        get => _dynamicOverflowOrder;
        set => SetAndRaise(DynamicOverflowOrderProperty, ref _dynamicOverflowOrder, value);
    }

    // ── Private fields ────────────────────────────────────────────────────────

    private bool _isInOverflow;
    private int  _dynamicOverflowOrder;

    // ── Overrides ─────────────────────────────────────────────────────────────

    protected override Type StyleKeyOverride => typeof(CommandBarButton);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IconProperty)
        {
            Debug.WriteLine($"[CommandBarButton] OnPropertyChanged - Icon changed, hasIcon={change.NewValue is not null}");
            PseudoClasses.Set(PC_Icon, change.NewValue is not null);
        }
        else if (change.Property == LabelProperty)
        {
            Debug.WriteLine($"[CommandBarButton] OnPropertyChanged - Label changed, hasLabel={change.NewValue is not null}");
            PseudoClasses.Set(PC_Label, change.NewValue is not null);
        }
        else if (change.Property == IsCompactProperty)
        {
            Debug.WriteLine($"[CommandBarButton] OnPropertyChanged - IsCompact changed to {change.GetNewValue<bool>()}");
            PseudoClasses.Set(PC_Compact, change.GetNewValue<bool>());
        }
    }

    protected override void OnClick()
    {
        Debug.WriteLine($"[CommandBarButton] OnClick - Label={Label}, IsInOverflow={IsInOverflow}");
        base.OnClick();

        // When clicked from the overflow menu, close the CommandBar.
        if (IsInOverflow)
        {
            Debug.WriteLine("[CommandBarButton] OnClick - Closing parent CommandBar");
            var bar = this.FindLogicalAncestorOfType<CommandBar>();
            if (bar is not null)
                bar.IsOpen = false;
        }
    }

    /// <summary>
    /// Sets the label-position pseudo-classes driven by the parent <see cref="CommandBar"/>.
    /// </summary>
    internal void ApplyLabelPosition(CommandBarDefaultLabelPosition pos)
    {
        Debug.WriteLine($"[CommandBarButton] ApplyLabelPosition - pos={pos}");
        PseudoClasses.Set(PC_LabelBottom,    pos == CommandBarDefaultLabelPosition.Bottom);
        PseudoClasses.Set(PC_LabelRight,     pos == CommandBarDefaultLabelPosition.Right);
        PseudoClasses.Set(PC_LabelCollapsed, pos == CommandBarDefaultLabelPosition.Collapsed);
    }

    /// <summary>Sets the :open pseudo-class driven by the parent <see cref="CommandBar"/>.</summary>
    internal void ApplyOpenState(bool open)
    {
        Debug.WriteLine($"[CommandBarButton] ApplyOpenState - open={open}");
        PseudoClasses.Set(PC_Open, open);
    }
}
