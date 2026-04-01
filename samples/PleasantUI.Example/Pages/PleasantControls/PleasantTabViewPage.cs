using Avalonia.Controls;
using PleasantUI.Example.Interfaces;
using PleasantUI.Example.Views.Pages.PleasantControlPages;

namespace PleasantUI.Example.Pages.PleasantControls;

public class PleasantTabViewPage : IPage
{
    public string Title { get; } = "PleasantTabView";
    public bool ShowTitle { get; } = true;
    public Control Content { get; } = new PleasantTabViewPageView();
}