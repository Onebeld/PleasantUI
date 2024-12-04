using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using PleasantUI.Reactive;

namespace PleasantUI.Controls;

public class ModalWindowHost : ContentControl
{
    private IDisposable? _rootBoundsWatcher;

    public ModalWindowHost()
    {
        Background = null;
    }

    protected override Type StyleKeyOverride => typeof(OverlayPopupHost);

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (e.Root is Control root)
            _rootBoundsWatcher = root.GetObservable(BoundsProperty).Subscribe(_ => OnRootBoundsChanged());
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        
        _rootBoundsWatcher?.Dispose();
        _rootBoundsWatcher = null;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        _ = base.MeasureOverride(availableSize);

        return VisualRoot switch
        {
            TopLevel topLevel => topLevel.ClientSize,
            Control control => control.Bounds.Size,
            
            _ => default
        };
    }
    
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        e.Handled = true;
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        e.Handled = true;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        e.Handled = true;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        e.Handled = true;
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        e.Handled = true;
    }

    private void OnRootBoundsChanged()
    {
        InvalidateMeasure();
    }
}