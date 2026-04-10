using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;

namespace PleasantUI.Controls;

/// <summary>
/// A scrollable two-column grid that displays a list of <see cref="PropertyRow"/> items.
/// The left column shows labels and the right column shows values — which can be plain text,
/// clickable links, colored status text, or arbitrary templated content.
/// </summary>
[TemplatePart(PART_RowsHost, typeof(ItemsControl))]
public class PropertyGrid : TemplatedControl
{
    // ── Template part names ───────────────────────────────────────────────────

    internal const string PART_RowsHost = "PART_RowsHost";

    // ── Styled properties ─────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="LabelColumnWidth"/> property.</summary>
    public static readonly StyledProperty<GridLength> LabelColumnWidthProperty =
        AvaloniaProperty.Register<PropertyGrid, GridLength>(nameof(LabelColumnWidth),
            defaultValue: GridLength.Auto);

    /// <summary>Defines the <see cref="RowSpacing"/> property.</summary>
    public static readonly StyledProperty<double> RowSpacingProperty =
        AvaloniaProperty.Register<PropertyGrid, double>(nameof(RowSpacing), defaultValue: 10);

    // ── Direct properties ─────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="Rows"/> direct property.</summary>
    public static readonly DirectProperty<PropertyGrid, AvaloniaList<PropertyRow>> RowsProperty =
        AvaloniaProperty.RegisterDirect<PropertyGrid, AvaloniaList<PropertyRow>>(
            nameof(Rows), o => o.Rows);

    // ── CLR accessors ─────────────────────────────────────────────────────────

    /// <summary>Gets or sets the width of the label column.</summary>
    public GridLength LabelColumnWidth
    {
        get => GetValue(LabelColumnWidthProperty);
        set => SetValue(LabelColumnWidthProperty, value);
    }

    /// <summary>Gets or sets the vertical spacing between rows.</summary>
    public double RowSpacing
    {
        get => GetValue(RowSpacingProperty);
        set => SetValue(RowSpacingProperty, value);
    }

    /// <summary>Gets the collection of property rows.</summary>
    [Content]
    public AvaloniaList<PropertyRow> Rows { get; } = new();

    // ── Private state ─────────────────────────────────────────────────────────

    private ItemsControl? _rowsHost;

    // ── Constructor ───────────────────────────────────────────────────────────

    public PropertyGrid()
    {
        Rows.CollectionChanged += OnRowsChanged;
    }

    // ── Template ──────────────────────────────────────────────────────────────

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _rowsHost = e.NameScope.Find<ItemsControl>(PART_RowsHost);

        if (_rowsHost is not null)
            _rowsHost.ItemsSource = Rows;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void OnRowsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_rowsHost is not null)
            _rowsHost.ItemsSource = Rows;
    }
}
