using Avalonia.Media;
using PleasantUI.Example.Interfaces;
using PleasantUI.Example.Messages;
using PleasantUI.ToolKit.Services.Interfaces;

namespace PleasantUI.Example.Models;

public class ControlPageCard
{
	private readonly IEventAggregator _eventAggregator;
		
	public string Title { get; set; }

	public Geometry Icon { get; set; }

	public string Description { get; set; }
	
	public IPage Page { get; set; }

	public ControlPageCard(string title, Geometry icon, string description, IPage page, IEventAggregator eventAggregator)
	{
		_eventAggregator = eventAggregator;
			
		Title = title;
		Icon = icon;
		Description = description;
		Page = page;
	}

	public void OpenPage()
	{
		_eventAggregator.PublishAsync(new ChangePageMessage(Page));
	}
}