using Avalonia.Controls;
using PleasantUI.Example.Interfaces;

namespace PleasantUI.Example.Pages.BasicControls;

public class SliderPage : IPage
{
    public string Title { get; } = "Slider";
    public bool ShowTitle { get; } = true;
    public Control Content { get; } = null!;
}