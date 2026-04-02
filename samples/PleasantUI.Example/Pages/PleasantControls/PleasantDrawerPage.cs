using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.PleasantControlPages;

namespace PleasantUI.Example.Pages.PleasantControls;

public class PleasantDrawerPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/PleasantDrawer";
    public override bool ShowTitle { get; } = true;
    protected override Control CreateContent() => new PleasantDrawerPageView();
}
