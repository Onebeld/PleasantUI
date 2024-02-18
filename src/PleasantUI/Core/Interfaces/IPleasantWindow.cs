using Avalonia.Collections;
using Avalonia.Controls.Primitives;
using PleasantUI.Controls;

namespace PleasantUI.Core.Interfaces;

public interface IPleasantWindow
{
    AvaloniaList<PleasantModalWindow> ModalWindows { get; }
    
    VisualLayerManager VisualLayerManager { get; }
    
    internal void AddModalWindow(PleasantModalWindow modalWindow);

    internal void RemoveModalWindow(PleasantModalWindow modalWindow);
}