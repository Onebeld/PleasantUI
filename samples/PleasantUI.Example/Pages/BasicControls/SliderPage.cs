using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.ControlPages;

namespace PleasantUI.Example.Pages.BasicControls;

public class SliderPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/Slider";
    public override bool ShowTitle { get; } = true;
    public override Control Content { get; } = new SliderPageView();
}
