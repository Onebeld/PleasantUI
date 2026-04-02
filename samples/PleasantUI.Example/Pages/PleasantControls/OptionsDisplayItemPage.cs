using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.PleasantControlPages;

namespace PleasantUI.Example.Pages.PleasantControls;

public class OptionsDisplayItemPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/OptionsDisplayItem";
    public override bool ShowTitle { get; } = true;
    public override Control Content { get; } = new OptionsDisplayItemPageView();
}
