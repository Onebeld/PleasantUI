using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.ControlPages;

namespace PleasantUI.Example.Pages.BasicControls;

public class CheckBoxPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/Checkbox";
    public override bool ShowTitle { get; } = true;
    public override Control Content => new CheckBoxPageView();
}
