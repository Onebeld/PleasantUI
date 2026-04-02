using PleasantUI.Example.ViewModels.Pages.ControlPages;

namespace PleasantUI.Example.Views.Pages.PleasantControlPages;

public partial class ProgressRingPageView : LocalizedUserControl
{
    public ProgressRingPageView()
    {
        InitializeComponent();
        DataContext = new ProgressRingViewModel();
    }
    protected override void ReinitializeComponent() => InitializeComponent();
}
