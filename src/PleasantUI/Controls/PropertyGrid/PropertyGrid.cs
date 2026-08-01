using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Metadata;

namespace PleasantUI.Controls;

/// <summary>
/// A scrollable two-column grid that displays a list of <see cref="PropertyRow"/> items.
/// The left column shows labels and the right column shows values — which can be plain text,
/// clickable links, colored status text, or arbitrary templated content.
/// </summary>
public class PropertyGrid : TemplatedControl
{
    /// <summary>Defines the <see cref="RowSpacing"/> property.</summary>
    public static readonly StyledProperty<double> RowSpacingProperty =
        AvaloniaProperty.Register<PropertyGrid, double>(nameof(RowSpacing), defaultValue: 10);
    
    /// <summary>Defines the <see cref="ValueHorizontalAlignment"/> property.</summary>
    public static readonly StyledProperty<HorizontalAlignment> ValueHorizontalAlignmentProperty =
        AvaloniaProperty.Register<PropertyGrid, HorizontalAlignment>(
            nameof(ValueHorizontalAlignment),
            defaultValue: HorizontalAlignment.Left,
            inherits: true); // Наследуется дочерними элементами

    /// <summary>Defines the <see cref="Rows"/> direct property.</summary>
    public static readonly DirectProperty<PropertyGrid, AvaloniaList<PropertyRow>> RowsProperty =
        AvaloniaProperty.RegisterDirect<PropertyGrid, AvaloniaList<PropertyRow>>(
            nameof(Rows), o => o.Rows);

    /// <summary>Gets or sets the vertical spacing between rows.</summary>
    public double RowSpacing
    {
        get => GetValue(RowSpacingProperty);
        set => SetValue(RowSpacingProperty, value);
    }
    
    /// <summary>Gets or sets the horizontal alignment of the row values.</summary>
    public HorizontalAlignment ValueHorizontalAlignment
    {
        get => GetValue(ValueHorizontalAlignmentProperty);
        set => SetValue(ValueHorizontalAlignmentProperty, value);
    }

    /// <summary>Gets the collection of property rows.</summary>
    [Content]
    public AvaloniaList<PropertyRow> Rows { get; } = [];
}