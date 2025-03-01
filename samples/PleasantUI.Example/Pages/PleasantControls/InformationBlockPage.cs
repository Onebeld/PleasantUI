using Avalonia.Controls;
using PleasantUI.Example.Interfaces;

namespace PleasantUI.Example.Pages.PleasantControls;

public class InformationBlockPage : IPage
{
    public string Title { get; } = "InformationBlock";
    public bool ShowTitle { get; } = true;
    public Control Content { get; } = null!;
}