using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using PleasantUI.Example.Interfaces;

namespace PleasantUI.Example.Models;

public partial class ControlPageCard
{
	public string Title { get; set; }

	public Geometry Icon { get; set; }

	public string Description { get; set; }
	
	public IPage Page { get; set; }

	public ControlPageCard(string title, Geometry icon, string description, IPage page)
	{
		Title = title;
		Icon = icon;
		Description = description;
		Page = page;
	}

	[RelayCommand]
	public void OpenPage()
	{
		WeakReferenceMessenger.Default.Send(new ValueChangedMessage<IPage>(Page));
	}
}