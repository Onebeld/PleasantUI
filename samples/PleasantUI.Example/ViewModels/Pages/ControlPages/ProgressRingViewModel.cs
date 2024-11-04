using CommunityToolkit.Mvvm.ComponentModel;

namespace PleasantUI.Example.ViewModels.Pages.ControlPages;

public partial class ProgressRingViewModel : ObservableObject
{
	[ObservableProperty]
	private double _value = 25;
	
	[ObservableProperty]
	private bool _isIndeterminate;
}