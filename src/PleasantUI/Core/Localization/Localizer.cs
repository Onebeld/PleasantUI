using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace PleasantUI.Core.Localization;

/// <summary>
/// Provides localization functionality for the application.
/// </summary>
public class Localizer : ILocalizer, INotifyPropertyChanged
{
	private const string DefaultLanguage = "en";

	private const string IndexerName = "Item";
	private const string IndexerArrayName = "Item[]";

	private static readonly List<ResourceManager>? ResourceManagers = new();
	
	private List<ResourceManager>? _resources;
	
	/// <summary>
	/// Gets the singleton instance of the <see cref="Localizer"/> class.
	/// </summary>
	public static Localizer Instance { get; } = new();
	
	/// <summary>
	/// Occurs when a property value changes.
	/// </summary>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>
	/// Initializes a new instance of the <see cref="Localizer"/> class.
	/// </summary>
	public Localizer()
	{
		LoadLanguage();
	}

	/// <summary>
	/// Loads the current language resources.
	/// </summary>
	public void LoadLanguage()
	{
		if (ResourceManagers != null)
			_resources = new List<ResourceManager>(ResourceManagers);
		
		InvalidateEvents();
	}

	/// <inheritdoc/>
	public void EditLanguage(string language)
	{
		Instance.ChangeLanguage(language);
	}

	/// <inheritdoc/>
	public void AddResourceManager(ResourceManager resourceManager)
	{
		ResourceManagers?.Add(resourceManager);
	}

	/// <inheritdoc/>
	public void ChangeLanguage(string language)
	{
		if (string.IsNullOrEmpty(language))
			language = DefaultLanguage;

		CultureInfo.CurrentUICulture = new CultureInfo(language);
		Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentUICulture;
		LoadLanguage();
	}

	/// <summary>
	/// Gets the localized string for the specified key.
	/// </summary>
	/// <param name="key">The key to look up.</param>
	/// <returns>The localized string, or an error message if the key is not found or the resources are empty.</returns>
	public string this[string key]
	{
		get
		{
			if (_resources == null || !_resources.Any())
				return "<ERROR! LANGUAGE Resources is empty>";

			string? row = GetExpression(key);

			if (string.IsNullOrEmpty(row))
				return $"<ERROR! Not found key \"{key}\">";

			string? ret = row?.Replace(@"\\n", "\n");
			
			if (string.IsNullOrEmpty(ret))
				ret = $"Localize:{key}";

			return ret;
		}
	}

	/// <inheritdoc/>
	public string? GetExpression(string key)
	{
		if (_resources == null) return string.Empty;
		foreach (ResourceManager? resource in _resources)
		{
			string? row = resource?.GetString(key);
			
			if (!string.IsNullOrEmpty(row))
				return row;
		}

		return string.Empty;
	}

	private void InvalidateEvents()
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
	}
}