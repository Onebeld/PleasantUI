using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.PleasantControlPages;

namespace PleasantUI.Example.Pages.Toolkit;

public class StepDialogPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/StepDialog";
    public override bool ShowTitle { get; } = true;
    protected override Control CreateContent() => new StepDialogPageView();
}
