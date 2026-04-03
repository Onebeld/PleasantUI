using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.ControlPages;

namespace PleasantUI.Example.Pages.BasicControls;

public class CarouselPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/Carousel";
    public override bool ShowTitle { get; } = true;
    protected override Control CreateContent() => new CarouselPageView();
}
