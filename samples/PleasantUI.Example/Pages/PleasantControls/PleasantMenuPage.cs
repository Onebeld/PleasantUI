using Avalonia.Controls;
using PleasantUI.Example.Interfaces;
using PleasantUI.Example.Views.Pages.PleasantControlPages;

namespace PleasantUI.Example.Pages.PleasantControls;

public class PleasantMenuPage : IPage
{
    public string Title { get; } = "PleasantMenu";
    public bool ShowTitle { get; } = true;
    public Control Content => new PleasantMenuPageView();
}
