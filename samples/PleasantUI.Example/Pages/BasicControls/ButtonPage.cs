using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.ControlPages;

namespace PleasantUI.Example.Pages.BasicControls;

public class ButtonPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/Button";
    public override bool ShowTitle { get; } = true;
    protected override Control CreateContent() => new ButtonPageView();
}
