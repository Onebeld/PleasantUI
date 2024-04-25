using Avalonia.Controls;
using PleasantUI.Example.ViewModels.Pages.ControlPages;

namespace PleasantUI.Example.Views.Pages.ControlPages;

public partial class ProgressRing : UserControl
{
	public ProgressRing()
	{
		InitializeComponent();
		DataContext = new ProgressRingViewModel();
	}
}