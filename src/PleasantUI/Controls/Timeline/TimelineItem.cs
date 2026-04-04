using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

namespace PleasantUI.Controls;

/// <summary>
/// Represents a single entry in a <see cref="Timeline"/>.
/// </summary>
[PseudoClasses(PC_First, PC_Last, PC_EmptyIcon, PC_AllLeft, PC_AllRight, PC_Separate)]
[TemplatePart(PART_Header,   typeof(ContentPresenter))]
[TemplatePart(PART_Icon,     typeof(Panel))]
[TemplatePart(PART_Content,  typeof(ContentPresenter))]
[TemplatePart(PART_Time,     typeof(TextBlock))]
[TemplatePart(PART_RootGrid, typeof(Grid))]
public class TimelineItem : HeaderedContentControl
{
    /// <summary>Pseudo-class applied to the first item in the timeline.</summary>
    public const string PC_First     = ":first";
    /// <summary>Pseudo-class applied to the last item in the timeline.</summary>
    public const string PC_Last      = ":last";
    /// <summary>Pseudo-class applied when no custom icon is set.</summary>
    public const string PC_EmptyIcon = ":empty-icon";
    /// <summary>Pseudo-class applied when position is <see cref="TimelineItemPosition.Left"/>.</summary>
    public const string PC_AllLeft   = ":all-left";
    /// <summary>Pseudo-class applied when position is <see cref="TimelineItemPosition.Right"/>.</summary>
    public const string PC_AllRight  = ":all-right";
    /// <summary>Pseudo-class applied when position is <see cref="TimelineItemPosition.Separate"/>.</summary>
    public const string PC_Separate  = ":separate";

    /// <summary>Template part name for the header presenter.</summary>
    public const string PART_Header   = "PART_Header";
    /// <summary>Template part name for the icon panel.</summary>
    public const string PART_Icon     = "PART_Icon";
    /// <summary>Template part name for the content presenter.</summary>
    public const string PART_Content  = "PART_Content";
    /// <summary>Template part name for the time text block.</summary>
    public const string PART_Time     = "PART_Time";
    /// <summary>Template part name for the root grid.</summary>
    public const string PART_RootGrid = "PART_RootGrid";

    private ContentPresenter? _headerPresenter;
    private Panel?            _iconPanel;
    private ContentPresenter? _contentPresenter;
    private TextBlock?        _timePresenter;
    private Grid?             _rootGrid;

    // ── Properties ──────────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<TimelineItem, object?>(nameof(Icon));

    /// <summary>Defines the <see cref="IconTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> IconTemplateProperty =
        AvaloniaProperty.Register<TimelineItem, IDataTemplate?>(nameof(IconTemplate));

    /// <summary>Defines the <see cref="Type"/> property.</summary>
    public static readonly StyledProperty<TimelineItemType> TypeProperty =
        AvaloniaProperty.Register<TimelineItem, TimelineItemType>(nameof(Type));

    /// <summary>Defines the <see cref="Position"/> property.</summary>
    public static readonly StyledProperty<TimelineItemPosition> PositionProperty =
        AvaloniaProperty.Register<TimelineItem, TimelineItemPosition>(nameof(Position),
            defaultValue: TimelineItemPosition.Left);

    /// <summary>Defines the <see cref="Time"/> property.</summary>
    public static readonly StyledProperty<DateTime> TimeProperty =
        AvaloniaProperty.Register<TimelineItem, DateTime>(nameof(Time));

    /// <summary>Defines the <see cref="TimeFormat"/> property.</summary>
    public static readonly StyledProperty<string?> TimeFormatProperty =
        AvaloniaProperty.Register<TimelineItem, string?>(nameof(TimeFormat));

    // Direct properties used by TimelinePanel for column-width synchronisation.
    private double _leftWidth, _iconWidth, _rightWidth;

    /// <summary>Defines the <see cref="LeftWidth"/> property.</summary>
    public static readonly DirectProperty<TimelineItem, double> LeftWidthProperty =
        AvaloniaProperty.RegisterDirect<TimelineItem, double>(nameof(LeftWidth),
            o => o.LeftWidth, (o, v) => o.LeftWidth = v);

    /// <summary>Defines the <see cref="IconWidth"/> property.</summary>
    public static readonly DirectProperty<TimelineItem, double> IconWidthProperty =
        AvaloniaProperty.RegisterDirect<TimelineItem, double>(nameof(IconWidth),
            o => o.IconWidth, (o, v) => o.IconWidth = v);

    /// <summary>Defines the <see cref="RightWidth"/> property.</summary>
    public static readonly DirectProperty<TimelineItem, double> RightWidthProperty =
        AvaloniaProperty.RegisterDirect<TimelineItem, double>(nameof(RightWidth),
            o => o.RightWidth, (o, v) => o.RightWidth = v);

    // ── CLR accessors ────────────────────────────────────────────────────────

    /// <summary>Gets or sets the icon content displayed on the axis node.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Gets or sets the data template used to display <see cref="Icon"/>.</summary>
    public IDataTemplate? IconTemplate
    {
        get => GetValue(IconTemplateProperty);
        set => SetValue(IconTemplateProperty, value);
    }

    /// <summary>Gets or sets the visual type / severity of this item.</summary>
    public TimelineItemType Type
    {
        get => GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    /// <summary>Gets or sets the position of this item relative to the axis.</summary>
    public TimelineItemPosition Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    /// <summary>Gets or sets the timestamp associated with this item.</summary>
    public DateTime Time
    {
        get => GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    /// <summary>Gets or sets the format string used to display <see cref="Time"/>.</summary>
    public string? TimeFormat
    {
        get => GetValue(TimeFormatProperty);
        set => SetValue(TimeFormatProperty, value);
    }

    /// <summary>Width of the left column (time or content depending on position).</summary>
    public double LeftWidth
    {
        get => _leftWidth;
        set => SetAndRaise(LeftWidthProperty, ref _leftWidth, value);
    }

    /// <summary>Width of the centre column (icon node).</summary>
    public double IconWidth
    {
        get => _iconWidth;
        set => SetAndRaise(IconWidthProperty, ref _iconWidth, value);
    }

    /// <summary>Width of the right column.</summary>
    public double RightWidth
    {
        get => _rightWidth;
        set => SetAndRaise(RightWidthProperty, ref _rightWidth, value);
    }

    // ── Static constructor ───────────────────────────────────────────────────

    static TimelineItem()
    {
        IconProperty.Changed.AddClassHandler<TimelineItem, object?>(
            (item, args) => item.OnIconChanged(args));

        PositionProperty.Changed.AddClassHandler<TimelineItem, TimelineItemPosition>(
            (item, args) => item.ApplyPositionPseudoClasses(args.NewValue.Value));

        AffectsMeasure<TimelineItem>(LeftWidthProperty, IconWidthProperty, RightWidthProperty);
    }

    // ── Template ─────────────────────────────────────────────────────────────

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _rootGrid        = e.NameScope.Find<Grid>(PART_RootGrid);
        _headerPresenter = e.NameScope.Find<ContentPresenter>(PART_Header);
        _iconPanel       = e.NameScope.Find<Panel>(PART_Icon);
        _contentPresenter = e.NameScope.Find<ContentPresenter>(PART_Content);
        _timePresenter   = e.NameScope.Find<TextBlock>(PART_Time);

        PseudoClasses.Set(PC_EmptyIcon, Icon is null);
        ApplyPositionPseudoClasses(Position);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        (Parent as Timeline)?.InvalidateContainers();
    }

    // ── Internal API used by Timeline / TimelinePanel ────────────────────────

    /// <summary>
    /// Marks this item as first, last, or neither in the list.
    /// </summary>
    internal void SetEndFlags(bool isFirst, bool isLast)
    {
        PseudoClasses.Set(PC_First, isFirst);
        PseudoClasses.Set(PC_Last,  isLast);
    }

    /// <summary>
    /// Returns the desired (left, icon, right) column widths based on current position and content.
    /// </summary>
    internal (double left, double icon, double right) GetColumnWidths()
    {
        double header  = _headerPresenter?.DesiredSize.Width  ?? 0;
        double icon    = _iconPanel?.DesiredSize.Width        ?? 0;
        double content = _contentPresenter?.DesiredSize.Width ?? 0;
        double time    = _timePresenter?.DesiredSize.Width    ?? 0;

        double contentMax = Math.Max(header, content);

        return Position switch
        {
            TimelineItemPosition.Left     => (0, icon, Math.Max(contentMax, time)),
            TimelineItemPosition.Right    => (Math.Max(contentMax, time), icon, 0),
            TimelineItemPosition.Separate => (time, icon, contentMax),
            _                             => (0, 0, 0),
        };
    }

    /// <summary>
    /// Applies the column widths resolved by <see cref="TimelinePanel"/>.
    /// </summary>
    internal void SetColumnWidths(double left, double icon, double right)
    {
        if (_rootGrid is null) return;
        _rootGrid.ColumnDefinitions[0].Width = new GridLength(left);
        _rootGrid.ColumnDefinitions[1].Width = new GridLength(icon);
        _rootGrid.ColumnDefinitions[2].Width = new GridLength(right);
    }

    /// <summary>
    /// Sets a property only if it has not been explicitly set by the user.
    /// </summary>
    internal void SetIfUnset<T>(AvaloniaProperty<T> property, T value)
    {
        if (!IsSet(property))
            SetCurrentValue(property, value);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private void OnIconChanged(AvaloniaPropertyChangedEventArgs<object?> args)
    {
        PseudoClasses.Set(PC_EmptyIcon, args.NewValue.Value is null);
    }

    private void ApplyPositionPseudoClasses(TimelineItemPosition position)
    {
        PseudoClasses.Set(PC_AllLeft,  position == TimelineItemPosition.Left);
        PseudoClasses.Set(PC_AllRight, position == TimelineItemPosition.Right);
        PseudoClasses.Set(PC_Separate, position == TimelineItemPosition.Separate);
    }
}
