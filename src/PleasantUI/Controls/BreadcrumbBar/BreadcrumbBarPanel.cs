using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace PleasantUI.Controls;

/// <summary>
/// Custom panel used inside <see cref="BreadcrumbBar"/>.
/// Measures all children, then arranges as many as fit from the right side.
/// When items overflow the available width the ellipsis item (index 0) is shown
/// and items that don't fit are hidden behind it.
/// </summary>
internal sealed class BreadcrumbBarPanel : Panel
{
    // ── State queried by BreadcrumbBar after each layout pass ─────────────────

    /// <summary>Whether the ellipsis item is currently visible.</summary>
    public bool EllipsisIsRendered { get; private set; }

    /// <summary>
    /// Index (within the panel's Children collection) of the first item rendered
    /// after the ellipsis. Meaningful only when <see cref="EllipsisIsRendered"/> is true.
    /// </summary>
    public int FirstRenderedItemIndexAfterEllipsis { get; private set; }

    /// <summary>Number of visible (non-ellipsis) items after the last arrange pass.</summary>
    public int VisibleItemsCount { get; private set; }

    // ── Layout ────────────────────────────────────────────────────────────────

    protected override Size MeasureOverride(Size availableSize)
    {
        double accumWidth  = 0;
        double accumHeight = 0;

        int count = Children.Count;
        for (int i = 0; i < count; i++)
        {
            var child = Children[i];
            child.Measure(availableSize);

            // Index 0 is always the ellipsis item — don't add its width to the total.
            if (i != 0)
            {
                accumWidth  += child.DesiredSize.Width;
                accumHeight  = Math.Max(accumHeight, child.DesiredSize.Height);
            }
        }

        EllipsisIsRendered = accumWidth > availableSize.Width;

        return new Size(accumWidth, accumHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        int count = Children.Count;

        FirstRenderedItemIndexAfterEllipsis = count > 0 ? count - 1 : 0;
        VisibleItemsCount = 0;

        int firstToRender = 0;

        if (EllipsisIsRendered && count > 0)
        {
            firstToRender = GetFirstItemIndexToArrange(finalSize.Width);
            FirstRenderedItemIndexAfterEllipsis = firstToRender;
        }

        double maxHeight = GetMaxHeight(firstToRender);
        double accumX    = 0;

        // Ellipsis item (index 0)
        if (count > 0)
        {
            var ellipsis = Children[0];
            if (EllipsisIsRendered)
            {
                ellipsis.Arrange(new Rect(accumX, 0, ellipsis.DesiredSize.Width, maxHeight));
                accumX += ellipsis.DesiredSize.Width;
            }
            else
            {
                ellipsis.Arrange(default);
            }
        }

        // Regular items (index 1+)
        for (int i = 1; i < count; i++)
        {
            var child = Children[i];
            if (i < firstToRender)
            {
                child.Arrange(default);
            }
            else
            {
                child.Arrange(new Rect(accumX, 0, child.DesiredSize.Width, maxHeight));
                accumX += child.DesiredSize.Width;
                VisibleItemsCount++;
            }
        }

        return finalSize;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Walks backwards from the last item and returns the index of the first item
    /// that still fits alongside the ellipsis button.
    /// </summary>
    private int GetFirstItemIndexToArrange(double availableWidth)
    {
        int count = Children.Count;
        if (count == 0) return 0;

        double ellipsisWidth = Children[0].DesiredSize.Width;

        // Always show at least the last item.
        double accumLength = Children[count - 1].DesiredSize.Width + ellipsisWidth;

        for (int i = count - 2; i >= 1; i--)
        {
            double newAccum = accumLength + Children[i].DesiredSize.Width;
            if (newAccum > availableWidth)
                return i + 1;
            accumLength = newAccum;
        }

        return 1; // everything fits after the ellipsis
    }

    private double GetMaxHeight(int firstToRender)
    {
        int count = Children.Count;
        double max = EllipsisIsRendered && count > 0 ? Children[0].DesiredSize.Height : 0;
        for (int i = firstToRender; i < count; i++)
            max = Math.Max(max, Children[i].DesiredSize.Height);
        return max;
    }
}
