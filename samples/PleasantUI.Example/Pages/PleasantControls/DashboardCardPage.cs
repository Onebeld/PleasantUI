using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.PleasantControlPages;

namespace PleasantUI.Example.Pages.PleasantControls;

public class DashboardCardPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/DashboardCard";
    public override bool ShowTitle { get; } = true;
    protected override Control CreateContent() => new DashboardCardPageView();
}
