using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using PleasantUI.Reactive;

namespace PleasantUI.Controls;

/// <summary>
/// Hosts a modal window as an overlay within the visual tree.
/// </summary>
public class ModalWindowHost : ContentControl
{
    private IDisposable? _rootBoundsWatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModalWindowHost"/> class.
    /// </summary>
    public ModalWindowHost()
    {
        Background = null;
    }

    /// <summary>
    /// Gets the key to use for styling this control.
    /// </summary>
    protected override Type StyleKeyOverride => typeof(OverlayPopupHost);

    /// <summary>
    /// Called when the control is attached to the visual tree.
    /// Sets up a watcher to track changes in the bounds of the root control.
    /// </summary>
    /// <param name="e">The event data containing information about the visual tree attachment.</param>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (e.Root is Control root)
            _rootBoundsWatcher = root.GetObservable(BoundsProperty).Subscribe(_ => OnRootBoundsChanged());
    }

    /// <summary>
    /// Called when the control is detached from the visual tree.
    /// Disposes of the bounds watcher.
    /// </summary>
    /// <param name="e">The event data containing information about the visual tree detachment.</param>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _rootBoundsWatcher?.Dispose();
        _rootBoundsWatcher = null;
    }

    /// <summary>
    /// Measures the control and returns the size required.
    /// </summary>
    /// <param name="availableSize">The available size that the control can give to child elements.</param>
    /// <returns>The desired size of the control.</returns>
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

    /// <summary>
    /// Handles the pointer entered event and marks it as handled.
    /// </summary>
    /// <param name="e">The pointer event data.</param>
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        e.Handled = true;
    }

    /// <summary>
    /// Handles the pointer exited event and marks it as handled.
    /// </summary>
    /// <param name="e">The pointer event data.</param>
    protected override void OnPointerExited(PointerEventArgs e)
    {
        e.Handled = true;
    }

    /// <summary>
    /// Handles the pointer pressed event and marks it as handled.
    /// </summary>
    /// <param name="e">The pointer event data.</param>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        e.Handled = true;
    }

    /// <summary>
    /// Handles the pointer released event and marks it as handled.
    /// </summary>
    /// <param name="e">The pointer event data.</param>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        e.Handled = true;
    }

    /// <summary>
    /// Handles the pointer capture lost event and marks it as handled.
    /// </summary>
    /// <param name="e">The pointer capture lost event data.</param>
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        e.Handled = true;
    }

    /// <summary>
    /// Handles the pointer moved event and marks it as handled.
    /// </summary>
    /// <param name="e">The pointer event data.</param>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        e.Handled = true;
    }

    /// <summary>
    /// Handles the pointer wheel changed event and marks it as handled.
    /// </summary>
    /// <param name="e">The pointer wheel event data.</param>
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        e.Handled = true;
    }

    private void OnRootBoundsChanged()
    {
        InvalidateMeasure();
    }
}