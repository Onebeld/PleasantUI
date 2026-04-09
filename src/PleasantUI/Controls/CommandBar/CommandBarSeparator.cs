using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace PleasantUI.Controls;

/// <summary>
/// A visual separator between items in a <see cref="CommandBar"/>.
/// Renders vertically when inline and horizontally when in the overflow menu.
/// </summary>
[PseudoClasses(CommandBarButton.PC_Overflow)]
public class CommandBarSeparator : TemplatedControl, ICommandBarElement
{
    /// <summary>Defines the <see cref="IsInOverflow"/> direct property.</summary>
    public static readonly DirectProperty<CommandBarSeparator, bool> IsInOverflowProperty =
        AvaloniaProperty.RegisterDirect<CommandBarSeparator, bool>(nameof(IsInOverflow),
            o => o.IsInOverflow);

    /// <summary>Defines the <see cref="DynamicOverflowOrder"/> direct property.</summary>
    public static readonly DirectProperty<CommandBarSeparator, int> DynamicOverflowOrderProperty =
        AvaloniaProperty.RegisterDirect<CommandBarSeparator, int>(nameof(DynamicOverflowOrder),
            o => o.DynamicOverflowOrder, (o, v) => o.DynamicOverflowOrder = v);

    /// <summary>Defines the <see cref="IsCompact"/> property.</summary>
    public static readonly StyledProperty<bool> IsCompactProperty =
        AvaloniaProperty.Register<CommandBarSeparator, bool>(nameof(IsCompact));

    /// <inheritdoc/>
    public bool IsInOverflow
    {
        get => _isInOverflow;
        internal set
        {
            Debug.WriteLine($"[CommandBarSeparator] IsInOverflow set to {value}");
            if (SetAndRaise(IsInOverflowProperty, ref _isInOverflow, value))
                PseudoClasses.Set(CommandBarButton.PC_Overflow, value);
        }
    }

    /// <inheritdoc/>
    public int DynamicOverflowOrder
    {
        get => _dynamicOverflowOrder;
        set
        {
            Debug.WriteLine($"[CommandBarSeparator] DynamicOverflowOrder set to {value}");
            SetAndRaise(DynamicOverflowOrderProperty, ref _dynamicOverflowOrder, value);
        }
    }

    /// <inheritdoc/>
    public bool IsCompact
    {
        get => GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }

    private bool _isInOverflow;
    private int  _dynamicOverflowOrder;
}
