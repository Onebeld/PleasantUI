using Avalonia.Controls;
using PleasantUI.Example.Interfaces;
using PleasantUI.Example.Views.Pages.ControlPages;

namespace PleasantUI.Example.Pages.BasicControls;

public class PinCodePage : IPage
{
    public string Title { get; } = "PinCode";
    public bool ShowTitle { get; } = true;
    public Control Content => new PinCodePageView();
}
