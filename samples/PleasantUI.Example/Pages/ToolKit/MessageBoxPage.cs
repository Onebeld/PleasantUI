using Avalonia.Controls;
using PleasantUI.Example.Interfaces;
using PleasantUI.Example.Views.Pages.ToolkitPages;

namespace PleasantUI.Example.Pages.Toolkit;

public class MessageBoxPage : IPage
{
    public string Title { get; } = "MessageBox";
    public bool ShowTitle { get; } = true;

    public Control Content => new MessageBoxPageView();
}