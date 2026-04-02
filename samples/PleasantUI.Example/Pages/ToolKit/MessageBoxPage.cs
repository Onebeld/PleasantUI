using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.ToolkitPages;

namespace PleasantUI.Example.Pages.Toolkit;

public class MessageBoxPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/MessageBox";
    public override bool ShowTitle { get; } = true;
    public override Control Content => new MessageBoxPageView();
}
