using Avalonia.Controls;
using PleasantUI.Example.Interfaces;

namespace PleasantUI.Example.Pages.BasicControls;

public class TextBoxPage : IPage
{
    public string Title { get; } = "TextBox";
    public bool ShowTitle { get; } = true;
    public Control Content { get; } = null!;
}