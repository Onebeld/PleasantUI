using CommunityToolkit.Mvvm.ComponentModel;

namespace PleasantUI.Example.ViewModels.Pages.ControlPages;

public class ProgressRingViewModel : ObservableObject
{
	private double _value = 25;
	private bool _isIndeterminate = false;

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