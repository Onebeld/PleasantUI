using System.Windows.Input;
using Avalonia.Media;
using PleasantUI.Example.Interfaces;

namespace PleasantUI.Example.Models;

public class ControlPageCard
{
	public string Title { get; set; }

	public Geometry Icon { get; set; }

	public string Description { get; set; }

	public ICommand Command { get; set; }
	
	public IPage Page { get; set; }

	public ControlPageCard(string title, Geometry icon, string description, ICommand command, IPage page)
	{
		Title = title;
		Icon = icon;
		Description = description;
		Command = command;
		Page = page;
	}
}