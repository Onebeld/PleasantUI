using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.ControlPages;

namespace PleasantUI.Example.Pages.BasicControls;

public class NavigationPageTitlePage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/NavigationPage";
    public override bool ShowTitle { get; } = true;
    protected override Control CreateContent() => new NavigationPageTitlePageView();
}