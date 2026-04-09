using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.PleasantControlPages;

namespace PleasantUI.Example.Pages.PleasantControls;

public class CommandBarPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/CommandBar";
    public override bool ShowTitle { get; } = true;
    protected override Control CreateContent() => new CommandBarPageView();
}
