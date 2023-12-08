using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace PleasantUI.Extensions;

public static class ControlExtensions
{
    /// <summary>
    /// Determines if the pointer is effectively over the specified control.
    /// </summary>
    /// <param name="control">The control to check.</param>
    /// <param name="e">The pointer event arguments.</param>
    /// <returns>
    /// True if the pointer is effectively over the control; otherwise, false.
    /// </returns>
    public static bool PointerEffectivelyOver(this Control control, PointerEventArgs e)
        => new Rect(control.Bounds.Size).Contains(e.GetPosition(control));
}