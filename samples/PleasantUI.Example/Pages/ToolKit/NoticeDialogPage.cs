using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.ToolkitPages;

namespace PleasantUI.Example.Pages.Toolkit;

public class NoticeDialogPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/NoticeDialog";
    public override bool ShowTitle { get; } = true;
    protected override Control CreateContent() => new NoticeDialogPageView();
}
