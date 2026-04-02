using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.ControlPages;

namespace PleasantUI.Example.Pages.PleasantControls;

public class ProgressPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/Progress";
    public override bool ShowTitle { get; } = true;
    public override Control Content { get; } = new ProgressBarPageView();
}
