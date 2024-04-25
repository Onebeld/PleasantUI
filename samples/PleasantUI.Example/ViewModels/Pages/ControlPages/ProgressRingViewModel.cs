namespace PleasantUI.Example.ViewModels.Pages.ControlPages;

public class ProgressRingViewModel : ViewModelBase
{
	private double _value = 25;

	public double Value
	{
		get => _value;
		set => RaiseAndSet(ref _value, value);
	}
}