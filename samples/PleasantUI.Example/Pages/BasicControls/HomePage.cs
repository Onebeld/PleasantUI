using Avalonia.Controls;
using PleasantUI.Example.Views.Pages;

namespace PleasantUI.Example.Pages.BasicControls;

public class HomePage : LocalizedPage
{
    public override string TitleKey { get; } = "Home";
    public override bool ShowTitle { get; } = false;
    public override Control Content => new HomePageView();
}
