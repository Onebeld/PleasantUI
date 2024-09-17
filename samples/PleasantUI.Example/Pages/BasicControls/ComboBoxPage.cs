using Avalonia.Controls;
using PleasantUI.Example.Interfaces;

namespace PleasantUI.Example.Pages.BasicControls;

public class ComboBoxPage : IPage
{
    public string Title { get; } = "ComboBox";
    public bool ShowTitle { get; } = true;
    public Control Content { get; }
}