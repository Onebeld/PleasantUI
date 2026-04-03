using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.ControlPages;

namespace PleasantUI.Example.Pages.BasicControls;

public class SelectionListPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/SelectionList";
    public override bool ShowTitle { get; } = true;
    protected override Control CreateContent() => new SelectionListPageView();
}
