using Avalonia.Media;
using Avalonia.Styling;

namespace PleasantUI.Core.Models;

/// <summary>
/// Represents a custom theme with customizable colors.
/// </summary>
public class CustomTheme : ViewModelBase
{
	private ThemeVariant _themeVariant;
	private string _name;
	
	/// <summary>
	/// Gets or sets the unique identifier for the theme.
	/// </summary>
	public Guid Id { get; set; }
	
	/// <summary>
	/// Gets the theme variant associated with the theme.
	/// </summary>
	public ThemeVariant ThemeVariant => _themeVariant;
	
	/// <summary>
	/// Gets or sets the dictionary of colors used in the theme.
	/// </summary>
	public Dictionary<string, Color> Colors { get; set; }
	
	/// <summary>
	/// Gets or sets the name of the theme.
	/// </summary>
	public string Name
	{
		get => _name;
		set
		{
			RaiseAndSet(ref _name, value);

			_themeVariant = new ThemeVariant(NameId, ThemeVariant.Light);
			
			RaisePropertyChanged(nameof(ThemeVariant));
		}
	}
	
	/// <summary>
	/// Gets the combined name and ID used for identification.
	/// </summary>
	public string NameId => Id + Name;

	/// <summary>
	/// Initializes a new instance of the <see cref="CustomTheme"/> class.
	/// </summary>
	/// <param name="id">The unique identifier for the theme.</param>
	/// <param name="name">The name of the theme.</param>
	/// <param name="colors">The dictionary of colors used in the theme.</param>
	public CustomTheme(Guid? id, string name, Dictionary<string, Color> colors)
	{
		Colors = colors;
		Id = id ?? Guid.NewGuid();
		Name = name;
	}
}