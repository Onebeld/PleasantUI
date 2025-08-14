using PleasantUI.Core;

namespace PleasantUI.Example.ViewModels.Pages.ControlPages;

public partial class ProgressRingViewModel : ViewModelBase
{
	private double _value = 25;
	
	private bool _isIndeterminate;

	public double Value
	{
		get => _value;
		set => SetProperty(ref _value, value);
	}

	public bool IsIndeterminate
	{
		get => _isIndeterminate;
		set => SetProperty(ref _isIndeterminate, value);
	}
}