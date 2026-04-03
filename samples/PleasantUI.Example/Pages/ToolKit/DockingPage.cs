using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.ToolkitPages;

namespace PleasantUI.Example.Pages.Toolkit;

public class DockingPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/Docking";
    public override bool ShowTitle { get; } = true;
    protected override Control CreateContent() => new DockingPageView();
}
