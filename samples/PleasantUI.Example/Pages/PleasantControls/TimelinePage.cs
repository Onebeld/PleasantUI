using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.PleasantControlPages;

namespace PleasantUI.Example.Pages.PleasantControls;
public class TimelinePage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/Timeline";
    public override bool ShowTitle { get; } = true;
    protected override Control CreateContent() => new TimelinePageView();
}
