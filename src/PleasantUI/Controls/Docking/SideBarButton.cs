using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace PleasantUI.Controls.Docking;

/// <summary>
/// Represents a button in the sidebar of the docking system.
/// </summary>
public class SideBarButton : ToggleButton
{
    /// <summary>
    /// String format token used to signal a SideBarButton drag operation.
    /// </summary>
    internal static readonly DataFormat<string> DragFormat =
        DataFormat.CreateStringApplicationFormat("SideBarButton");

    /// <summary>
    /// The button currently being dragged. Set before drag starts, cleared after.
    /// </summary>
    internal static SideBarButton? CurrentDragButton { get; private set; }

    /// <summary>Defines the <see cref="DisplayMode"/> property.</summary>
    public static readonly StyledProperty<DockableDisplayMode> DisplayModeProperty =
        AvaloniaProperty.Register<SideBarButton, DockableDisplayMode>(nameof(DisplayMode),
            defaultValue: DockableDisplayMode.Docked);

    private bool _canDrag;
    private Point _startPoint;
    private PointerPressedEventArgs? _pressedArgs;

    /// <summary>Gets or sets the display mode of the button.</summary>
    public DockableDisplayMode DisplayMode
    {
        get => GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    /// <summary>Gets the dock location of the button.</summary>
    public DockAreaLocation? DockLocation { get; internal set; }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        var itemsControl = this.FindAncestorOfType<ItemsControl>();
        var location = itemsControl?.Name switch
        {
            "PART_UpperTopTools" => SideBarButtonLocation.UpperTop,
            "PART_UpperBottomTools" => SideBarButtonLocation.UpperBottom,
            "PART_LowerTopTools" => SideBarButtonLocation.LowerTop,
            "PART_LowerBottomTools" => SideBarButtonLocation.LowerBottom,
            _ => default
        };

        var sideBar = this.FindAncestorOfType<SideBar>();
        if (sideBar != null)
            DockLocation = new DockAreaLocation(location, sideBar.Location);
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        DockLocation = null;
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            this.FindAncestorOfType<ReDockHost>()?.ShowFlyout(this);
        }
        else
        {
            _canDrag = true;
            _startPoint = e.GetPosition(this);
            _pressedArgs = e;
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _canDrag = false;
        _pressedArgs = null;
    }

    /// <inheritdoc/>
    protected override async void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (!_canDrag || _pressedArgs == null) return;

        var point = e.GetPosition(this);
        var threshold = Bounds.Height / 4;

        if (!(Math.Abs(point.X - _startPoint.X) > threshold) && !(Math.Abs(point.Y - _startPoint.Y) > threshold))
            return;

        _canDrag = false;
        var pressedArgs = _pressedArgs;
        _pressedArgs = null;

        var sideBar = this.FindAncestorOfType<SideBar>();
        sideBar?.SetGridHitTestVisible(false);

        IsVisible = false;
        if (Parent is ContentPresenter cp)
            cp.IsVisible = false;

        var item = DataTransferItem.Create(DragFormat, "drag");
        var data = new DataTransfer();
        data.Add(item);

        CurrentDragButton = this;
        await DragDrop.DoDragDropAsync(pressedArgs, data, DragDropEffects.Move);
        CurrentDragButton = null;

        sideBar?.SetGridHitTestVisible(true);
        IsVisible = true;
        if (Parent is ContentPresenter cp2)
            cp2.IsVisible = true;
    }
}
