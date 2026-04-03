using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.PleasantControlPages;

namespace PleasantUI.Example.Pages.PleasantControls;

public class PopConfirmPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/PopConfirm";
    public override bool ShowTitle { get; } = true;
    protected override Control CreateContent() => new PopConfirmPageView();
}
