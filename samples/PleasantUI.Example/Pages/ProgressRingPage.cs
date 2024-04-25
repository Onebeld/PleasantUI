using Avalonia.Controls;
using PleasantUI.Example.Interfaces;
using PleasantUI.Example.Views.Pages.ControlPages;

namespace PleasantUI.Example.Pages;

public class ProgressRingPage : IPage
{
	public string Title { get; } = "ProgressRing";
	public bool ShowTitle { get; } = true;
	public Control Content
	{
		get => new ProgressRing();
	}
}