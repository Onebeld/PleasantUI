using Avalonia.Controls;
using PleasantUI.Controls;

namespace PleasantUI.Extensions;

public static class TitleBarExtensions
{
    /// <summary>
    /// Attaches a title bar to the control.
    /// </summary>
    /// <param name="control">The control to attach the title bar to.</param>
    /// <param name="host">The <see cref="PleasantWindow"/> host which contains the title bar.</param>
    public static void AttachTitleBar(this Control control, PleasantWindow host)
    {
        if (host.TitleBar is null) return;

        control.ContextFlyout = host.TitleBar.GetContextFlyout();

        control.DoubleTapped += host.TitleBar.OnDragWindowBorderOnDoubleTapped;
        control.PointerPressed += host.TitleBar.OnDragWindowBorderOnPointerPressed;
    }

    /// <summary>
    /// Detaches the title bar event handlers and context flyout from the specified control.
    /// </summary>
    /// <param name="control">The control to detach the title bar event handlers and context flyout from.</param>
    /// <param name="host">The <see cref="PleasantWindow"/> host containing the title bar.</param>
    public static void DetachTitleBar(this Control control, PleasantWindow host)
    {
        if (host.TitleBar is null) return;

        control.ContextFlyout = null;

        control.DoubleTapped -= host.TitleBar.OnDragWindowBorderOnDoubleTapped;
        control.PointerPressed -= host.TitleBar.OnDragWindowBorderOnPointerPressed;
    }
}