using System.Collections;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Reactive;
using Avalonia.VisualTree;

namespace PleasantUI.Controls.Docking;

/// <summary>
/// Represents a button in the sidebar of the docking system.
/// </summary>
public class SideBarButton : ToggleButton
{
    internal static readonly DataFormat<string> DragFormat =
        DataFormat.CreateStringApplicationFormat("SideBarButton");

    internal static SideBarButton? CurrentDragButton { get; private set; }

    public static readonly StyledProperty<DockableDisplayMode> DisplayModeProperty =
        AvaloniaProperty.Register<SideBarButton, DockableDisplayMode>(nameof(DisplayMode),
            defaultValue: DockableDisplayMode.Docked);

    private bool _canDrag;
    private Point _startPoint;
    private PointerPressedEventArgs? _pressedArgs;

    /// <summary>
    /// Stores pending move information for deferred execution after drag completes.
    /// </summary>
    internal PendingMoveInfo? PendingMove { get; set; }

    public DockableDisplayMode DisplayMode
    {
        get => GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    public DockAreaLocation? DockLocation { get; internal set; }

    private string DebugTag => DataContext?.ToString() ?? "(no DC)";

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        var itemsControl = this.FindAncestorOfType<ItemsControl>();
        var location = itemsControl?.Name switch
        {
            "PART_UpperTopTools"    => SideBarButtonLocation.UpperTop,
            "PART_UpperBottomTools" => SideBarButtonLocation.UpperBottom,
            "PART_LowerTopTools"    => SideBarButtonLocation.LowerTop,
            "PART_LowerBottomTools" => SideBarButtonLocation.LowerBottom,
            _                       => default
        };

        var sideBar = this.FindAncestorOfType<SideBar>();
        if (sideBar != null)
        {
            DockLocation = new DockAreaLocation(location, sideBar.Location);
            Debug.WriteLine($"[SideBarButton] AttachedToVisualTree tag={DebugTag} DockLocation={DockLocation.ButtonLocation}/{DockLocation.LeftRight} itemsControl={itemsControl?.Name ?? "null"}");
        }
        else
        {
            Debug.WriteLine($"[SideBarButton] AttachedToVisualTree tag={DebugTag} — no SideBar ancestor found");
        }
        
        // Subscribe to IsChecked property changes.
        // _isCheckedSubscriptionReady guards against the initial emission that fires
        // synchronously on subscribe — we only want to react to actual user-driven
        // toggle events, not the construction-time value which fires before DockLocation
        // is set and before the button is fully in the visual tree.
        bool isCheckedSubscriptionReady = false;
        this.GetObservable(IsCheckedProperty)
            .Subscribe(new AnonymousObserver<bool?>(isChecked =>
            {
                if (!isCheckedSubscriptionReady)
                {
                    isCheckedSubscriptionReady = true;
                    return;
                }

                if (DockLocation == null)
                {
                    Debug.WriteLine($"[SideBarButton] IsCheckedChanged tag={DebugTag} IsChecked={isChecked} DockLocation=null - skipping");
                    return;
                }

                var host = this.FindAncestorOfType<ReDockHost>();
                if (host != null)
                {
                    host.HandleButtonToggle(this, isChecked == true);
                    Debug.WriteLine($"[SideBarButton] IsCheckedChanged tag={DebugTag} IsChecked={isChecked} DockLocation={DockLocation.ButtonLocation}/{DockLocation.LeftRight}");
                }
            }));
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Debug.WriteLine($"[SideBarButton] DetachedFromVisualTree tag={DebugTag} clearing DockLocation (was {DockLocation?.ButtonLocation}/{DockLocation?.LeftRight})");
        DockLocation = null;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var pt = e.GetCurrentPoint(this);
        Debug.WriteLine($"[SideBarButton] PointerPressed tag={DebugTag} pos={pt.Position:F1} right={pt.Properties.IsRightButtonPressed}");

        if (pt.Properties.IsRightButtonPressed)
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

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        Debug.WriteLine($"[SideBarButton] PointerReleased tag={DebugTag} pos={e.GetPosition(this):F1}");
        _canDrag = false;
        _pressedArgs = null;
    }

    protected override async void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (!_canDrag || _pressedArgs == null) return;

        var point = e.GetPosition(this);
        var threshold = Bounds.Height / 4;
        var dx = Math.Abs(point.X - _startPoint.X);
        var dy = Math.Abs(point.Y - _startPoint.Y);

        if (!(dx > threshold) && !(dy > threshold))
            return;

        Debug.WriteLine($"[SideBarButton] DragStart tag={DebugTag} startPt={_startPoint:F1} curPt={point:F1} dx={dx:F1} dy={dy:F1} threshold={threshold:F1} DockLocation={DockLocation?.ButtonLocation}/{DockLocation?.LeftRight}");

        _canDrag = false;
        var pressedArgs = _pressedArgs;
        _pressedArgs = null;

        var sideBar = this.FindAncestorOfType<SideBar>();
        Debug.WriteLine($"[SideBarButton] DragStart — source SideBar={sideBar?.Location.ToString() ?? "null"}");
        sideBar?.SetGridHitTestVisible(false);

        IsVisible = false;
        if (Parent is ContentPresenter cp)
        {
            cp.IsVisible = false;
            Debug.WriteLine($"[SideBarButton] DragStart — hiding button and ContentPresenter parent");
        }

        var item = DataTransferItem.Create(DragFormat, "drag");
        var data = new DataTransfer();
        data.Add(item);

        CurrentDragButton = this;
        Debug.WriteLine($"[SideBarButton] DoDragDropAsync starting...");

        // Snapshot the source sidebar BEFORE the drop — after DoDragDropAsync returns
        // the button may have been moved to a different sidebar's ItemsSource, so
        // FindAncestorOfType<SideBar>() would return the new sidebar (or null).
        var sourceSideBar = this.FindAncestorOfType<SideBar>();

        await DragDrop.DoDragDropAsync(pressedArgs, data, DragDropEffects.Move);
        Debug.WriteLine($"[SideBarButton] DoDragDropAsync completed");
        CurrentDragButton = null;

        sideBar?.SetGridHitTestVisible(true);

        var currentSideBar = this.FindAncestorOfType<SideBar>();
        bool wasDetached = currentSideBar == null;
        bool movedToOtherSideBar = !wasDetached && !ReferenceEquals(currentSideBar, sourceSideBar);
        
        // Only restore visibility if the button is still attached to the visual tree.
        // For cross-sidebar moves, the old button gets detached when the data context is
        // removed from the source collection, and a new button is created in the destination
        // sidebar via ItemsControl container generation. The new button is already visible.
        // Restoring visibility on a detached button is unnecessary and can cause issues.
        if (!wasDetached)
        {
            IsVisible = true;
            if (Parent is ContentPresenter cp2)
                cp2.IsVisible = true;
        }

        Debug.WriteLine($"[SideBarButton] DragEnd — visibility restored. detached={wasDetached} crossSidebar={movedToOtherSideBar} DockLocation={DockLocation?.ButtonLocation}/{DockLocation?.LeftRight}");

        // Execute pending move after drag operation completes
        if (PendingMove != null)
        {
            ExecutePendingMove();
        }
    }

    private void ExecutePendingMove()
    {
        if (PendingMove == null) return;

        Debug.WriteLine($"[SideBarButton] Executing pending move from {PendingMove.SourceSideBar?.Location}/{PendingMove.SourceLocation?.ButtonLocation} to {PendingMove.DestinationSideBar?.Location}/{PendingMove.DestinationLocation?.ButtonLocation}[{PendingMove.DestinationIndex}]");

        var oldItemsSource = PendingMove.SourceLocation?.ButtonLocation switch
        {
            SideBarButtonLocation.UpperTop    => PendingMove.SourceSideBar?.UpperTopToolsSource,
            SideBarButtonLocation.UpperBottom => PendingMove.SourceSideBar?.UpperBottomToolsSource,
            SideBarButtonLocation.LowerTop    => PendingMove.SourceSideBar?.LowerTopToolsSource,
            SideBarButtonLocation.LowerBottom => PendingMove.SourceSideBar?.LowerBottomToolsSource,
            _                                 => null
        };

        var newItemsSource = PendingMove.DestinationLocation?.ButtonLocation switch
        {
            SideBarButtonLocation.UpperTop    => PendingMove.DestinationSideBar?.UpperTopToolsSource,
            SideBarButtonLocation.UpperBottom => PendingMove.DestinationSideBar?.UpperBottomToolsSource,
            SideBarButtonLocation.LowerTop    => PendingMove.DestinationSideBar?.LowerTopToolsSource,
            SideBarButtonLocation.LowerBottom => PendingMove.DestinationSideBar?.LowerBottomToolsSource,
            _                                 => null
        };

        if (oldItemsSource is IList oldSource && newItemsSource is IList newSource)
        {
            if (oldSource.Contains(PendingMove.DataContext))
            {
                oldSource.Remove(PendingMove.DataContext);
                newSource.Insert(PendingMove.DestinationIndex, PendingMove.DataContext);
                Debug.WriteLine($"[SideBarButton] Successfully moved DataContext in ExecutePendingMove");
                
                // Update the button's DockLocation to reflect the new location
                // This is important for subsequent drag operations
                var newSideBar = PendingMove.DestinationSideBar;
                var newLocation = newSideBar?.FindAncestorOfType<ItemsControl>()?.Name switch
                {
                    "PART_UpperTopTools" => SideBarButtonLocation.UpperTop,
                    "PART_UpperBottomTools" => SideBarButtonLocation.UpperBottom,
                    "PART_LowerTopTools" => SideBarButtonLocation.LowerTop,
                    "PART_LowerBottomTools" => SideBarButtonLocation.LowerBottom,
                    _ => default
                };
                
                if (newSideBar != null && newLocation != default)
                {
                    DockLocation = new DockAreaLocation(newLocation, newSideBar.Location);
                    Debug.WriteLine($"[SideBarButton] Updated DockLocation to {DockLocation.ButtonLocation}/{DockLocation.LeftRight}");
                }
            }
            else
            {
                Debug.WriteLine($"[SideBarButton] DataContext not found in oldSource during ExecutePendingMove");
            }
        }
        else
        {
            Debug.WriteLine($"[SideBarButton] oldSource or newSource is not IList during ExecutePendingMove");
        }

        PendingMove = null;
    }
}

/// <summary>
/// Stores information about a pending move operation to be executed after drag completes.
/// </summary>
internal class PendingMoveInfo
{
    public object DataContext { get; set; } = null!;
    public SideBar? SourceSideBar { get; set; }
    public DockAreaLocation? SourceLocation { get; set; }
    public SideBar? DestinationSideBar { get; set; }
    public DockAreaLocation? DestinationLocation { get; set; }
    public int DestinationIndex { get; set; }
}
