using PleasantUI.Controls;

namespace PleasantUI.Core.Interfaces;

public interface IPleasantWindow
{
    internal void AddModalWindow(PleasantModalWindow modalWindow);

    internal void RemoveModalWindow(PleasantModalWindow modalWindow);
}