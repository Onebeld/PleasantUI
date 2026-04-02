using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.ControlPages;

namespace PleasantUI.Example.Pages.BasicControls;

public class CheckBoxPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/Checkbox";
    public override bool ShowTitle { get; } = true;
    protected override Control CreateContent() => new CheckBoxPageView();
}
