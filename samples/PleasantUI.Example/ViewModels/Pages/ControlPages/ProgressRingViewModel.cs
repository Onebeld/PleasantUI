using PleasantUI.Core;

namespace PleasantUI.Example.ViewModels.Pages.ControlPages;

public partial class ProgressRingViewModel : ViewModelBase
{
	public double Value
	{
		get;
		set => SetProperty(ref field, value);
	} = 25;

	public bool IsIndeterminate
	{
		get;
		set => SetProperty(ref field, value);
	}
}