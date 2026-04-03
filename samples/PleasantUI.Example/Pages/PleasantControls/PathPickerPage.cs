using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.PleasantControlPages;

namespace PleasantUI.Example.Pages.PleasantControls;

public class PathPickerPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/PathPicker";
    public override bool ShowTitle { get; } = true;
    protected override Control CreateContent() => new PathPickerPageView();
}
