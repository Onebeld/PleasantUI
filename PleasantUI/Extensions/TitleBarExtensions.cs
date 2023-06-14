using Avalonia.Controls;
using PleasantUI.Controls;

namespace PleasantUI.Extensions;

public static class TitleBarExtensions
{
    public static void AttachTitleBar(this Control control, PleasantWindow host)
    {
        if (host.TitleBar is null) return;

        control.ContextFlyout = host.TitleBar.GetContextFlyout();

        control.DoubleTapped += host.TitleBar.OnDragWindowBorderOnDoubleTapped;
        control.PointerPressed += host.TitleBar.OnDragWindowBorderOnPointerPressed;
    }

    public static void DetachTitleBar(this Control control, PleasantWindow host)
    {
        if (host.TitleBar is null) return;

        control.ContextFlyout = null;

        control.DoubleTapped -= host.TitleBar.OnDragWindowBorderOnDoubleTapped;
        control.PointerPressed -= host.TitleBar.OnDragWindowBorderOnPointerPressed;
    }
}