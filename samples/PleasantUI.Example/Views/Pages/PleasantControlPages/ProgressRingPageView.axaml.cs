using Avalonia.Controls;
using PleasantUI.Example.ViewModels.Pages.ControlPages;

namespace PleasantUI.Example.Views.Pages.PleasantControlPages;

public partial class ProgressRingPageView : UserControl
{
	public ProgressRingPageView()
	{
		InitializeComponent();
		DataContext = new ProgressRingViewModel();
	}
}