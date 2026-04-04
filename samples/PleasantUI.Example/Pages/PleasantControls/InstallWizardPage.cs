using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.PleasantControlPages;

namespace PleasantUI.Example.Pages.PleasantControls;

public class InstallWizardPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/InstallWizard";
    public override bool ShowTitle { get; } = true;
    protected override Control CreateContent() => new InstallWizardPageView();
}
