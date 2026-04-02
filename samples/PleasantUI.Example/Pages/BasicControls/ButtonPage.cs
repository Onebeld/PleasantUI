using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.ControlPages;

namespace PleasantUI.Example.Pages.BasicControls;

public class ButtonPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/Button";
    public override bool ShowTitle { get; } = true;
    public override Control Content => new ButtonPageView();
}
