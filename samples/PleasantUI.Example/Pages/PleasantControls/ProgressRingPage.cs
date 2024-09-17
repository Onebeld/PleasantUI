using Avalonia.Controls;
using PleasantUI.Example.Interfaces;
using PleasantUI.Example.Views.Pages.ControlPages;
using PleasantUI.Example.Views.Pages.PleasantControlPages;

namespace PleasantUI.Example.Pages.PleasantControls;

public class ProgressRingPage : IPage
{
	public string Title { get; } = "ProgressRing";
	public bool ShowTitle { get; } = true;
	public Control Content
	{
		get => new ProgressRingPageView();
	}
}