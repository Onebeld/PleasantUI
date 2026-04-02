using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace PleasantUI.Controls;

/// <summary>
/// Displays a vertical sequence of events along a connecting axis line.
/// </summary>
public class Timeline : ItemsControl
{
    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new TimelinePanel());

    // ── Binding-forwarding properties ────────────────────────────────────────

    /// <summary>Defines the <see cref="IconMemberBinding"/> property.</summary>
    public static readonly StyledProperty<BindingBase?> IconMemberBindingProperty =
        AvaloniaProperty.Register<Timeline, BindingBase?>(nameof(IconMemberBinding));

    /// <summary>Defines the <see cref="HeaderMemberBinding"/> property.</summary>
    public static readonly StyledProperty<BindingBase?> HeaderMemberBindingProperty =
        AvaloniaProperty.Register<Timeline, BindingBase?>(nameof(HeaderMemberBinding));

    /// <summary>Defines the <see cref="ContentMemberBinding"/> property.</summary>
    public static readonly StyledProperty<BindingBase?> ContentMemberBindingProperty =
        AvaloniaProperty.Register<Timeline, BindingBase?>(nameof(ContentMemberBinding));

    /// <summary>Defines the <see cref="TimeMemberBinding"/> property.</summary>
    public static readonly StyledProperty<BindingBase?> TimeMemberBindingProperty =
        AvaloniaProperty.Register<Timeline, BindingBase?>(nameof(TimeMemberBinding));

    // ── Template properties ──────────────────────────────────────────────────

    /// <summary>Defines the <see cref="IconTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> IconTemplateProperty =
        AvaloniaProperty.Register<Timeline, IDataTemplate?>(nameof(IconTemplate));

    /// <summary>Defines the <see cref="DescriptionTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> DescriptionTemplateProperty =
        AvaloniaProperty.Register<Timeline, IDataTemplate?>(nameof(DescriptionTemplate));

    // ── Display properties ───────────────────────────────────────────────────

    /// <summary>Defines the <see cref="TimeFormat"/> property.</summary>
    public static readonly StyledProperty<string?> TimeFormatProperty =
        AvaloniaProperty.Register<Timeline, string?>(nameof(TimeFormat),
            defaultValue: "yyyy-MM-dd HH:mm:ss");

    /// <summary>Defines the <see cref="Mode"/> property.</summary>
    public static readonly StyledProperty<TimelineDisplayMode> ModeProperty =
        AvaloniaProperty.Register<Timeline, TimelineDisplayMode>(nameof(Mode));

    // ── CLR accessors ────────────────────────────────────────────────────────

    /// <summary>Binding path for the icon of each item when using <see cref="ItemsControl.ItemsSource"/>.</summary>
    [AssignBinding]
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public BindingBase? IconMemberBinding
    {
        get => GetValue(IconMemberBindingProperty);
        set => SetValue(IconMemberBindingProperty, value);
    }

    /// <summary>Binding path for the header of each item when using <see cref="ItemsControl.ItemsSource"/>.</summary>
    [AssignBinding]
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public BindingBase? HeaderMemberBinding
    {
        get => GetValue(HeaderMemberBindingProperty);
        set => SetValue(HeaderMemberBindingProperty, value);
    }

    /// <summary>Binding path for the content of each item when using <see cref="ItemsControl.ItemsSource"/>.</summary>
    [AssignBinding]
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public BindingBase? ContentMemberBinding
    {
        get => GetValue(ContentMemberBindingProperty);
        set => SetValue(ContentMemberBindingProperty, value);
    }

    /// <summary>Binding path for the timestamp of each item when using <see cref="ItemsControl.ItemsSource"/>.</summary>
    [AssignBinding]
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public BindingBase? TimeMemberBinding
    {
        get => GetValue(TimeMemberBindingProperty);
        set => SetValue(TimeMemberBindingProperty, value);
    }

    /// <summary>Data template used to display the icon node of each item.</summary>
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? IconTemplate
    {
        get => GetValue(IconTemplateProperty);
        set => SetValue(IconTemplateProperty, value);
    }

    /// <summary>Data template used to display the description/content of each item.</summary>
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? DescriptionTemplate
    {
        get => GetValue(DescriptionTemplateProperty);
        set => SetValue(DescriptionTemplateProperty, value);
    }

    /// <summary>Format string applied to <see cref="TimelineItem.Time"/>.</summary>
    public string? TimeFormat
    {
        get => GetValue(TimeFormatProperty);
        set => SetValue(TimeFormatProperty, value);
    }

    /// <summary>Controls how items are positioned relative to the axis line.</summary>
    public TimelineDisplayMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    // ── Static constructor ───────────────────────────────────────────────────

    static Timeline()
    {
        ItemsPanelProperty.OverrideDefaultValue<Timeline>(DefaultPanel);
        ModeProperty.Changed.AddClassHandler<Timeline, TimelineDisplayMode>(
            (t, e) => t.OnDisplayModeChanged(e));
    }

    // ── Container overrides ──────────────────────────────────────────────────

    /// <inheritdoc />
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        recycleKey = null;
        return item is not TimelineItem;
    }

    /// <inheritdoc />
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new TimelineItem();

    /// <inheritdoc />
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);

        if (container is not TimelineItem timelineItem) return;

        timelineItem.SetEndFlags(index == 0, index == ItemCount - 1);

        if (IconMemberBinding is not null)
            timelineItem.Bind(TimelineItem.IconProperty, IconMemberBinding);

        if (HeaderMemberBinding is not null)
            timelineItem.Bind(HeaderedContentControl.HeaderProperty, HeaderMemberBinding);

        if (ContentMemberBinding is not null)
            timelineItem.Bind(ContentControl.ContentProperty, ContentMemberBinding);

        if (TimeMemberBinding is not null)
            timelineItem.Bind(TimelineItem.TimeProperty, TimeMemberBinding);

        timelineItem.SetIfUnset(TimelineItem.TimeFormatProperty, TimeFormat);
        timelineItem.SetIfUnset(TimelineItem.IconTemplateProperty, IconTemplate);
        timelineItem.SetIfUnset(HeaderedContentControl.HeaderTemplateProperty, ItemTemplate);
        timelineItem.SetIfUnset(ContentControl.ContentTemplateProperty, DescriptionTemplate);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (ItemsPanelRoot is TimelinePanel panel)
            panel.Mode = Mode;

        ApplyItemPositions();
        return base.ArrangeOverride(finalSize);
    }

    // ── Internal API ─────────────────────────────────────────────────────────

    /// <summary>
    /// Re-evaluates first/last flags and position pseudo-classes for all realised containers.
    /// Called by <see cref="TimelineItem"/> when it attaches to the visual tree.
    /// </summary>
    internal void InvalidateContainers()
    {
        var items = this.GetVisualDescendants().OfType<TimelineItem>().ToList();
        for (int i = 0; i < items.Count; i++)
            items[i].SetEndFlags(i == 0, i == items.Count - 1);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private void OnDisplayModeChanged(AvaloniaPropertyChangedEventArgs<TimelineDisplayMode> e)
    {
        if (ItemsPanelRoot is TimelinePanel panel)
            panel.Mode = e.NewValue.Value;

        // Force a full layout pass so ArrangeOverride re-applies positions
        // with the new mode and TimelinePanel re-synchronises column widths.
        InvalidateMeasure();
        InvalidateArrange();
    }

    private void ApplyItemPositions()
    {
        if (ItemsPanelRoot is not TimelinePanel panel) return;

        bool alternate = false;
        foreach (var item in panel.Children.OfType<TimelineItem>())
        {
            TimelineItemPosition pos = Mode switch
            {
                TimelineDisplayMode.Left      => TimelineItemPosition.Left,
                TimelineDisplayMode.Right     => TimelineItemPosition.Right,
                TimelineDisplayMode.Center    => TimelineItemPosition.Separate,
                TimelineDisplayMode.Alternate => alternate
                    ? TimelineItemPosition.Right
                    : TimelineItemPosition.Left,
                _ => TimelineItemPosition.Left,
            };

            // Always overwrite — mode changes must override any previously computed position.
            item.SetCurrentValue(TimelineItem.PositionProperty, pos);

            if (Mode == TimelineDisplayMode.Alternate)
                alternate = !alternate;
        }
    }

    private static void SetIfUnset<T>(AvaloniaObject target, StyledProperty<T> property, T value)
    {
        if (!target.IsSet(property))
            target.SetCurrentValue(property, value);
    }
}
