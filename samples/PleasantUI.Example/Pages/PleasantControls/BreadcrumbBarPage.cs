using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.PleasantControlPages;

namespace PleasantUI.Example.Pages.PleasantControls;

public class BreadcrumbBarPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/BreadcrumbBar";
    public override bool ShowTitle { get; } = true;
    protected override Control CreateContent() => new BreadcrumbBarPageView();
}
