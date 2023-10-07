using Avalonia.Collections;
using PleasantUI.Controls;

namespace PleasantUI.Core.Interfaces;

public interface IPleasantWindow
{
    AvaloniaList<PleasantModalWindow> OpenedModalWindows { get; }
    
    internal void AddModalWindow(PleasantModalWindow modalWindow);

    internal void RemoveModalWindow(PleasantModalWindow modalWindow);
}