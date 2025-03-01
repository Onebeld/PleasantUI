using Avalonia.Controls;
using PleasantUI.Example.Interfaces;

namespace PleasantUI.Example.Pages.BasicControls;

public class ProgressBarPage : IPage
{
    public string Title { get; } = "ProgressBar";
    public bool ShowTitle { get; } = true;
    public Control Content { get; } = null!;
}