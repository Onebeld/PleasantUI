using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.ControlPages;

namespace PleasantUI.Example.Pages.BasicControls;

public class PinCodePage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/PinCode";
    public override bool ShowTitle { get; } = true;
    public override Control Content => new PinCodePageView();
}
