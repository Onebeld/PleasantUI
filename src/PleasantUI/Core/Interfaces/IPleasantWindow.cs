using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PleasantUI.Controls;

namespace PleasantUI.Core.Interfaces;

public interface IPleasantWindow
{
    AvaloniaList<PleasantModalWindow> ModalWindows { get; }
    
    AvaloniaList<Control> Controls { get; }
    
    VisualLayerManager VisualLayerManager { get; }
    
    internal void AddModalWindow(PleasantModalWindow modalWindow);

    internal void RemoveModalWindow(PleasantModalWindow modalWindow);
    
    internal void AddControl(Control control);
    
    internal void RemoveControl(Control control);
}