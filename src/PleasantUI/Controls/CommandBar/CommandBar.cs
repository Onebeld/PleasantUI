using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Metadata;

namespace PleasantUI.Controls;

/// <summary>
/// A toolbar control that displays primary commands inline and moves overflow items
/// into a popup menu. Supports dynamic overflow, label positions, and open/closed states.
/// </summary>
[TemplatePart(PART_PrimaryItemsControl,   typeof(ItemsControl))]
[TemplatePart(PART_ContentControl,        typeof(ContentControl))]
[TemplatePart(PART_SecondaryItemsControl, typeof(CommandBarOverflowPresenter))]
[TemplatePart(PART_MoreButton,            typeof(Button))]
[PseudoClasses(PC_Open, PC_Compact, PC_Minimal, PC_Hidden,
               PC_LabelBottom, PC_LabelRight, PC_LabelCollapsed,
               PC_PrimaryOnly, PC_SecondaryOnly, PC_ItemsRight)]
public class CommandBar : ContentControl
{
    // ── Template part names ───────────────────────────────────────────────────

    private const string PART_PrimaryItemsControl   = "PART_PrimaryItemsControl";
    private const string PART_ContentControl        = "PART_ContentControl";
    private const string PART_SecondaryItemsControl = "PART_SecondaryItemsControl";
    private const string PART_MoreButton            = "PART_MoreButton";

    // ── Pseudo-class names ────────────────────────────────────────────────────

    private const string PC_Open           = ":open";
    private const string PC_Compact        = ":compact";
    private const string PC_Minimal        = ":minimal";
    private const string PC_Hidden         = ":hidden";
    private const string PC_LabelBottom    = ":labelBottom";
    private const string PC_LabelRight     = ":labelRight";
    private const string PC_LabelCollapsed = ":labelCollapsed";
    private const string PC_PrimaryOnly    = ":primaryOnly";
    private const string PC_SecondaryOnly  = ":secondaryOnly";
    private const string PC_ItemsRight     = ":itemsRight";

    // ── Styled / direct properties ────────────────────────────────────────────

    /// <summary>Defines the <see cref="IsOpen"/> property.</summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<CommandBar, bool>(nameof(IsOpen));

    /// <summary>Defines the <see cref="IsSticky"/> property.</summary>
    public static readonly StyledProperty<bool> IsStickyProperty =
        AvaloniaProperty.Register<CommandBar, bool>(nameof(IsSticky), defaultValue: true);

    /// <summary>Defines the <see cref="ClosedDisplayMode"/> property.</summary>
    public static readonly StyledProperty<CommandBarClosedDisplayMode> ClosedDisplayModeProperty =
        AvaloniaProperty.Register<CommandBar, CommandBarClosedDisplayMode>(nameof(ClosedDisplayMode),
            defaultValue: CommandBarClosedDisplayMode.Compact);

    /// <summary>Defines the <see cref="DefaultLabelPosition"/> property.</summary>
    public static readonly StyledProperty<CommandBarDefaultLabelPosition> DefaultLabelPositionProperty =
        AvaloniaProperty.Register<CommandBar, CommandBarDefaultLabelPosition>(nameof(DefaultLabelPosition));

    /// <summary>Defines the <see cref="ItemsAlignment"/> property.</summary>
    public static readonly StyledProperty<CommandBarItemsAlignment> ItemsAlignmentProperty =
        AvaloniaProperty.Register<CommandBar, CommandBarItemsAlignment>(nameof(ItemsAlignment));

    /// <summary>Defines the <see cref="OverflowButtonVisibility"/> property.</summary>
    public static readonly StyledProperty<CommandBarOverflowButtonVisibility> OverflowButtonVisibilityProperty =
        AvaloniaProperty.Register<CommandBar, CommandBarOverflowButtonVisibility>(nameof(OverflowButtonVisibility));

    /// <summary>Defines the <see cref="IsDynamicOverflowEnabled"/> property.</summary>
    public static readonly StyledProperty<bool> IsDynamicOverflowEnabledProperty =
        AvaloniaProperty.Register<CommandBar, bool>(nameof(IsDynamicOverflowEnabled), defaultValue: true);

    /// <summary>Defines the <see cref="PrimaryCommands"/> direct property.</summary>
    public static readonly DirectProperty<CommandBar, IAvaloniaList<ICommandBarElement>> PrimaryCommandsProperty =
        AvaloniaProperty.RegisterDirect<CommandBar, IAvaloniaList<ICommandBarElement>>(
            nameof(PrimaryCommands), o => o.PrimaryCommands);

    /// <summary>Defines the <see cref="SecondaryCommands"/> direct property.</summary>
    public static readonly DirectProperty<CommandBar, IAvaloniaList<ICommandBarElement>> SecondaryCommandsProperty =
        AvaloniaProperty.RegisterDirect<CommandBar, IAvaloniaList<ICommandBarElement>>(
            nameof(SecondaryCommands), o => o.SecondaryCommands);

    // ── CLR accessors ─────────────────────────────────────────────────────────

    /// <summary>Gets or sets whether the overflow popup is open.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Gets or sets whether the popup stays open on light-dismiss.</summary>
    public bool IsSticky
    {
        get => GetValue(IsStickyProperty);
        set => SetValue(IsStickyProperty, value);
    }

    /// <summary>Gets or sets how the bar appears when closed.</summary>
    public CommandBarClosedDisplayMode ClosedDisplayMode
    {
        get => GetValue(ClosedDisplayModeProperty);
        set => SetValue(ClosedDisplayModeProperty, value);
    }

    /// <summary>Gets or sets the label position for primary command buttons.</summary>
    public CommandBarDefaultLabelPosition DefaultLabelPosition
    {
        get => GetValue(DefaultLabelPositionProperty);
        set => SetValue(DefaultLabelPositionProperty, value);
    }

    /// <summary>Gets or sets how primary commands are aligned.</summary>
    public CommandBarItemsAlignment ItemsAlignment
    {
        get => GetValue(ItemsAlignmentProperty);
        set => SetValue(ItemsAlignmentProperty, value);
    }

    /// <summary>Gets or sets when the overflow button is shown.</summary>
    public CommandBarOverflowButtonVisibility OverflowButtonVisibility
    {
        get => GetValue(OverflowButtonVisibilityProperty);
        set => SetValue(OverflowButtonVisibilityProperty, value);
    }

    /// <summary>Gets or sets whether primary commands automatically move to overflow when space is limited.</summary>
    public bool IsDynamicOverflowEnabled
    {
        get => GetValue(IsDynamicOverflowEnabledProperty);
        set => SetValue(IsDynamicOverflowEnabledProperty, value);
    }

    /// <summary>Gets the collection of primary command elements.</summary>
    [Content]
    public IAvaloniaList<ICommandBarElement> PrimaryCommands => _primaryCommands;

    /// <summary>Gets the collection of secondary command elements shown in the overflow popup.</summary>
    public IAvaloniaList<ICommandBarElement> SecondaryCommands => _secondaryCommands;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Raised when the overflow popup starts opening.</summary>
    public event EventHandler<EventArgs>? Opening;
    /// <summary>Raised when the overflow popup has opened.</summary>
    public event EventHandler<EventArgs>? Opened;
    /// <summary>Raised when the overflow popup starts closing.</summary>
    public event EventHandler<EventArgs>? Closing;
    /// <summary>Raised when the overflow popup has closed.</summary>
    public event EventHandler<EventArgs>? Closed;

    // ── Private state ─────────────────────────────────────────────────────────

    private readonly AvaloniaList<ICommandBarElement> _primaryCommands   = new();
    private readonly AvaloniaList<ICommandBarElement> _secondaryCommands = new();

    // Internal working lists sent to the ItemsControls.
    // We never modify the user-facing _primaryCommands/_secondaryCommands.
    private AvaloniaList<ICommandBarElement>? _primaryItems;
    private AvaloniaList<ICommandBarElement>? _overflowItems;

    private ItemsControl?                _primaryItemsHost;
    private ContentControl?              _contentHost;
    private CommandBarOverflowPresenter? _overflowItemsHost;
    private Button?                      _moreButton;
    private CommandBarSeparator?         _overflowSeparator;

    // Dynamic overflow tracking
    private int  _numInOverflow;
    private int  _hasOrderedOverflow;
    private Size _availableSize;
    private Dictionary<ICommandBarElement, double>? _widthCache;
    private double _minRecoverWidth;

    private bool _templateApplied;

    // ── Constructor ───────────────────────────────────────────────────────────

    public CommandBar()
    {
        _primaryCommands.CollectionChanged   += OnPrimaryCommandsChanged;
        _secondaryCommands.CollectionChanged += OnSecondaryCommandsChanged;

        PseudoClasses.Add(PC_Compact);
        PseudoClasses.Add(PC_LabelBottom);
    }

    // ── Template ──────────────────────────────────────────────────────────────

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _templateApplied = false;

        if (_moreButton is not null)
            _moreButton.Click -= OnMoreButtonClick;

        base.OnApplyTemplate(e);

        _primaryItemsHost  = e.NameScope.Find<ItemsControl>(PART_PrimaryItemsControl);
        _contentHost       = e.NameScope.Find<ContentControl>(PART_ContentControl);
        _overflowItemsHost = e.NameScope.Find<CommandBarOverflowPresenter>(PART_SecondaryItemsControl);
        _moreButton        = e.NameScope.Find<Button>(PART_MoreButton);

        if (_moreButton is not null)
            _moreButton.Click += OnMoreButtonClick;

        _templateApplied = true;
        AttachItems();

        // Re-apply open state now that items exist — IsOpen may have been set
        // before the template was applied (e.g. IsOpen="True" in XAML).
        if (IsOpen)
            ApplyOpenStateToItems(true);

        UpdateMoreButtonVisibility();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsOpenProperty)
        {
            OnIsOpenChanged(change.GetNewValue<bool>());
        }
        else if (change.Property == DefaultLabelPositionProperty)
        {
            var pos = change.GetNewValue<CommandBarDefaultLabelPosition>();
            PseudoClasses.Set(PC_LabelBottom,    pos == CommandBarDefaultLabelPosition.Bottom);
            PseudoClasses.Set(PC_LabelRight,     pos == CommandBarDefaultLabelPosition.Right);
            PseudoClasses.Set(PC_LabelCollapsed, pos == CommandBarDefaultLabelPosition.Collapsed);
            ApplyLabelPositionToItems(pos);
        }
        else if (change.Property == ClosedDisplayModeProperty)
        {
            var mode = change.GetNewValue<CommandBarClosedDisplayMode>();
            PseudoClasses.Set(PC_Compact, mode == CommandBarClosedDisplayMode.Compact);
            PseudoClasses.Set(PC_Minimal, mode == CommandBarClosedDisplayMode.Minimal);
            PseudoClasses.Set(PC_Hidden,  mode == CommandBarClosedDisplayMode.Hidden);
        }
        else if (change.Property == ItemsAlignmentProperty)
        {
            PseudoClasses.Set(PC_ItemsRight,
                change.GetNewValue<CommandBarItemsAlignment>() == CommandBarItemsAlignment.Right);
        }
    }

    // ── Measure — dynamic overflow ────────────────────────────────────────────

    protected override Size MeasureOverride(Size availableSize)
    {
        if (!IsDynamicOverflowEnabled || _primaryItems is null || _moreButton is null)
            return base.MeasureOverride(availableSize);

        _availableSize = availableSize;

        // First pass: measure everything unconstrained.
        var sz = base.MeasureOverride(Size.Infinity);

        if (_primaryCommands.Count == 0)
        {
            UpdateMoreButtonVisibility();
            return sz;
        }

        double contentWidth = _contentHost?.DesiredSize.Width ?? 0;
        double moreWidth    = _moreButton.DesiredSize.Width;
        double available    = availableSize.Width - contentWidth - moreWidth - 5;

        double primaryWidth = _primaryItemsHost?.DesiredSize.Width ?? 0;

        // Try to recover items from overflow if space has grown.
        if (_minRecoverWidth < available && _numInOverflow > 0)
        {
            double tracked = primaryWidth;
            while (_numInOverflow > 0)
            {
                var toReturn = GetReturnToPrimaryItems();
                if (toReturn is null) break;

                double groupWidth = 0;
                foreach (var item in toReturn)
                {
                    groupWidth += GetCachedWidth(item);
                    _overflowItems!.Remove(item);
                    int origIdx = Math.Min(_primaryItems.Count, _primaryCommands.IndexOf(item));
                    _primaryItems.Insert(origIdx, item);
                    _numInOverflow--;
                    if (item is CommandBarSeparator sep) sep.IsVisible = true;
                }

                tracked          += groupWidth;
                _minRecoverWidth += groupWidth;

                if (tracked >= available) break;
            }
        }
        else if (primaryWidth > available)
        {
            // Push items into overflow.
            double tracked = 0;
            while (_primaryItemsHost!.DesiredSize.Width - tracked > available)
            {
                var toOverflow = GetNextItemsToOverflow();
                if (toOverflow is null) break;

                foreach (var item in toOverflow)
                {
                    double w = (item as Control)?.DesiredSize.Width ?? 0;
                    CacheWidth(item, w);
                    tracked += w;
                    _primaryItems.Remove(item);
                    _overflowItems!.Insert(_numInOverflow, item);
                    _numInOverflow++;
                    if (item is CommandBarSeparator sep) sep.IsVisible = false;
                }
            }

            _minRecoverWidth = _primaryItemsHost.DesiredSize.Width;
        }

        // Keep the overflow separator positioned after dynamic-overflow items.
        if (_overflowSeparator is not null && _overflowItems is not null)
        {
            _overflowSeparator.IsVisible = _numInOverflow > 0 && _secondaryCommands.Count > 0;
            int sepIdx = _overflowItems.IndexOf(_overflowSeparator);
            if (sepIdx >= 0 && sepIdx != _numInOverflow)
                _overflowItems.Move(sepIdx, _numInOverflow);
        }

        UpdateMoreButtonVisibility();
        return base.MeasureOverride(availableSize);
    }

    // ── Open / close ──────────────────────────────────────────────────────────

    protected virtual void OnOpening() => Opening?.Invoke(this, EventArgs.Empty);
    protected virtual void OnOpened()  => Opened?.Invoke(this, EventArgs.Empty);
    protected virtual void OnClosing() => Closing?.Invoke(this, EventArgs.Empty);
    protected virtual void OnClosed()
    {
        Closed?.Invoke(this, EventArgs.Empty);
        _moreButton?.Focus();
    }

    private void OnIsOpenChanged(bool open)
    {
        if (open)
        {
            OnOpening();
            PseudoClasses.Set(PC_Open, true);
            ApplyOpenStateToItems(true);
            OnOpened();
        }
        else
        {
            OnClosing();
            PseudoClasses.Set(PC_Open, false);
            ApplyOpenStateToItems(false);
            OnClosed();
        }
    }

    private void OnMoreButtonClick(object? sender, RoutedEventArgs e)
        => IsOpen = !IsOpen;

    // ── Collection change handlers ────────────────────────────────────────────

    private void OnPrimaryCommandsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!_templateApplied) return;

        if (_primaryItems is null)
        {
            AttachItems();
            goto SetState;
        }

        if (IsDynamicOverflowEnabled)
            ReturnOverflowToPrimary();

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var item in e.NewItems!.Cast<ICommandBarElement>())
                    if (item.DynamicOverflowOrder != 0) _hasOrderedOverflow++;
                _primaryItems.InsertRange(e.NewStartingIndex, e.NewItems!.Cast<ICommandBarElement>());
                break;

            case NotifyCollectionChangedAction.Remove:
                foreach (var item in e.OldItems!.Cast<ICommandBarElement>())
                    if (item.DynamicOverflowOrder != 0) _hasOrderedOverflow--;
                _primaryItems.RemoveAll(e.OldItems!.Cast<ICommandBarElement>());
                break;

            case NotifyCollectionChangedAction.Reset:
                _hasOrderedOverflow = 0;
                _primaryItems.Clear();
                break;

            case NotifyCollectionChangedAction.Replace:
                foreach (var item in e.OldItems!.Cast<ICommandBarElement>())
                    if (item.DynamicOverflowOrder != 0) _hasOrderedOverflow--;
                _primaryItems.RemoveRange(e.OldStartingIndex, e.OldItems!.Count);
                _primaryItems.InsertRange(e.NewStartingIndex, e.NewItems!.Cast<ICommandBarElement>());
                foreach (var item in e.NewItems!.Cast<ICommandBarElement>())
                    if (item.DynamicOverflowOrder != 0) _hasOrderedOverflow++;
                break;

            case NotifyCollectionChangedAction.Move:
                _primaryItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                break;
        }

        SetState:
        UpdateCommandStatePseudoClasses();
        InvalidateMeasure();
    }

    private void OnSecondaryCommandsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!_templateApplied) return;

        if (_overflowItems is null)
        {
            AttachItems();
            goto SetState;
        }

        // Secondary items start after the dynamic-overflow items + separator.
        int startIndex = _numInOverflow == 0 ? 0 : _numInOverflow + 1;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                for (int i = 0; i < e.NewItems!.Count; i++)
                    _overflowItems.Insert(e.NewStartingIndex + i + startIndex,
                        (ICommandBarElement)e.NewItems[i]!);
                break;

            case NotifyCollectionChangedAction.Remove:
                for (int i = 0; i < e.OldItems!.Count; i++)
                    _overflowItems.RemoveAt(e.OldStartingIndex + i + startIndex);
                break;

            case NotifyCollectionChangedAction.Reset:
                _overflowItems.RemoveRange(startIndex, _overflowItems.Count - startIndex);
                break;

            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Move:
                for (int i = 0; i < e.OldItems!.Count; i++)
                    _overflowItems.RemoveAt(e.OldStartingIndex + i + startIndex);
                for (int i = 0; i < e.NewItems!.Count; i++)
                    _overflowItems.Insert(e.NewStartingIndex + i + startIndex,
                        (ICommandBarElement)e.NewItems[i]!);
                break;
        }

        SetState:
        UpdateCommandStatePseudoClasses();
        InvalidateMeasure();
    }

    // ── Initialisation ────────────────────────────────────────────────────────

    private void AttachItems()
    {
        var labelPos = DefaultLabelPosition;

        if (_primaryCommands.Count > 0)
        {
            _primaryItems = new AvaloniaList<ICommandBarElement>();
            _primaryItems.CollectionChanged += OnPrimaryItemsCollectionChanged;

            foreach (var item in _primaryCommands)
            {
                if (item.DynamicOverflowOrder != 0) _hasOrderedOverflow++;
                ApplyLabelPositionToItem(item, labelPos);
            }

            _primaryItems.AddRange(_primaryCommands);

            if (_primaryItemsHost is not null)
                _primaryItemsHost.ItemsSource = _primaryItems;
        }

        if (_secondaryCommands.Count > 0 || IsDynamicOverflowEnabled)
        {
            _overflowSeparator = new CommandBarSeparator { IsVisible = false };
            _overflowItems     = new AvaloniaList<ICommandBarElement> { _overflowSeparator };
            _overflowItems.AddRange(_secondaryCommands);

            if (_overflowItemsHost is not null)
                _overflowItemsHost.ItemsSource = _overflowItems;
        }

        UpdateCommandStatePseudoClasses();

        // Apply current open state to newly attached items.
        if (IsOpen)
            ApplyOpenStateToItems(true);
    }

    // ── Primary items collection — label/open pseudo-class propagation ─────────

    private void OnPrimaryItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var pos = DefaultLabelPosition;

        if (e.Action is NotifyCollectionChangedAction.Add && e.NewItems is not null)
        {
            foreach (var item in e.NewItems.Cast<ICommandBarElement>())
                ApplyLabelPositionToItem(item, pos);
        }
        else if (e.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Reset
                 && e.OldItems is not null)
        {
            foreach (var item in e.OldItems.Cast<ICommandBarElement>())
                ClearLabelPositionOnItem(item);
        }
    }

    // ── Dynamic overflow helpers ──────────────────────────────────────────────

    private void ReturnOverflowToPrimary()
    {
        if (_overflowItems is null || _primaryItems is null) return;

        for (int i = _numInOverflow - 1; i >= 0; i--)
        {
            var item = _overflowItems[i];
            _overflowItems.RemoveAt(i);
            int origIdx = Math.Min(_primaryItems.Count, _primaryCommands.IndexOf(item));
            _primaryItems.Insert(origIdx, item);
        }

        _numInOverflow = 0;
    }

    private IList<ICommandBarElement>? GetNextItemsToOverflow()
    {
        if (_primaryItems is null || _primaryItems.Count == 0) return null;

        if (_hasOrderedOverflow > 0)
        {
            int nextOrder = int.MaxValue;
            bool found    = false;

            for (int i = _primaryItems.Count - 1; i >= 0; i--)
            {
                int order = _primaryItems[i].DynamicOverflowOrder;
                if (order == 0) continue;
                found    = true;
                nextOrder = Math.Min(nextOrder, order);
            }

            if (found)
            {
                var group = new List<ICommandBarElement>();
                for (int i = 0; i < _primaryItems.Count; i++)
                    if (_primaryItems[i].DynamicOverflowOrder == nextOrder)
                        group.Add(_primaryItems[i]);
                return group;
            }
        }

        return new[] { _primaryItems[_primaryItems.Count - 1] };
    }

    private IList<ICommandBarElement>? GetReturnToPrimaryItems()
    {
        if (_overflowItems is null || _numInOverflow == 0) return null;

        var last = _overflowItems[_numInOverflow - 1];
        if (last.DynamicOverflowOrder == 0)
            return new[] { last };

        int group = last.DynamicOverflowOrder;
        int count = 1;
        for (int i = _numInOverflow - 2; i >= 0; i--)
        {
            if (_overflowItems[i].DynamicOverflowOrder == group) count++;
            else break;
        }

        return _overflowItems.GetRange(_numInOverflow - count, count).ToList();
    }

    private void CacheWidth(ICommandBarElement item, double width)
    {
        _widthCache ??= new Dictionary<ICommandBarElement, double>();
        _widthCache[item] = width;
    }

    private double GetCachedWidth(ICommandBarElement item)
        => _widthCache?.TryGetValue(item, out double w) == true ? w : 0;

    private void UpdateMoreButtonVisibility()
    {
        if (_moreButton is null) return;

        var vis = OverflowButtonVisibility;
        if (vis == CommandBarOverflowButtonVisibility.Auto)
        {
            bool hasDynamic  = IsDynamicOverflowEnabled && _numInOverflow > 0;
            bool hasSecondary = _secondaryCommands.Count > 0;
            _moreButton.IsVisible = hasDynamic || hasSecondary;
        }
        else
        {
            _moreButton.IsVisible = vis == CommandBarOverflowButtonVisibility.Visible;
        }
    }

    // ── Visual state helpers ──────────────────────────────────────────────────

    private void UpdateCommandStatePseudoClasses()
    {
        PseudoClasses.Set(PC_PrimaryOnly,   _primaryCommands.Count > 0 && _secondaryCommands.Count == 0);
        PseudoClasses.Set(PC_SecondaryOnly, _primaryCommands.Count == 0 && _secondaryCommands.Count > 0);
    }

    private void ApplyLabelPositionToItems(CommandBarDefaultLabelPosition pos)
    {
        if (_primaryItems is null) return;
        foreach (var item in _primaryItems)
            ApplyLabelPositionToItem(item, pos);
    }

    private static void ApplyLabelPositionToItem(ICommandBarElement item, CommandBarDefaultLabelPosition pos)
    {
        switch (item)
        {
            case CommandBarButton btn:    btn.ApplyLabelPosition(pos);    break;
            case CommandBarToggleButton tb: tb.ApplyLabelPosition(pos);   break;
        }
    }

    private static void ClearLabelPositionOnItem(ICommandBarElement item)
    {
        switch (item)
        {
            case CommandBarButton btn:    btn.ApplyLabelPosition(CommandBarDefaultLabelPosition.Bottom); break;
            case CommandBarToggleButton tb: tb.ApplyLabelPosition(CommandBarDefaultLabelPosition.Bottom); break;
        }
    }

    private void ApplyOpenStateToItems(bool open)
    {
        if (_primaryItems is null) return;
        foreach (var item in _primaryItems)
        {
            switch (item)
            {
                case CommandBarButton btn:    btn.ApplyOpenState(open);    break;
                case CommandBarToggleButton tb: tb.ApplyOpenState(open);   break;
            }
        }
    }
}
