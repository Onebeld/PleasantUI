using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.PleasantControlPages;

namespace PleasantUI.Example.Pages.PleasantControls;

public class PleasantSnackbarPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/PleasantSnackbar";
    public override bool ShowTitle { get; } = true;
    public override Control Content => new PleasantSnackbarPageView();
}
