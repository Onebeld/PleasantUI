using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.VisualTree;

namespace PleasantUI.Controls.Docking;

/// <summary>
/// Represents a sidebar in the docking system that can host dockable buttons.
/// </summary>
[TemplatePart("PART_Grid", typeof(Grid))]
[TemplatePart("PART_UpperStack", typeof(StackPanel))]
[TemplatePart("PART_LowerStack", typeof(StackPanel))]
[TemplatePart("PART_UpperTopTools", typeof(ItemsControl))]
[TemplatePart("PART_UpperBottomTools", typeof(ItemsControl))]
[TemplatePart("PART_LowerTopTools", typeof(ItemsControl))]
[TemplatePart("PART_LowerBottomTools", typeof(ItemsControl))]
[TemplatePart("PART_UpperDivider", typeof(Border))]
[TemplatePart("PART_LowerDivider", typeof(Border))]
public class SideBar : TemplatedControl
{
    /// <summary>Defines the <see cref="Location"/> property.</summary>
    public static readonly StyledProperty<SideBarLocation> LocationProperty =
        AvaloniaProperty.Register<SideBar, SideBarLocation>(nameof(Location));

    /// <summary>Defines the <see cref="UpperTopToolsSource"/> property.</summary>
    public static readonly StyledProperty<IEnumerable?> UpperTopToolsSourceProperty =
        AvaloniaProperty.Register<SideBar, IEnumerable?>(nameof(UpperTopToolsSource));

    /// <summary>Defines the <see cref="UpperBottomToolsSource"/> property.</summary>
    public static readonly StyledProperty<IEnumerable?> UpperBottomToolsSourceProperty =
        AvaloniaProperty.Register<SideBar, IEnumerable?>(nameof(UpperBottomToolsSource));

    /// <summary>Defines the <see cref="LowerTopToolsSource"/> property.</summary>
    public static readonly StyledProperty<IEnumerable?> LowerTopToolsSourceProperty =
        AvaloniaProperty.Register<SideBar, IEnumerable?>(nameof(LowerTopToolsSource));

    /// <summary>Defines the <see cref="LowerBottomToolsSource"/> property.</summary>
    public static readonly StyledProperty<IEnumerable?> LowerBottomToolsSourceProperty =
        AvaloniaProperty.Register<SideBar, IEnumerable?>(nameof(LowerBottomToolsSource));

    /// <summary>Defines the <see cref="ItemTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<SideBar, IDataTemplate?>(nameof(ItemTemplate));

    /// <summary>Defines the <see cref="Spacing"/> property.</summary>
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<SideBar, double>(nameof(Spacing), 8.0);

    /// <summary>Defines the <see cref="ButtonWidth"/> property.</summary>
    public static readonly StyledProperty<double> ButtonWidthProperty =
        AvaloniaProperty.Register<SideBar, double>(nameof(ButtonWidth), 32.0);

    /// <summary>Defines the <see cref="ButtonHeight"/> property.</summary>
    public static readonly StyledProperty<double> ButtonHeightProperty =
        AvaloniaProperty.Register<SideBar, double>(nameof(ButtonHeight), 32.0);

    /// <summary>Defines the <see cref="IsDragDropEnabled"/> property.</summary>
    public static readonly StyledProperty<bool> IsDragDropEnabledProperty =
        AvaloniaProperty.Register<SideBar, bool>(nameof(IsDragDropEnabled), true);

    private Grid? _grid;
    private StackPanel? _upperStack;
    private StackPanel? _lowerStack;
    private ItemsControl? _upperTopTools;
    private ItemsControl? _upperBottomTools;
    private ItemsControl? _lowerTopTools;
    private ItemsControl? _lowerBottomTools;
    private Border? _upperDivider;
    private Border? _lowerDivider;
    private Border? _dragGhost;
    private AdornerLayer? _layer;
    private ReDockHost? _host;
    private SideBarButtonLocation[]? _supportedLocations;

    /// <summary>Initializes a new instance of the <see cref="SideBar"/> class.</summary>
    public SideBar()
    {
        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
        AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
        AddHandler(DragDrop.DropEvent, OnDrop);
    }
    /// <summary>Gets or sets the location of the sidebar (left or right).</summary>
    public SideBarLocation Location
    {
        get => GetValue(LocationProperty);
        set => SetValue(LocationProperty, value);
    }

    /// <summary>Gets or sets the items source for the upper top tools section.</summary>
    public IEnumerable? UpperTopToolsSource
    {
        get => GetValue(UpperTopToolsSourceProperty);
        set => SetValue(UpperTopToolsSourceProperty, value);
    }

    /// <summary>Gets or sets the items source for the upper bottom tools section.</summary>
    public IEnumerable? UpperBottomToolsSource
    {
        get => GetValue(UpperBottomToolsSourceProperty);
        set => SetValue(UpperBottomToolsSourceProperty, value);
    }

    /// <summary>Gets or sets the items source for the lower top tools section.</summary>
    public IEnumerable? LowerTopToolsSource
    {
        get => GetValue(LowerTopToolsSourceProperty);
        set => SetValue(LowerTopToolsSourceProperty, value);
    }

    /// <summary>Gets or sets the items source for the lower bottom tools section.</summary>
    public IEnumerable? LowerBottomToolsSource
    {
        get => GetValue(LowerBottomToolsSourceProperty);
        set => SetValue(LowerBottomToolsSourceProperty, value);
    }

    /// <summary>Gets or sets the item template for the sidebar buttons.</summary>
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>Gets or sets the spacing between buttons.</summary>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <summary>Gets or sets the width of buttons.</summary>
    public double ButtonWidth
    {
        get => GetValue(ButtonWidthProperty);
        set => SetValue(ButtonWidthProperty, value);
    }

    /// <summary>Gets or sets the height of buttons.</summary>
    public double ButtonHeight
    {
        get => GetValue(ButtonHeightProperty);
        set => SetValue(ButtonHeightProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether drag and drop is enabled.</summary>
    public bool IsDragDropEnabled
    {
        get => GetValue(IsDragDropEnabledProperty);
        set => SetValue(IsDragDropEnabledProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _upperTopTools = e.NameScope.Get<ItemsControl>("PART_UpperTopTools");
        _upperBottomTools = e.NameScope.Get<ItemsControl>("PART_UpperBottomTools");
        _lowerTopTools = e.NameScope.Get<ItemsControl>("PART_LowerTopTools");
        _lowerBottomTools = e.NameScope.Get<ItemsControl>("PART_LowerBottomTools");
        _grid = e.NameScope.Get<Grid>("PART_Grid");
        _upperStack = e.NameScope.Get<StackPanel>("PART_UpperStack");
        _lowerStack = e.NameScope.Get<StackPanel>("PART_LowerStack");
        _upperDivider = e.NameScope.Get<Border>("PART_UpperDivider");
        _lowerDivider = e.NameScope.Get<Border>("PART_LowerDivider");

        UpdateDividerVisibility();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == UpperTopToolsSourceProperty || change.Property == UpperBottomToolsSourceProperty)
        {
            if (change.OldValue is INotifyCollectionChanged oldColl)
                oldColl.CollectionChanged -= OnUpperToolsCollectionChanged;
            if (change.NewValue is INotifyCollectionChanged newColl)
                newColl.CollectionChanged += OnUpperToolsCollectionChanged;
            UpdateUpperDividerVisibility();
        }
        else if (change.Property == LowerTopToolsSourceProperty || change.Property == LowerBottomToolsSourceProperty)
        {
            if (change.OldValue is INotifyCollectionChanged oldColl)
                oldColl.CollectionChanged -= OnLowerToolsCollectionChanged;
            if (change.NewValue is INotifyCollectionChanged newColl)
                newColl.CollectionChanged += OnLowerToolsCollectionChanged;
            UpdateLowerDividerVisibility();
        }
    }

    /// <inheritdoc/>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        _host = this.FindLogicalAncestorOfType<ReDockHost>();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        _host = null;
    }

    /// <summary>Sets the grid hit test visibility.</summary>
    internal void SetGridHitTestVisible(bool value)
    {
        if (_grid != null) _grid.IsHitTestVisible = value;
    }

    private void OnUpperToolsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => UpdateUpperDividerVisibility();

    private void OnLowerToolsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => UpdateLowerDividerVisibility();

    private void UpdateDividerVisibility()
    {
        UpdateUpperDividerVisibility();
        UpdateLowerDividerVisibility();
    }

    private void UpdateUpperDividerVisibility()
    {
        if (_upperDivider == null) return;

        if (SupportsLocation(SideBarButtonLocation.UpperTop) && SupportsLocation(SideBarButtonLocation.UpperBottom))
            _upperDivider.IsVisible = HasItems(UpperTopToolsSource) && HasItems(UpperBottomToolsSource);
        else
            _upperDivider.IsVisible = false;
    }

    private void UpdateLowerDividerVisibility()
    {
        if (_lowerDivider == null) return;

        if (SupportsLocation(SideBarButtonLocation.LowerTop) && SupportsLocation(SideBarButtonLocation.LowerBottom))
            _lowerDivider.IsVisible = HasItems(LowerTopToolsSource) && HasItems(LowerBottomToolsSource);
        else
            _lowerDivider.IsVisible = false;
    }

    private static bool HasItems(IEnumerable? source)
    {
        if (source == null) return false;
        var enumerator = source.GetEnumerator();
        try { return enumerator.MoveNext(); }
        finally { (enumerator as IDisposable)?.Dispose(); }
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        var (location, index) = DetermineLocation(e);
        OnDragLeave(sender, e);

        SideBar? oldSideBar = null;
        try
        {
            if (!e.DataTransfer.Contains(SideBarButton.DragFormat) ||
                SideBarButton.CurrentDragButton is not { DockLocation: not null } button)
                return;

            if (index < 0) return;

            oldSideBar = button.FindAncestorOfType<SideBar>();
            if (oldSideBar == null) return;

            var args = new SideBarButtonMoveEventArgs(ReDockHost.ButtonMoveEvent, this)
            {
                Item = button.DataContext,
                Button = button,
                SourceSideBar = oldSideBar,
                SourceLocation = button.DockLocation,
                DestinationSideBar = this,
                DestinationLocation = new DockAreaLocation(location, Location),
                DestinationIndex = index
            };

            RaiseEvent(args);

            if (args.Handled) return;

            var newItemsSource = location switch
            {
                SideBarButtonLocation.UpperTop => UpperTopToolsSource,
                SideBarButtonLocation.UpperBottom => UpperBottomToolsSource,
                SideBarButtonLocation.LowerTop => LowerTopToolsSource,
                SideBarButtonLocation.LowerBottom => LowerBottomToolsSource,
                _ => null
            };

            var itemsControl = button.FindAncestorOfType<ItemsControl>();
            var items = itemsControl?.ItemsSource;

            if (items is not IList oldSource || newItemsSource is not IList newSource) return;

            if (oldSource.Contains(button.DataContext))
            {
                oldSource.Remove(button.DataContext);
                newSource.Insert(index, button.DataContext);
            }
            else if (oldSource.Contains(button))
            {
                oldSource.Remove(button);
                newSource.Insert(index, button);
            }
        }
        finally
        {
            UpdateDividerVisibility();
            oldSideBar?.UpdateDividerVisibility();
        }
    }

    private void OnDragEnter(object? sender, DragEventArgs e)
    {
        if (!e.DataTransfer.Contains(SideBarButton.DragFormat)) return;

        SetGridHitTestVisible(false);
        _supportedLocations = _host?.DockAreas
            .Where(i => i.Location.LeftRight == Location)
            .Select(i => i.Location.ButtonLocation)
            .ToArray();

        if (_upperDivider != null)
            _upperDivider.IsVisible = SupportsLocation(SideBarButtonLocation.UpperTop) && SupportsLocation(SideBarButtonLocation.UpperBottom);
        if (_lowerDivider != null)
            _lowerDivider.IsVisible = SupportsLocation(SideBarButtonLocation.LowerTop) && SupportsLocation(SideBarButtonLocation.LowerBottom);

        _dragGhost = new Border
        {
            [!Border.BackgroundProperty] = new DynamicResourceExtension("PleasantDockingGhostBackground"),
            IsHitTestVisible = false,
            Width = ButtonWidth,
            Height = ButtonHeight,
            CornerRadius = new CornerRadius(4)
        };

        _layer = AdornerLayer.GetAdornerLayer(this);
        _layer?.Children.Add(_dragGhost);

        OnDragOver(sender, e);
    }

    private void OnDragLeave(object? sender, DragEventArgs e)
    {
        if (_upperTopTools == null || _upperBottomTools == null ||
            _lowerBottomTools == null || _lowerTopTools == null) return;

        if (!e.DataTransfer.Contains(SideBarButton.DragFormat)) return;

        SetGridHitTestVisible(true);

        foreach (Control item in _upperTopTools.GetRealizedContainers()
            .Concat(_upperBottomTools.GetRealizedContainers())
            .Concat(_lowerBottomTools.GetRealizedContainers())
            .Concat(_lowerTopTools.GetRealizedContainers()))
        {
            item.Margin = default;
        }

        _upperTopTools.Margin = default;
        _upperBottomTools.Margin = default;
        _lowerTopTools.Margin = default;
        _lowerBottomTools.Margin = default;

        UpdateDividerVisibility();

        if (_dragGhost != null && _layer != null)
        {
            _layer.Children.Remove(_dragGhost);
            _layer = null;
            _dragGhost = null;
        }
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        if (_upperTopTools == null || _upperBottomTools == null || _lowerTopTools == null ||
            _lowerBottomTools == null || _grid == null || _upperStack == null || _lowerStack == null ||
            _layer == null || _dragGhost == null)
            return;

        if (!e.DataTransfer.Contains(SideBarButton.DragFormat)) return;

        const double Spacing = 8;
        var Size = ButtonHeight;

        _grid.IsHitTestVisible = false;

        var spaceBetween = _grid.Bounds.Height - (_upperStack.Bounds.Height + _lowerStack.Bounds.Height);
        if (spaceBetween < 0) spaceBetween = 16;

        bool handled = false;
        double pad = 0;

        if (SupportsLocation(SideBarButtonLocation.UpperTop))
        {
            int visibleCount = 0;
            for (int i = 0; i < _upperTopTools.ItemCount; i++)
            {
                Control? item = _upperTopTools.ContainerFromIndex(i);
                if (item?.IsVisible != true) continue;
                visibleCount++;
                var clientPos = e.GetPosition(item);
                if (clientPos.Y + item.Margin.Top < item.Bounds.Height / 2 && !handled)
                {
                    SetGhostYPosition(item, -pad - item.Margin.Top);
                    item.Margin = new Thickness(0, Size + Spacing, 0, 0);
                    handled = true;
                    pad = 0;
                }
                else
                {
                    pad += item.Margin.Top;
                    item.Margin = default;
                }
            }

            var ctrlPos = e.GetPosition(_upperTopTools);
            if (ctrlPos.Y < _upperTopTools.Bounds.Height + Spacing && !handled)
            {
                if (visibleCount == 0)
                {
                    SetGhostYPosition(_upperTopTools, _upperTopTools.Bounds.Height - pad);
                    _upperTopTools.Margin = new Thickness(0, 0, 0, Size);
                }
                else
                {
                    SetGhostYPosition(_upperTopTools, _upperTopTools.Bounds.Height + Spacing - pad);
                    _upperTopTools.Margin = new Thickness(0, 0, 0, Size + Spacing);
                }
                handled = true;
            }
            else
            {
                pad = _upperTopTools.Margin.Bottom;
                _upperTopTools.Margin = default;
            }
        }

        if (SupportsLocation(SideBarButtonLocation.UpperBottom))
        {
            int visibleCount = 0;
            for (int i = 0; i < _upperBottomTools.ItemCount; i++)
            {
                Control? item = _upperBottomTools.ContainerFromIndex(i);
                if (item?.IsVisible != true) continue;
                visibleCount++;
                var clientPos = e.GetPosition(item);
                if (clientPos.Y + item.Margin.Top < item.Bounds.Height / 2 && !handled)
                {
                    SetGhostYPosition(item, -pad - item.Margin.Top);
                    item.Margin = new Thickness(0, Size + Spacing, 0, 0);
                    handled = true;
                    pad = 0;
                }
                else
                {
                    pad += item.Margin.Top;
                    item.Margin = default;
                }
            }

            var ctrlPos = e.GetPosition(_upperBottomTools);
            if (ctrlPos.Y < _upperBottomTools.Bounds.Height + spaceBetween / 2 && !handled)
            {
                if (visibleCount == 0)
                    SetGhostYPosition(_upperBottomTools, _upperBottomTools.Bounds.Height - pad);
                else
                    SetGhostYPosition(_upperBottomTools, _upperBottomTools.Bounds.Height + Spacing - pad);
                handled = true;
            }
        }

        pad = 0;

        if (SupportsLocation(SideBarButtonLocation.LowerBottom))
        {
            int visibleCount = 0;
            for (int i = _lowerBottomTools.ItemCount - 1; i >= 0; i--)
            {
                Control? item = _lowerBottomTools.ContainerFromIndex(i);
                if (item?.IsVisible != true) continue;
                visibleCount++;
                var clientPos = e.GetPosition(item);
                if (clientPos.Y > item.Bounds.Height / 2 && !handled)
                {
                    SetGhostYPosition(item, pad + item.Margin.Bottom);
                    item.Margin = new Thickness(0, 0, 0, Size + Spacing);
                    handled = true;
                }
                else
                {
                    pad += item.Margin.Bottom;
                    item.Margin = default;
                }
            }

            var ctrlPos = e.GetPosition(_lowerBottomTools);
            if (ctrlPos.Y > -8 && !handled)
            {
                if (visibleCount == 0)
                {
                    SetGhostYPosition(_lowerTopTools, (Spacing * 2) + pad + _lowerTopTools.Bounds.Height);
                    _lowerBottomTools.Margin = new Thickness(0, Size, 0, 0);
                }
                else
                {
                    SetGhostYPosition(_lowerBottomTools, -(Size + Spacing) + pad);
                    _lowerBottomTools.Margin = new Thickness(0, Size + Spacing, 0, 0);
                }
                handled = true;
            }
            else
            {
                pad = _lowerBottomTools.Margin.Top;
                _lowerBottomTools.Margin = default;
            }
        }

        if (SupportsLocation(SideBarButtonLocation.LowerTop))
        {
            int visibleCount = 0;
            for (int i = _lowerTopTools.ItemCount - 1; i >= 0; i--)
            {
                Control? item = _lowerTopTools.ContainerFromIndex(i);
                if (item?.IsVisible != true) continue;
                visibleCount++;
                var clientPos = e.GetPosition(item);
                if (clientPos.Y > item.Bounds.Height / 2 && !handled)
                {
                    SetGhostYPosition(item, pad + item.Margin.Bottom);
                    item.Margin = new Thickness(0, 0, 0, Size + Spacing);
                    handled = true;
                }
                else
                {
                    pad += item.Margin.Bottom;
                    item.Margin = default;
                }
            }

            var ctrlPos = e.GetPosition(_lowerTopTools);
            if (ctrlPos.Y < _lowerTopTools.Bounds.Height + spaceBetween / 2 && !handled)
            {
                if (visibleCount == 0)
                    SetGhostYPosition(_lowerTopTools, pad - _dragGhost.Bounds.Height);
                else
                    SetGhostYPosition(_lowerTopTools, -Spacing + pad - _dragGhost.Bounds.Height);
            }
        }
    }

    private (SideBarButtonLocation location, int index) DetermineLocation(DragEventArgs e)
    {
        if (_upperTopTools == null || _upperBottomTools == null ||
            _lowerTopTools == null || _lowerBottomTools == null || _grid == null ||
            _upperStack == null || _lowerStack == null)
            return (default, -1);

        var spaceBetween = _grid.Bounds.Height - (_upperStack.Bounds.Height + _lowerStack.Bounds.Height);
        if (spaceBetween < 0) spaceBetween = 16;

        const double Spacing = 8;

        if (SupportsLocation(SideBarButtonLocation.UpperTop))
        {
            for (int i = 0; i < _upperTopTools.ItemCount; i++)
            {
                Control? item = _upperTopTools.ContainerFromIndex(i);
                if (item?.IsVisible != true) continue;
                var pos = e.GetPosition(item);
                if (pos.Y + item.Margin.Top < item.Bounds.Height / 2)
                    return (SideBarButtonLocation.UpperTop, i);
            }

            var ctrlPos = e.GetPosition(_upperTopTools);
            if (ctrlPos.Y < _upperTopTools.Bounds.Height + Spacing)
                return (SideBarButtonLocation.UpperTop, _upperTopTools.ItemCount);
        }

        if (SupportsLocation(SideBarButtonLocation.UpperBottom))
        {
            for (int i = 0; i < _upperBottomTools.ItemCount; i++)
            {
                Control? item = _upperBottomTools.ContainerFromIndex(i);
                if (item?.IsVisible != true) continue;
                var pos = e.GetPosition(item);
                if (pos.Y + item.Margin.Top < item.Bounds.Height / 2)
                    return (SideBarButtonLocation.UpperBottom, i);
            }

            var ctrlPos = e.GetPosition(_upperBottomTools);
            if (ctrlPos.Y < _upperBottomTools.Bounds.Height + spaceBetween / 2)
                return (SideBarButtonLocation.UpperBottom, _upperBottomTools.ItemCount);
        }

        if (SupportsLocation(SideBarButtonLocation.LowerTop))
        {
            var ctrlPos = e.GetPosition(_lowerTopTools);
            if (ctrlPos.Y < 0)
                return (SideBarButtonLocation.LowerTop, 0);

            for (int i = 0; i < _lowerTopTools.ItemCount; i++)
            {
                Control? item = _lowerTopTools.ContainerFromIndex(i);
                if (item?.IsVisible != true) continue;
                var pos = e.GetPosition(item);
                if (pos.Y < item.Bounds.Height / 2)
                    return (SideBarButtonLocation.LowerTop, i);
            }

            ctrlPos = e.GetPosition(_lowerTopTools);
            if (ctrlPos.Y < _lowerTopTools.Bounds.Height + 8)
                return (SideBarButtonLocation.LowerTop, _lowerTopTools.ItemCount);
        }

        if (SupportsLocation(SideBarButtonLocation.LowerBottom))
        {
            var ctrlPos = e.GetPosition(_lowerBottomTools);
            if (ctrlPos.Y < -8)
                return (SideBarButtonLocation.LowerBottom, 0);

            for (int i = 0; i < _lowerBottomTools.ItemCount; i++)
            {
                Control? item = _lowerBottomTools.ContainerFromIndex(i);
                if (item?.IsVisible != true) continue;
                var pos = e.GetPosition(item);
                if (pos.Y < item.Bounds.Height / 2)
                    return (SideBarButtonLocation.LowerBottom, i);
            }

            ctrlPos = e.GetPosition(_lowerBottomTools);
            if (ctrlPos.Y > _lowerBottomTools.Bounds.Height - 16)
                return (SideBarButtonLocation.LowerBottom, _lowerBottomTools.ItemCount);
        }

        return (default, -1);
    }

    private void SetGhostYPosition(Control @base, double y)
    {
        if (_dragGhost == null || _layer == null) return;
        _dragGhost.Margin = (@base.TranslatePoint(new Point(0, y), _layer) ?? default).ToThickness();
    }

    private bool SupportsLocation(SideBarButtonLocation location)
        => _supportedLocations?.Contains(location) != false;
}
