using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Utilities;
using Avalonia.VisualTree;
using PleasantUI.Controls.Utils;

namespace PleasantUI.Controls;

/// <summary>
/// Positions child elements in sequential position from left to right,
/// breaking content to the next line at the edge of the containing box.
/// Subsequent ordering happens sequentially from top to bottom or from right to left,
/// depending on the value of the <see cref="Orientation" /> property.
/// </summary>
/// <remarks>
/// Reference: https://github.com/afunc233/BilibiliClient/blob/599d7451e9187e7a967cbbb0cdbbd6a428493672/src/BilibiliClient/Controls/VirtualizingWarpPanel.cs
/// </remarks>
public class VirtualizingWrapPanel : VirtualizingPanel
{
    private static readonly Rect InvalidViewport = new(double.PositiveInfinity, double.PositiveInfinity, 0, 0);
    private readonly Action<Control, int> _recycleElement;
    private readonly Action<Control> _recycleElementOnItemRemoved;
    private readonly Action<Control, int, int> _updateElementIndex;
    private bool _isInLayout;
    private bool _isWaitingForViewportUpdate;
    private UvSize _lastEstimatedElementSizeUv = new(Orientation.Horizontal, 25, 25);
    private RealizedWrappedElements? _measureElements;
    private RealizedWrappedElements? _realizedElements;
    private Stack<Control>? _recyclePool;
    private Control? _scrollToElement;
    private int _scrollToIndex = -1;
    private ScrollViewer? _scrollViewer;
    private Control? _unrealizedFocusedElement;
    private int _unrealizedFocusedIndex = -1;
    private Rect _viewport = InvalidViewport;

    /// <summary>
    /// Defines the <see cref="Orientation" /> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<VirtualizingWrapPanel>();

    /// <summary>
    /// Defines the <see cref="ItemWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ItemWidthProperty =
        AvaloniaProperty.Register<VirtualizingWrapPanel, double>(nameof(ItemWidth), double.NaN);

    /// <summary>
    /// Defines the <see cref="ItemHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ItemHeightProperty =
        AvaloniaProperty.Register<VirtualizingWrapPanel, double>(nameof(ItemHeight), double.NaN);

    private static readonly AttachedProperty<bool> ItemIsOwnContainerProperty =
        AvaloniaProperty.RegisterAttached<VirtualizingWrapPanel, Control, bool>("ItemIsOwnContainer");

    /// <summary>
    /// Gets or sets the axis along which items are laid out.
    /// </summary>
    /// <value>
    /// One of the enumeration values that specifies the axis along which items are laid out.
    /// The default is Vertical.
    /// </value>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of all items in the WrapPanel.
    /// </summary>
    public double ItemWidth
    {
        get => GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of all items in the WrapPanel.
    /// </summary>
    public double ItemHeight
    {
        get => GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    /// <summary>
    /// Gets the index of the first realized element, or -1 if no elements are realized.
    /// </summary>
    public int FirstRealizedIndex => _realizedElements?.FirstIndex ?? -1;

    /// <summary>
    /// Gets the index of the last realized element, or -1 if no elements are realized.
    /// </summary>
    public int LastRealizedIndex => _realizedElements?.LastIndex ?? -1;

    static VirtualizingWrapPanel()
    {
        OrientationProperty.OverrideDefaultValue(typeof(VirtualizingWrapPanel), Orientation.Horizontal);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualizingWrapPanel" /> class.
    /// </summary>
    public VirtualizingWrapPanel()
    {
        _recycleElement = RecycleElement;
        _recycleElementOnItemRemoved = RecycleElementOnItemRemoved;
        _updateElementIndex = UpdateElementIndex;
        EffectiveViewportChanged += OnEffectiveViewportChanged;
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        IReadOnlyList<object?> items = Items;

        if (items.Count == 0)
            return default;

        // If we're bringing an item into view, ignore any layout passes until we receive a new
        // effective viewport.
        if (_isWaitingForViewportUpdate)
            return DesiredSize;

        _isInLayout = true;

        try
        {
            Orientation orientation = Orientation;

            _realizedElements ??= new RealizedWrappedElements();
            _measureElements ??= new RealizedWrappedElements();

            // We handle horizontal and vertical layouts here so X and Y are abstracted to:
            // - Horizontal layouts: U = horizontal, V = vertical
            // - Vertical layouts: U = vertical, V = horizontal
            MeasureViewport viewport = CalculateMeasureViewport(items);

            // If the viewport is disjunct then we can recycle everything.
            if (viewport.ViewportIsDisjunct)
                _realizedElements.RecycleAllElements(_recycleElement, orientation);

            // Do the measure, creating/recycling elements as necessary to fill the viewport. Don't
            // write to _realizedElements yet, only _measureElements.
            RealizeElements(items, availableSize, ref viewport);

            // Now swap the measureElements and realizedElements collection.
            (_measureElements, _realizedElements) = (_realizedElements, _measureElements);
            _measureElements.ResetForReuse(Orientation);

            return CalculateDesiredSize(orientation, items.Count, viewport);
        }
        finally
        {
            _isInLayout = false;
        }
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (_realizedElements is null)
            return default;

        _isInLayout = true;

        try
        {
            for (int i = 0; i < _realizedElements.Count; ++i)
            {
                Control? e = _realizedElements.Elements[i];

                if (e is not null)
                {
                    UvSize sizeUv = _realizedElements.SizeUv;
                    UvSize positionUv = _realizedElements.PositionsUv[i];
                    Rect rect = new(positionUv.Width, positionUv.Height, sizeUv.Width, sizeUv.Height);
                    e.Arrange(rect);
                    _scrollViewer?.RegisterAnchorCandidate(e);
                }
            }

            return finalSize;
        }
        finally
        {
            _isInLayout = false;
        }
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _scrollViewer = this.FindAncestorOfType<ScrollViewer>();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _scrollViewer = null;
    }

    /// <inheritdoc />
    protected override void OnItemsChanged(IReadOnlyList<object?> items, NotifyCollectionChangedEventArgs e)
    {
        InvalidateMeasure();

        if (_realizedElements is null)
            return;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                _realizedElements.ItemsInserted(e.NewStartingIndex, e.NewItems!.Count, _updateElementIndex);
                break;
            case NotifyCollectionChangedAction.Remove:
                _realizedElements.ItemsRemoved(e.OldStartingIndex, e.OldItems!.Count, _updateElementIndex,
                    _recycleElementOnItemRemoved);
                break;
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Move:
                _realizedElements.ItemsRemoved(e.OldStartingIndex, e.OldItems!.Count, _updateElementIndex,
                    _recycleElementOnItemRemoved);
                _realizedElements.ItemsInserted(e.NewStartingIndex, e.NewItems!.Count, _updateElementIndex);
                break;
            case NotifyCollectionChangedAction.Reset:
                _realizedElements.ItemsReset(_recycleElementOnItemRemoved, Orientation);
                break;
        }
    }

    /// <inheritdoc />
    protected override IInputElement? GetControl(NavigationDirection direction, IInputElement? from, bool wrap)
    {
        int count = Items.Count;

        if (count == 0 || from is not Control fromControl)
            return null;

        bool horiz = Orientation == Orientation.Horizontal;
        int fromIndex = IndexFromContainer(fromControl);
        int toIndex = fromIndex;

        switch (direction)
        {
            case NavigationDirection.First:
                toIndex = 0;
                break;
            case NavigationDirection.Last:
                toIndex = count - 1;
                break;
            case NavigationDirection.Next:
                ++toIndex;
                break;
            case NavigationDirection.Previous:
                --toIndex;
                break;
            case NavigationDirection.Left:
                if (horiz)
                    --toIndex;
                break;
            case NavigationDirection.Right:
                if (horiz)
                    ++toIndex;
                break;
            case NavigationDirection.Up:
                if (!horiz)
                    --toIndex;
                break;
            case NavigationDirection.Down:
                if (!horiz)
                    ++toIndex;
                break;
            default:
                return null;
        }

        if (fromIndex == toIndex)
            return from;

        if (wrap)
        {
            if (toIndex < 0)
                toIndex = count - 1;
            else if (toIndex >= count)
                toIndex = 0;
        }

        return ScrollIntoView(toIndex);
    }

    /// <inheritdoc />
    protected override IEnumerable<Control> GetRealizedContainers()
    {
        return _realizedElements?.Elements.Where(x => x is not null)!;
    }

    /// <inheritdoc />
    protected override Control? ContainerFromIndex(int index)
    {
        if (index < 0 || index >= Items.Count)
            return null;
        if (_realizedElements?.GetElement(index) is { } realized)
            return realized;
        if (Items[index] is Control c && c.GetValue(ItemIsOwnContainerProperty))
            return c;
        return null;
    }

    /// <inheritdoc />
    protected override int IndexFromContainer(Control container)
    {
        return _realizedElements?.GetIndex(container) ?? -1;
    }

    /// <inheritdoc />
    protected override Control? ScrollIntoView(int index)
    {
        IReadOnlyList<object?> items = Items;

        if (_isInLayout || index < 0 || index >= items.Count || _realizedElements is null)
            return null;

        if (GetRealizedElement(index) is { } element)
        {
            element.BringIntoView();
            return element;
        }

        if (this.GetVisualRoot() is ILayoutRoot)
        {
            // Create and measure the element to be brought into view. Store it in a field so that
            // it can be re-used in the layout pass.
            double itemWidth = ItemWidth;
            double itemHeight = ItemHeight;
            bool isItemWidthSet = !double.IsNaN(itemWidth);
            bool isItemHeightSet = !double.IsNaN(itemHeight);
            Size size = new(isItemWidthSet ? itemWidth : double.PositiveInfinity,
                isItemHeightSet ? itemHeight : double.PositiveInfinity);
            _scrollToElement = GetOrCreateElement(items, index);
            _scrollToElement.Measure(size);
            _scrollToIndex = index;

            Rect viewport = _viewport != InvalidViewport ? _viewport : EstimateViewport();
            UvSize viewportEnd = Orientation == Orientation.Horizontal
                ? new UvSize(Orientation, viewport.Right, viewport.Bottom)
                : new UvSize(Orientation, viewport.Bottom, viewport.Right);

            // Get the expected position of the element and put it in place.
            UvSize anchorUv =
                _realizedElements.GetOrEstimateElementUv(index, ref _lastEstimatedElementSizeUv, viewportEnd);
            size = new Size(isItemWidthSet ? itemWidth : _scrollToElement.DesiredSize.Width,
                isItemHeightSet ? itemHeight : _scrollToElement.DesiredSize.Height);
            Rect rect = new(anchorUv.Width, anchorUv.Height, size.Width, size.Height);
            _scrollToElement.Arrange(rect);

            // If the item being brought into view was added since the last layout pass then
            // our bounds won't be updated, so any containing scroll viewers will not have an
            // updated extent. Do a layout pass to ensure that the containing scroll viewers
            // will be able to scroll the new item into view.
            if (!Bounds.Contains(rect) && !_viewport.Contains(rect))
            {
                _isWaitingForViewportUpdate = true;
                //root.LayoutManager.ExecuteLayoutPass();
                _isWaitingForViewportUpdate = false;
            }

            // Try to bring the item into view.
            _scrollToElement.BringIntoView();

            // If the viewport does not contain the item to scroll to, set _isWaitingForViewportUpdate:
            // this should cause the following chain of events:
            // - Measure is first done with the old viewport (which will be a no-op, see MeasureOverride)
            // - The viewport is then updated by the layout system which invalidates our measure
            // - Measure is then done with the new viewport.
            _isWaitingForViewportUpdate = !_viewport.Contains(rect);
            //root.LayoutManager.ExecuteLayoutPass();

            // If for some reason the layout system didn't give us a new viewport during the layout, we
            // need to do another layout pass as the one that took place was a no-op.
            if (_isWaitingForViewportUpdate)
            {
                _isWaitingForViewportUpdate = false;
                InvalidateMeasure();
                //root.LayoutManager.ExecuteLayoutPass();
            }

            Control? result = _scrollToElement;
            _scrollToElement = null;
            _scrollToIndex = -1;
            return result;
        }

        return null;
    }

    private UvSize EstimateElementSizeUv()
    {
        double itemWidth = ItemWidth;
        double itemHeight = ItemHeight;
        bool isItemWidthSet = !double.IsNaN(itemWidth);
        bool isItemHeightSet = !double.IsNaN(itemHeight);

        UvSize estimatedSize = new(Orientation,
            isItemWidthSet ? itemWidth : _lastEstimatedElementSizeUv.Width,
            isItemHeightSet ? itemHeight : _lastEstimatedElementSizeUv.Height);

        if ((isItemWidthSet && isItemHeightSet) || _realizedElements is null)
            return estimatedSize;

        UvSize? result = _realizedElements.EstimateElementSize(Orientation);
        if (result != null)
        {
            estimatedSize = result.Value;
            estimatedSize.Width = isItemWidthSet ? itemWidth : estimatedSize.Width;
            estimatedSize.Height = isItemHeightSet ? itemHeight : estimatedSize.Height;
        }

        return estimatedSize;
    }

    private MeasureViewport CalculateMeasureViewport(IReadOnlyList<object?> items)
    {

        Debug.Assert(_realizedElements is not null);

        if (_realizedElements is null)
            throw new InvalidOperationException("_realizedElements must not be null.");

        // If the control has not yet been laid out then the effective viewport won't have been set.
        // Try to work it out from an ancestor control.
        Rect viewport = _viewport != InvalidViewport ? _viewport : EstimateViewport();

        // Get the viewport in the orientation direction.
        UvSize viewportStart = new(Orientation, viewport.X, viewport.Y);
        UvSize viewportEnd = new(Orientation, viewport.Right, viewport.Bottom);

        // Get or estimate the anchor element from which to start realization.
        int itemCount = items.Count;
        _lastEstimatedElementSizeUv.Orientation = Orientation;
        (int anchorIndex, UvSize anchorU) = _realizedElements.GetOrEstimateAnchorElementForViewport(
            viewportStart,
            viewportEnd,
            itemCount,
            ref _lastEstimatedElementSizeUv);

        // Check if the anchor element is not within the currently realized elements.
        bool disjunct = anchorIndex < _realizedElements.FirstIndex ||
                        anchorIndex > _realizedElements.LastIndex;

        return new MeasureViewport
        {
            AnchorIndex = anchorIndex,
            AnchorUv = anchorU,
            ViewportUvStart = viewportStart,
            ViewportUvEnd = viewportEnd,
            ViewportIsDisjunct = disjunct
        };
    }

    private Size CalculateDesiredSize(Orientation orientation, int itemCount, in MeasureViewport viewport)
    {
        UvSize sizeUv = new(orientation);
        UvSize estimatedSize = EstimateElementSizeUv();

        if (!double.IsNaN(ItemWidth) && !double.IsNaN(ItemHeight))
        {
            // Since ItemWidth and ItemHeight are set, we simply compute the actual size
            double uLength = viewport.ViewportUvEnd.U;
            int estimatedItemsPerU = (int)(uLength / estimatedSize.U);
            double estimatedULanes = Math.Ceiling((double)itemCount / estimatedItemsPerU);
            sizeUv.U = estimatedItemsPerU * estimatedSize.U;
            sizeUv.V = estimatedULanes * estimatedSize.V;
        }
        else if (viewport.LastIndex >= 0)
        {
            int remaining = itemCount - viewport.LastIndex - 1;
            sizeUv = viewport.RealizedEndUv;
            double u = sizeUv.U;

            while (remaining > 0)
            {
                double newU = u + estimatedSize.U;
                if (newU > viewport.ViewportUvEnd.U)
                {
                    sizeUv.V += estimatedSize.V;
                    newU = viewport.ViewportUvStart.U + estimatedSize.U;
                }

                u = newU;
                sizeUv.U = Math.Max(sizeUv.U, u);

                remaining--;
            }

            sizeUv.V += estimatedSize.V;
        }

        return new Size(sizeUv.Width, sizeUv.Height);
    }

    private Rect EstimateViewport()
    {
        Visual? c = this.GetVisualParent();
        Rect viewport = new();

        if (c is null) return viewport;

        while (c is not null)
        {
            if ((c.Bounds.Width != 0 || c.Bounds.Height != 0) &&
                c.TransformToVisual(this) is { } transform)
            {
                viewport = new Rect(0, 0, c.Bounds.Width, c.Bounds.Height)
                    .TransformToAABB(transform);
                break;
            }

            c = c.GetVisualParent();
        }


        return viewport;
    }

    private void RealizeElements(
    IReadOnlyList<object?> items,
    Size availableSize,
    ref MeasureViewport viewport)
    {
        if (_measureElements is null)
            throw new InvalidOperationException("_measureElements is null.");
        if (_realizedElements is null)
            throw new InvalidOperationException("_realizedElements is null.");
        if (items.Count == 0)
            throw new ArgumentException("Items collection cannot be empty.", nameof(items));

        int index = viewport.AnchorIndex;
        UvSize uv = viewport.AnchorUv;
        double v = uv.V, maxSizeV = 0;
        UvSize size = new(Orientation);
        bool firstChildMeasured = false;
        double itemWidth = ItemWidth, itemHeight = ItemHeight;
        bool isItemWidthSet = !double.IsNaN(itemWidth), isItemHeightSet = !double.IsNaN(itemHeight);
        Size childConstraint = new(isItemWidthSet ? itemWidth : availableSize.Width,
                                   isItemHeightSet ? itemHeight : availableSize.Height);

        // Recycle elements before the anchor if necessary.
        if (uv.V <= viewport.AnchorUv.V)
            _realizedElements.RecycleElementsBefore(viewport.AnchorIndex, _recycleElement, Orientation);

        // Realize elements forward from the anchor.
        while (uv.V < viewport.ViewportUvEnd.V && index < items.Count)
        {
            if (uv.U + size.U > viewport.ViewportUvEnd.U && uv.V + maxSizeV > viewport.ViewportUvEnd.V)
                break;

            if (firstChildMeasured)
                childConstraint = new Size(size.Width, size.Height);

            Control e = GetOrCreateElement(items, index);
            e.Measure(childConstraint);
            if (!firstChildMeasured)
            {
                size = new UvSize(Orientation,
                    isItemWidthSet ? itemWidth : e.DesiredSize.Width,
                    isItemHeightSet ? itemHeight : e.DesiredSize.Height);
                firstChildMeasured = true;
            }
            maxSizeV = Math.Max(maxSizeV, size.V);

            UvSize uEnd = new(Orientation) { U = uv.U + size.U, V = Math.Max(v, uv.V) };
            if (uEnd.U > viewport.ViewportUvEnd.U)
            {
                uv.U = viewport.ViewportUvStart.U;
                v += maxSizeV;
                maxSizeV = 0;
                uv.V = v;
            }

            _measureElements.Add(index, e, uv, size);
            uv = new UvSize(Orientation) { U = uv.U + size.U, V = Math.Max(v, uv.V) };
            index++;
        }

        viewport.LastIndex = index - 1;
        viewport.RealizedEndUv = uv;
        _realizedElements.RecycleElementsAfter(viewport.LastIndex, _recycleElement, Orientation);

        // Realize elements backwards from the anchor.
        index = viewport.AnchorIndex - 1;
        uv = viewport.AnchorUv;
        while (index >= 0)
        {
            if (uv.U - size.U < viewport.ViewportUvStart.U && uv.V <= viewport.ViewportUvStart.V)
                break;

            if (firstChildMeasured)
                childConstraint = new Size(size.Width, size.Height);

            Control e = GetOrCreateElement(items, index);
            e.Measure(childConstraint);
            if (!firstChildMeasured)
            {
                size = new UvSize(Orientation,
                    isItemWidthSet ? itemWidth : e.DesiredSize.Width,
                    isItemHeightSet ? itemHeight : e.DesiredSize.Height);
                firstChildMeasured = true;
            }
            uv.U -= size.U;
            if (uv.U < viewport.ViewportUvStart.U)
            {
                double uLength = viewport.ViewportUvEnd.U - viewport.ViewportUvStart.U;
                double uConstraint = (int)(uLength / size.U) * size.U;
                uv.U = uConstraint - size.U;
                uv.V -= size.V;
            }
            _measureElements.Add(index, e, uv, size);
            index--;
        }
        _realizedElements.RecycleElementsBefore(index + 1, _recycleElement, Orientation);
    }

    private Control GetOrCreateElement(IReadOnlyList<object?> items, int index)
    {
        Control e = GetRealizedElement(index) ??
                    GetItemIsOwnContainer(items, index) ??
                    GetRecycledElement(items, index) ??
                    CreateElement(items, index);
        return e;
    }

    private Control? GetRealizedElement(int index)
    {
        if (_scrollToIndex == index)
            return _scrollToElement;
        return _realizedElements?.GetElement(index);
    }

    private Control? GetItemIsOwnContainer(IReadOnlyList<object?> items, int index)
    {
        object? item = items[index];

        if (item is Control controlItem)
            if (controlItem.IsSet(ItemIsOwnContainerProperty))
            {
                controlItem.IsVisible = true;
                return controlItem;
            }

        /*else if (generator.IsItemItsOwnContainer(controlItem))
            {
                generator.PrepareItemContainer(controlItem, controlItem, index);
                AddInternalChild(controlItem);
                controlItem.SetValue(ItemIsOwnContainerProperty, true);
                generator.ItemContainerPrepared(controlItem, item, index);
                return controlItem;
            }*/
        return null;
    }

    private Control? GetRecycledElement(IReadOnlyList<object?> items, int index)
    {
        ItemContainerGenerator generator = ItemContainerGenerator!;
        object? item = items[index];

        if (_unrealizedFocusedIndex == index && _unrealizedFocusedElement is not null)
        {
            Control? element = _unrealizedFocusedElement;
            _unrealizedFocusedElement.LostFocus -= OnUnrealizedFocusedElementLostFocus;
            _unrealizedFocusedElement = null;
            _unrealizedFocusedIndex = -1;
            return element;
        }

        if (_recyclePool?.Count > 0)
        {
            Control recycled = _recyclePool.Pop();
            recycled.IsVisible = true;
            generator.PrepareItemContainer(recycled, item, index);
            generator.ItemContainerPrepared(recycled, item, index);
            return recycled;
        }

        return null;
    }

    private Control CreateElement(IReadOnlyList<object?> items, int index)
    {
        ItemContainerGenerator generator = ItemContainerGenerator!;
        object? item = items[index];
        generator.NeedsContainer(item, index, out object? key);
        Control container = generator.CreateContainer(item, index, key);

        generator.PrepareItemContainer(container, item, index);
        AddInternalChild(container);
        generator.ItemContainerPrepared(container, item, index);

        return container;
    }

    private void RecycleElement(Control element, int index)
    {
        _scrollViewer?.UnregisterAnchorCandidate(element);

        if (element.IsSet(ItemIsOwnContainerProperty))
        {
            element.IsVisible = false;
        }
        else if (element.IsKeyboardFocusWithin)
        {
            _unrealizedFocusedElement = element;
            _unrealizedFocusedIndex = index;
            _unrealizedFocusedElement.LostFocus += OnUnrealizedFocusedElementLostFocus;
        }
        else
        {
            ItemContainerGenerator!.ClearItemContainer(element);
            _recyclePool ??= new Stack<Control>();
            _recyclePool.Push(element);
            element.IsVisible = false;
        }
    }

    private void RecycleElementOnItemRemoved(Control element)
    {
        if (element.IsSet(ItemIsOwnContainerProperty))
        {
            RemoveInternalChild(element);
        }
        else
        {
            ItemContainerGenerator!.ClearItemContainer(element);
            _recyclePool ??= new Stack<Control>();
            _recyclePool.Push(element);
            element.IsVisible = false;
        }
    }

    private void UpdateElementIndex(Control element, int oldIndex, int newIndex)
    {
        if (ItemContainerGenerator is null)
            throw new InvalidOperationException("ItemContainerGenerator is null.");

        ItemContainerGenerator.ItemContainerIndexChanged(element, oldIndex, newIndex);
    }

    private void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        bool horizontal = Orientation == Orientation.Horizontal;
        double oldViewportStartU = horizontal ? _viewport.Left : _viewport.Top;
        double oldViewportEndU = horizontal ? _viewport.Right : _viewport.Bottom;
        double oldViewportStartV = horizontal ? _viewport.Top : _viewport.Left;
        double oldViewportEndV = horizontal ? _viewport.Bottom : _viewport.Right;

        _viewport = e.EffectiveViewport.Intersect(new Rect(Bounds.Size));
        _isWaitingForViewportUpdate = false;

        double newViewportStartU = horizontal ? _viewport.Left : _viewport.Top;
        double newViewportEndU = horizontal ? _viewport.Right : _viewport.Bottom;
        double newViewportStartV = horizontal ? _viewport.Top : _viewport.Left;
        double newViewportEndV = horizontal ? _viewport.Bottom : _viewport.Right;

        if (!MathUtilities.AreClose(oldViewportStartU, newViewportStartU) ||
            !MathUtilities.AreClose(oldViewportEndU, newViewportEndU) ||
            !MathUtilities.AreClose(oldViewportStartV, newViewportStartV) ||
            !MathUtilities.AreClose(oldViewportEndV, newViewportEndV))
            InvalidateMeasure();
    }

    private void OnUnrealizedFocusedElementLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is null || _unrealizedFocusedElement is null || !sender.Equals(_unrealizedFocusedElement))
            return;

        _unrealizedFocusedElement.LostFocus -= OnUnrealizedFocusedElementLostFocus;
        RecycleElement(_unrealizedFocusedElement, _unrealizedFocusedIndex);
        _unrealizedFocusedElement = null;
        _unrealizedFocusedIndex = -1;
    }

    private struct MeasureViewport
    {
        public int AnchorIndex;
        public UvSize AnchorUv;
        public UvSize ViewportUvStart;
        public UvSize ViewportUvEnd;
        public UvSize RealizedEndUv;
        public int LastIndex;
        public bool ViewportIsDisjunct;
    }
}