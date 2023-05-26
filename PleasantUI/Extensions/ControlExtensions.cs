using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace PleasantUI.Extensions;

public static class ControlExtensions
{
    public static bool PointerEffectivelyOver(this Control control, PointerEventArgs e)
        => new Rect(control.Bounds.Size).Contains(e.GetPosition(control));
}