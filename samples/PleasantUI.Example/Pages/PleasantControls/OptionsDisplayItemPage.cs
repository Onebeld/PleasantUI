using Avalonia.Controls;
using PleasantUI.Example.Interfaces;

namespace PleasantUI.Example.Pages.PleasantControls;

public class OptionsDisplayItemPage : IPage
{
    public string Title { get; } = "OptionsDisplayItem";
    public bool ShowTitle { get; } = true;
    public Control Content { get; } = null!;
}