using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace PleasantUI.Controls;

/// <summary>
/// A toggle button that can be placed in a <see cref="CommandBar"/> primary or secondary commands list.
/// Supports an icon, a text label, a keyboard shortcut hint, and checked/unchecked states.
/// </summary>
[PseudoClasses(CommandBarButton.PC_Icon, CommandBarButton.PC_Label, CommandBarButton.PC_Compact,
               CommandBarButton.PC_Overflow, CommandBarButton.PC_LabelRight,
               CommandBarButton.PC_LabelBottom, CommandBarButton.PC_LabelCollapsed,
               CommandBarButton.PC_Open)]
public class CommandBarToggleButton : ToggleButton, ICommandBarElement
{
    // ── Styled properties ─────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<Geometry?> IconProperty =
        CommandBarButton.IconProperty.AddOwner<CommandBarToggleButton>();

    /// <summary>Defines the <see cref="Label"/> property.</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        CommandBarButton.LabelProperty.AddOwner<CommandBarToggleButton>();

    /// <summary>Defines the <see cref="KeyboardAcceleratorText"/> property.</summary>
    public static readonly StyledProperty<string?> KeyboardAcceleratorTextProperty =
        CommandBarButton.KeyboardAcceleratorTextProperty.AddOwner<CommandBarToggleButton>();

    /// <summary>Defines the <see cref="IsCompact"/> property.</summary>
    public static readonly StyledProperty<bool> IsCompactProperty =
        CommandBarButton.IsCompactProperty.AddOwner<CommandBarToggleButton>();

    /// <summary>Defines the <see cref="IsInOverflow"/> direct property.</summary>
    public static readonly DirectProperty<CommandBarToggleButton, bool> IsInOverflowProperty =
        AvaloniaProperty.RegisterDirect<CommandBarToggleButton, bool>(nameof(IsInOverflow),
            o => o.IsInOverflow);

    /// <summary>Defines the <see cref="DynamicOverflowOrder"/> direct property.</summary>
    public static readonly DirectProperty<CommandBarToggleButton, int> DynamicOverflowOrderProperty =
        AvaloniaProperty.RegisterDirect<CommandBarToggleButton, int>(nameof(DynamicOverflowOrder),
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
                PseudoClasses.Set(CommandBarButton.PC_Overflow, value);
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

    protected override Type StyleKeyOverride => typeof(CommandBarToggleButton);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IconProperty)
        {
            Debug.WriteLine($"[CommandBarToggleButton] OnPropertyChanged - Icon changed, hasIcon={change.NewValue is not null}");
            PseudoClasses.Set(CommandBarButton.PC_Icon, change.NewValue is not null);
        }
        else if (change.Property == LabelProperty)
        {
            Debug.WriteLine($"[CommandBarToggleButton] OnPropertyChanged - Label changed, hasLabel={change.NewValue is not null}");
            PseudoClasses.Set(CommandBarButton.PC_Label, change.NewValue is not null);
        }
        else if (change.Property == IsCompactProperty)
        {
            Debug.WriteLine($"[CommandBarToggleButton] OnPropertyChanged - IsCompact changed to {change.GetNewValue<bool>()}");
            PseudoClasses.Set(CommandBarButton.PC_Compact, change.GetNewValue<bool>());
        }
    }

    protected override void OnClick()
    {
        Debug.WriteLine($"[CommandBarToggleButton] OnClick - Label={Label}, IsInOverflow={IsInOverflow}, IsChecked={IsChecked}");
        base.OnClick();

        if (IsInOverflow)
        {
            Debug.WriteLine("[CommandBarToggleButton] OnClick - Closing parent CommandBar");
            var bar = this.FindLogicalAncestorOfType<CommandBar>();
            if (bar is not null)
                bar.IsOpen = false;
        }
    }

    internal void ApplyLabelPosition(CommandBarDefaultLabelPosition pos)
    {
        Debug.WriteLine($"[CommandBarToggleButton] ApplyLabelPosition - pos={pos}");
        PseudoClasses.Set(CommandBarButton.PC_LabelBottom,    pos == CommandBarDefaultLabelPosition.Bottom);
        PseudoClasses.Set(CommandBarButton.PC_LabelRight,     pos == CommandBarDefaultLabelPosition.Right);
        PseudoClasses.Set(CommandBarButton.PC_LabelCollapsed, pos == CommandBarDefaultLabelPosition.Collapsed);
    }

    internal void ApplyOpenState(bool open)
    {
        Debug.WriteLine($"[CommandBarToggleButton] ApplyOpenState - open={open}");
        PseudoClasses.Set(CommandBarButton.PC_Open, open);
    }
}
