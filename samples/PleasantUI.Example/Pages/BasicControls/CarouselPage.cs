using Avalonia.Controls;
using PleasantUI.Example.Interfaces;

namespace PleasantUI.Example.Pages.BasicControls;

public class CarouselPage : IPage
{
    public string Title { get; } = "Carousel";
    public bool ShowTitle { get; } = true;
    public Control Content { get; }
}