using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.ControlPages;

namespace PleasantUI.Example.Pages.BasicControls;

public class ProgressBarPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/Progress";
    public override bool ShowTitle { get; } = true;
    protected override Control CreateContent() => new ProgressBarPageView();
}
