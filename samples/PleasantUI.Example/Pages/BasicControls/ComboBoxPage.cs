using Avalonia.Controls;
using PleasantUI.Example.Interfaces;
using PleasantUI.Example.Views.Pages.ControlPages;

namespace PleasantUI.Example.Pages.BasicControls;

public class ComboBoxPage : IPage
{
    public string Title { get; } = "ComboBox";
    public bool ShowTitle { get; } = true;
    public Control Content { get; } = new ComboBoxPageView();
}