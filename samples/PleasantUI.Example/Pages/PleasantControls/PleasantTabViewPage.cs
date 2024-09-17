using Avalonia.Controls;
using PleasantUI.Example.Interfaces;

namespace PleasantUI.Example.Pages.PleasantControls;

public class PleasantTabViewPage : IPage
{
    public string Title { get; } = "PleasantTabView";
    public bool ShowTitle { get; } = true;
    public Control Content { get; }
}