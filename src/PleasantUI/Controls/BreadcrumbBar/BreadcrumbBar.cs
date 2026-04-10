using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace PleasantUI.Controls;

/// <summary>
/// Displays a breadcrumb trail that shows the path to the current location.
/// Items that overflow the available width are collapsed into an ellipsis flyout.
/// </summary>
[TemplatePart(PART_Panel, typeof(BreadcrumbBarPanel))]
public class BreadcrumbBar : ItemsControl
{
    private const string PART_Panel = "PART_Panel";

    // ── Styled properties ─────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="IsLastItemClickEnabled"/> property.</summary>
    public static readonly StyledProperty<bool> IsLastItemClickEnabledProperty =
        AvaloniaProperty.Register<BreadcrumbBar, bool>(nameof(IsLastItemClickEnabled));

    // ── CLR accessors ─────────────────────────────────────────────────────────

    /// <summary>
    /// Gets or sets whether the last (current) item can be clicked.
    /// When <c>false</c> the last item is rendered as plain text.
    /// </summary>
    public bool IsLastItemClickEnabled
    {
        get => GetValue(IsLastItemClickEnabledProperty);
        set => SetValue(IsLastItemClickEnabledProperty, value);
    }

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Raised when the user clicks a breadcrumb item.</summary>
    public event EventHandler<BreadcrumbBarItemClickedEventArgs>? ItemClicked;

    // ── Private state ─────────────────────────────────────────────────────────

    private BreadcrumbBarPanel? _panel;
    private BreadcrumbBarItem?  _ellipsisItem;
    private BreadcrumbBarItem?  _lastItem;
    private int                 _focusedIndex;

    // ── Static constructor ────────────────────────────────────────────────────

    static BreadcrumbBar()
    {
        // Override the default items panel to our custom breadcrumb panel.
        ItemsPanelProperty.OverrideDefaultValue<BreadcrumbBar>(
            new FuncTemplate<Panel?>(() => new BreadcrumbBarPanel()));
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    public BreadcrumbBar()
    {
        AddHandler(KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
    }

    // ── ItemsControl overrides ────────────────────────────────────────────────

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        recycleKey = null;
        return item is not BreadcrumbBarItem;
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new BreadcrumbBarItem();

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);

        if (container is not BreadcrumbBarItem bcbItem) return;

        // +1 because index 0 in the panel is the ellipsis sentinel.
        int panelIndex = index + 1;

        bcbItem.SetIsEllipsisDropDownItem(false);
        bcbItem.SetParentBreadcrumb(this);
        bcbItem.SetIndex(panelIndex);

        int lastIndex = ItemCount - 1;
        if (index == lastIndex)
            UpdateLastElement(bcbItem);
        else
            bcbItem.ResetVisualProperties();
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is BreadcrumbBarItem bcbItem)
            bcbItem.ResetVisualProperties();
        base.ClearContainerForItemOverride(container);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _panel = ItemsPanelRoot as BreadcrumbBarPanel;
        EnsureEllipsisItem();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsLastItemClickEnabledProperty)
            ForceUpdateLastElement();
        else if (change.Property == ItemTemplateProperty)
            _ellipsisItem?.SetEllipsisDropDownItemDataTemplate(ItemTemplate);
        else if (change.Property == ItemsSourceProperty)
        {
            // Unsubscribe from old collection, subscribe to new one.
            if (change.OldValue is INotifyCollectionChanged oldNcc)
                oldNcc.CollectionChanged -= OnSourceCollectionChanged;
            if (change.NewValue is INotifyCollectionChanged newNcc)
                newNcc.CollectionChanged += OnSourceCollectionChanged;

            ForceUpdateLastElement();
        }
    }

    private void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => ForceUpdateLastElement();

    // ── Internal API used by BreadcrumbBarItem / BreadcrumbBarPanel ───────────

    internal void RaiseItemClickedEvent(object? content, int index)
        => ItemClicked?.Invoke(this, new BreadcrumbBarItemClickedEventArgs(index, content));

    /// <summary>
    /// Returns the items currently hidden behind the ellipsis, in source order.
    /// </summary>
    internal IEnumerable<object?>? HiddenElements()
    {
        if (_panel is null || !_panel.EllipsisIsRendered) return null;
        return GetHiddenElements(_panel.FirstRenderedItemIndexAfterEllipsis);
    }

    /// <summary>
    /// Updates accessibility position/size attributes on all visible items.
    /// Called by <see cref="BreadcrumbBarPanel"/> after each arrange pass.
    /// </summary>
    internal void ReIndexVisibleElementsForAccessibility()
    {
        if (_panel is null) return;

        bool ellipsisVisible = _panel.EllipsisIsRendered;
        int  visibleCount    = _panel.VisibleItemsCount;
        int  firstIndex      = ellipsisVisible ? _panel.FirstRenderedItemIndexAfterEllipsis : 1;

        if (_ellipsisItem is not null)
        {
            var view = ellipsisVisible ? AccessibilityView.Content : AccessibilityView.Raw;
            _ellipsisItem.SetValue(AutomationProperties.AccessibilityViewProperty, view);
        }

        var panelChildren = _panel.Children;
        for (int acc = 1, real = firstIndex; acc <= visibleCount; acc++, real++)
        {
            if (real < panelChildren.Count && panelChildren[real] is Control c)
            {
                c.SetValue(AutomationProperties.PositionInSetProperty, acc);
                c.SetValue(AutomationProperties.SizeOfSetProperty, visibleCount);
            }
        }
    }

    // ── Ellipsis item management ──────────────────────────────────────────────

    /// <summary>
    /// Ensures the ellipsis sentinel item exists at index 0 of the panel.
    /// The ellipsis is not part of ItemsSource — it is injected directly into the panel.
    /// </summary>
    private void EnsureEllipsisItem()
    {
        if (_panel is null) return;

        // Remove any stale ellipsis item.
        if (_ellipsisItem is not null && _panel.Children.Contains(_ellipsisItem))
            _panel.Children.Remove(_ellipsisItem);

        _ellipsisItem = new BreadcrumbBarItem();
        _ellipsisItem.SetParentBreadcrumb(this);
        _ellipsisItem.SetIndex(0);
        _ellipsisItem.SetPropertiesForEllipsisItem();
        _ellipsisItem.SetEllipsisDropDownItemDataTemplate(ItemTemplate);

        AutomationProperties.SetName(_ellipsisItem, "More");

        // Insert at position 0 — before all data items.
        _panel.Children.Insert(0, _ellipsisItem);
    }

    // ── Last-item tracking ────────────────────────────────────────────────────

    private void ForceUpdateLastElement()
    {
        if (ItemCount == 0) { ResetLastItem(); return; }

        // The last container is at ItemCount-1 in the ItemsControl.
        var container = ContainerFromIndex(ItemCount - 1) as BreadcrumbBarItem;
        if (container is not null)
            UpdateLastElement(container);
    }

    private void UpdateLastElement(BreadcrumbBarItem newLast)
    {
        ResetLastItem();
        newLast.SetPropertiesForLastItem();
        _lastItem = newLast;
    }

    private void ResetLastItem()
    {
        _lastItem?.ResetVisualProperties();
        _lastItem = null;
    }

    // ── Hidden elements ───────────────────────────────────────────────────────

    private IEnumerable<object?> GetHiddenElements(int firstShownPanelIndex)
    {
        // firstShownPanelIndex is a panel index (0 = ellipsis, 1+ = data items).
        // Convert to ItemsSource index: panelIndex - 1.
        int firstShownItemIndex = firstShownPanelIndex - 1;
        for (int i = 0; i < firstShownItemIndex; i++)
            yield return Items[i];
    }

    // ── Keyboard navigation ───────────────────────────────────────────────────

    private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        bool ltr      = FlowDirection == Avalonia.Media.FlowDirection.LeftToRight;
        bool keyLeft  = e.Key == Key.Left;
        bool keyRight = e.Key == Key.Right;

        if ((ltr && keyRight) || (!ltr && keyLeft))
        {
            if (MoveFocusNext()) { e.Handled = true; return; }
        }
        else if ((ltr && keyLeft) || (!ltr && keyRight))
        {
            if (MoveFocusPrevious()) { e.Handled = true; }
        }
    }

    private bool MoveFocusNext()
    {
        int delta = 1;
        if (_focusedIndex == 0 && _panel is { EllipsisIsRendered: true })
            delta = _panel.FirstRenderedItemIndexAfterEllipsis;
        return MoveFocus(delta);
    }

    private bool MoveFocusPrevious()
    {
        if (_panel is { EllipsisIsRendered: true } &&
            _focusedIndex == _panel.FirstRenderedItemIndexAfterEllipsis)
            return MoveFocus(-_focusedIndex); // jump back to ellipsis (index 0)

        return MoveFocus(-1);
    }

    private bool MoveFocus(int delta)
    {
        if (delta == 0 || _panel is null) return false;

        var focused = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        if (focused is not Control focusedControl) return false;

        int current = _panel.Children.IndexOf(focusedControl);
        if (current < 0) return false;

        int target = current + delta;
        int count  = _panel.Children.Count;

        while (target >= 0 && target < count)
        {
            var el = _panel.Children[target];
            if (el.Focus())
            {
                _focusedIndex = target;
                return true;
            }
            target += Math.Sign(delta);
        }

        return false;
    }
}
