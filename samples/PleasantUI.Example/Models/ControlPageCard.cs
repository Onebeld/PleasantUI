using System.Windows.Input;
using Avalonia.Media;

namespace PleasantUI.Example.Models;

public class ControlPageCard
{
	private string _title;
	private Geometry _icon;
	private string _description;
	private ICommand _action;
	
	public string Title
	{
		get => _title;
		set => _title = value;
	}
	
	public Geometry Icon
	{
		get => _icon;
		set => _icon = value;
	}
	
	public string Description
	{
		get => _description;
		set => _description = value;
	}
	
	public ICommand Action
	{
		get => _action;
		set => _action = value;
	}
	
	public ControlPageCard(string title, Geometry icon, string description, ICommand action)
	{
		_title = title;
		_icon = icon;
		_description = description;
		_action = action;
	}
}