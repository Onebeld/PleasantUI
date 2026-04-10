using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.PleasantControlPages;

namespace PleasantUI.Example.Pages.PleasantControls;

public class ItemListPanelPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/ItemListPanel";
    public override bool ShowTitle { get; } = true;
    protected override Control CreateContent() => new ItemListPanelPageView();
}
