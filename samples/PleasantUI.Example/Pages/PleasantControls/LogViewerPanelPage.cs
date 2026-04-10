using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.PleasantControlPages;

namespace PleasantUI.Example.Pages.PleasantControls;

public class LogViewerPanelPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/LogViewerPanel";
    public override bool ShowTitle { get; } = true;
    protected override Control CreateContent() => new LogViewerPanelPageView();
}
