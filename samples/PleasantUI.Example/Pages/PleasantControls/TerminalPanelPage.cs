using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.PleasantControlPages;

namespace PleasantUI.Example.Pages.PleasantControls;

public class TerminalPanelPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/TerminalPanel";
    public override bool ShowTitle { get; } = true;
    protected override Control CreateContent() => new TerminalPanelPageView();
}
