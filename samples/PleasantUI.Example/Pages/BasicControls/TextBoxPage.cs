using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.ControlPages;

namespace PleasantUI.Example.Pages.BasicControls;

public class TextBoxPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/TextBox";
    public override bool ShowTitle { get; } = true;
    protected override Control CreateContent() => new TextBoxPageView();
}
