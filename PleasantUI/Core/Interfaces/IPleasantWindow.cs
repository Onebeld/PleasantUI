using Avalonia.Animation;
using PleasantUI.Controls;

namespace PleasantUI.Core.Interfaces;

public interface IPleasantWindow
{
    internal void AddModalWindow(PleasantModalWindow modalWindow, Animation animation);

    internal void RemoveModalWindow(PleasantModalWindow modalWindow);
}