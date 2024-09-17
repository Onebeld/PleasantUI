using Avalonia.Controls;
using PleasantUI.Example.Interfaces;
using PleasantUI.Example.Views.Pages.ControlPages;

namespace PleasantUI.Example.Pages.BasicControls;

public class CheckBoxPage : IPage
{
    public string Title { get; } = "CheckBox";

    public bool ShowTitle { get; } = true;

    public Control Content
    {
        get { return new CheckBoxPageView(); }
    }
}