using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.PleasantControlPages;

namespace PleasantUI.Example.Pages.PleasantControls;

public class InformationBlockPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/InformationBlock";
    public override bool ShowTitle { get; } = true;
    protected override Control CreateContent() => new InformationBlockPageView();
}
