using Avalonia;
using Avalonia.Controls;

namespace PleasantUI.Controls;

/// <summary>
/// Custom panel that stacks <see cref="TimelineItem"/> children vertically and
/// synchronises their column widths so the axis line is perfectly aligned.
/// </summary>
public class TimelinePanel : Panel
{
    /// <summary>
    /// Defines the <see cref="Mode"/> property.
    /// </summary>
    public static readonly StyledProperty<TimelineDisplayMode> ModeProperty =
        Timeline.ModeProperty.AddOwner<TimelinePanel>();

    /// <summary>
    /// Gets or sets the display mode, forwarded from the parent <see cref="Timeline"/>.
    /// </summary>
    public TimelineDisplayMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    static TimelinePanel()
    {
        AffectsMeasure<TimelinePanel>(ModeProperty);
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        double left = 0, icon = 0, right = 0, height = 0;

        foreach (var child in Children)
        {
            child.Measure(availableSize);
            if (child is TimelineItem item)
            {
                var (l, m, r) = item.GetColumnWidths();
                left  = Math.Max(left,  l);
                icon  = Math.Max(icon,  m);
                right = Math.Max(right, r);
            }
            height += child.DesiredSize.Height;
        }

        return new Size(left + icon + right, height);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        double left = 0, icon = 0, right = 0, height = 0;

        foreach (var child in Children)
        {
            if (child is TimelineItem item)
            {
                var (l, m, r) = item.GetColumnWidths();
                left  = Math.Max(left,  l);
                icon  = Math.Max(icon,  m);
                right = Math.Max(right, r);
            }
        }

        var rect = new Rect(0, 0, left + icon + right, 0);

        foreach (var child in Children)
        {
            if (child is TimelineItem item)
            {
                item.SetColumnWidths(left, icon, right);
                item.InvalidateArrange();
            }

            rect = rect.WithHeight(child.DesiredSize.Height);
            child.Arrange(rect);
            rect = rect.WithY(rect.Y + child.DesiredSize.Height);
            height += child.DesiredSize.Height;
        }

        return new Size(left + icon + right, height);
    }
}
