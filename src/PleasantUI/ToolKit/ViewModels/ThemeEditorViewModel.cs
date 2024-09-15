using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using PleasantUI.Controls;
using PleasantUI.Core.Interfaces;
using PleasantUI.Core.Localization;
using PleasantUI.Core.Models;
using PleasantUI.Extensions;
using PleasantUI.ToolKit.Commands;
using PleasantUI.ToolKit.Commands.ThemeEditor;
using PleasantUI.Windows;
using Serilog;

namespace PleasantUI.ToolKit.ViewModels;

/// <summary>
/// ViewModel for the <see cref="ThemeEditorWindow"/>.
/// </summary>
public class ThemeEditorViewModel : ViewModelBase
{
	private readonly Stack<IEditorCommand> _undoStack = new();
	private readonly Stack<IEditorCommand> _redoStack = new();
	
	private readonly ThemeEditorWindow _themeEditorWindow;
	
	private AvaloniaList<Theme> _themes = new();
	private CustomTheme _customTheme;
	private string _themeName;

	/// <summary>
	/// Gets or sets the custom theme being edited.
	/// </summary>
	public CustomTheme CustomTheme
	{
		get => _customTheme;
		set => RaiseAndSet(ref _customTheme, value);
	}

	/// <summary>
	/// Gets or sets the name of the theme.
	/// </summary>
	public string ThemeName
	{
		get => _themeName;
		set
		{
			if (string.IsNullOrWhiteSpace(value))
				return;
			
			IEditorCommand command = new ThemeNameChangeCommand(this, _themeName, value);
			ExecuteCommand(command);
		}
	}

	/// <summary>
	/// Gets or sets the list of available default themes.
	/// </summary>
	public AvaloniaList<Theme> Themes
	{
		get => _themes;
		set => RaiseAndSet(ref _themes, value);
	}
	
	/// <summary>
	/// Gets the resource dictionary containing the theme colors.
	/// </summary>
	public readonly ResourceDictionary ResourceDictionary;
	
	/// <summary>
	/// Gets the list of theme colors.
	/// </summary>
	public AvaloniaList<ThemeColor> ThemeColors { get; } = new();

	/// <summary>
	/// Gets the colors of the custom theme in JSON format.
	/// </summary>
	public string ColorsJson => GetJson();
	
	/// <summary>
	/// Gets a value indicating whether an undo operation is possible.
	/// </summary>
	public bool CanUndo => _undoStack.Count > 0;
	
	/// <summary>
	/// Gets a value indicating whether a redo operation is possible.
	/// </summary>
	public bool CanRedo => _redoStack.Count > 0;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="ThemeEditorViewModel"/> class.
	/// </summary>
	/// <param name="parent">The parent window.</param>
	/// <param name="themeEditorWindow">The theme editor window.</param>
	/// <param name="customTheme">The custom theme to edit (optional).</param>
	public ThemeEditorViewModel(IPleasantWindow parent, ThemeEditorWindow themeEditorWindow, CustomTheme? customTheme)
	{
		_customTheme = customTheme ?? new CustomTheme(null, "NewTheme", PleasantTheme.GetThemeTemplateDictionary());
		_themeName = _customTheme.Name;
		
		ResourceDictionary = new ResourceDictionary();
		
		ThemeColor.Parent = parent;

		foreach (KeyValuePair<string, Color> pair in _customTheme.Colors)
		{
			ResourceDictionary.Add(pair.Key, pair.Value);

			ThemeColor themeColor = CreateThemeColor(pair.Key, pair.Value);
			ThemeColors.Add(themeColor);
		}
		
		foreach (Theme theme in PleasantTheme.Themes)
		{
			if (theme.Name is "System" or "Custom")
				continue;
			
			Themes.Add(theme);
		}
		
		themeEditorWindow.ThemeVariantScope.Resources.ThemeDictionaries.Add(_customTheme.ThemeVariant, ResourceDictionary);
		_themeEditorWindow = themeEditorWindow;

		RaisePropertyChanged(nameof(ColorsJson));
	}

	/// <summary>
	/// Undoes the last change.
	/// </summary>
	public void Undo()
	{
		if (!CanUndo) return;

		IEditorCommand command = _undoStack.Pop();
		command.Undo();
		_redoStack.Push(command);
		
		UpdateProperties();
	}

	/// <summary>
	/// Redoes the last undone change.
	/// </summary>
	public void Redo()
	{
		if (!CanRedo) return;

		IEditorCommand command = _redoStack.Pop();
		command.Redo();
		_undoStack.Push(command);
		
		UpdateProperties();
	}

	/// <summary>
	/// Copies the theme colors in JSON format to the clipboard.
	/// </summary>
	public async void CopyTheme()
	{
		await TopLevel.GetTopLevel(ThemeColor.Parent as Visual)?.Clipboard?.SetTextAsync(ColorsJson)!;

		Geometry? icon = ResourceExtensions.GetResource<Geometry>("CopyRegular");
		string text = Localizer.Instance["ThemeCopiedToClipboard"];
		
		PleasantSnackbar.Show(ThemeColor.Parent, text, icon);
	}

	public async void PasteTheme()
	{
		string? data = await TopLevel.GetTopLevel(ThemeColor.Parent as Visual)?.Clipboard?.GetTextAsync()!;
		
		ParseColorsFromJson(data);
	}

	public async void ImportTheme()
	{
		TopLevel? topLevel = TopLevel.GetTopLevel(ThemeColor.Parent as Visual);
		
		if (topLevel is null)
			return;
		
		IReadOnlyList<IStorageFile> pickerFileTypes = await topLevel.StorageProvider.OpenFilePickerAsync(
			new FilePickerOpenOptions
			{
				FileTypeFilter = [new FilePickerFileType("JSON") { Patterns = ["*.json"] }],
				AllowMultiple = false
			})!;
		
		if (pickerFileTypes.Count == 0)
			return;

		string json = File.ReadAllText(pickerFileTypes[0].Path.LocalPath);
		
		ParseColorsFromJson(json);
	}

	public async void ExportTheme()
	{
		TopLevel? topLevel = TopLevel.GetTopLevel(ThemeColor.Parent as Visual);
		
		if (topLevel is null)
			return;

		IStorageFile? result = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
		{
			FileTypeChoices = [new FilePickerFileType("JSON") { Patterns = ["*.json"] }],
			DefaultExtension = "json",
			SuggestedFileName = ThemeName
		});
		
		if (result is null)
			return;
		
		File.WriteAllText(result.Path.LocalPath, ColorsJson);
		
		Geometry? fileExportIcon = ResourceExtensions.GetResource<Geometry>("FileExportRegular");
		string themeExportedText = Localizer.Instance["ThemeExported"];
		
		PleasantSnackbar.Show(ThemeColor.Parent, themeExportedText, fileExportIcon, NotificationType.Success);
	}

	public void ChangeThemeName(string newName)
	{
		_themeName = newName;
		RaisePropertyChanged(nameof(ThemeName));
	}
	
	/// <summary>
	/// Applies the colors from the specified default theme.
	/// </summary>
	/// <param name="theme">The default theme to apply.</param>
	public void GetColorsFromTheme(Theme theme)
	{
		_themeEditorWindow.ButtonThemesFlyout.Flyout?.Hide();
		
		Dictionary<string, Color> colors = PleasantTheme.GetColorsDictionary(theme);

		IEditorCommand command = new ThemeChangeCommand(this, GetDictionary(), colors, ThemeName, theme.Name);
		ExecuteCommand(command);
	}

	/// <summary>
	/// Applies the colors from the specified custom theme.
	/// </summary>
	/// <param name="customTheme">The custom theme to apply.</param>
	public void GetColorsFromCustomTheme(CustomTheme customTheme)
	{
		_themeEditorWindow.ButtonThemesFlyout.Flyout?.Hide();
		
		Dictionary<string, Color> colors = customTheme.Colors;
		
		IEditorCommand command = new ThemeChangeCommand(this, GetDictionary(), colors, ThemeName, customTheme.Name);
		ExecuteCommand(command);
	}

	public void DropFile(IStorageItem file)
	{
		string json = File.ReadAllText(file.Path.AbsolutePath);
		
		ParseColorsFromJson(json);
	}

	private void ParseColorsFromJson(string json)
	{
		JsonDocument jsonDocument;
		
		try
		{
			jsonDocument = JsonDocument.Parse(json);
		}
		catch (Exception)
		{
			Log.Error("Error parsing colors from json. An invalid JSON was received.");
			
			Geometry? closeCircleIcon = ResourceExtensions.GetResource<Geometry>("CloseCircleRegular");
			string themeImportErrorText = Localizer.Instance["ThemeImportError"];
		
			PleasantSnackbar.Show(ThemeColor.Parent, themeImportErrorText, closeCircleIcon, NotificationType.Error);
			
			return;
		}

		Dictionary<string, Color> colors = new();

		string? themeName = null;

		if (jsonDocument.RootElement.TryGetProperty("ThemeName", out JsonElement element))
			themeName = element.GetString();

		foreach (JsonProperty jsonProperty in jsonDocument.RootElement.EnumerateObject())
		{
			string name = jsonProperty.Name;
			string? hexColor = jsonProperty.Value.GetString();
			
			if (hexColor is null)
				continue;
			
			if (!Color.TryParse(hexColor, out Color color))
				continue;
			
			colors.Add(name, color);
		}
		
		IEditorCommand command = new ThemeChangeCommand(this, GetDictionary(), colors, ThemeName, themeName);
		ExecuteCommand(command);
		
		jsonDocument.Dispose();
		
		Geometry? fileImportIcon = ResourceExtensions.GetResource<Geometry>("FileImportRegular");
		string themeImportedText = Localizer.Instance["ThemeImported"];
		
		PleasantSnackbar.Show(ThemeColor.Parent, themeImportedText, fileImportIcon, NotificationType.Success);
	}
	
	private void ExecuteCommand(IEditorCommand command)
	{
		command.Redo();
		
		_undoStack.Push(command);
		_redoStack.Clear();
		
		UpdateProperties();
	}

	private void UpdateProperties()
	{
		RaisePropertyChanged(nameof(CanUndo));
		RaisePropertyChanged(nameof(CanRedo));
		
		RaisePropertyChanged(nameof(ColorsJson));
	}

	private Dictionary<string, Color> GetDictionary()
	{
		Dictionary<string, Color> colors = new();

		foreach (ThemeColor themeColor in ThemeColors) 
			colors.Add(themeColor.Name, themeColor.Color);

		return colors;
	}

	private string GetJson()
	{
		JsonObject jsonObject = new() { { "ThemeName", JsonValue.Create(ThemeName) } };

		foreach (ThemeColor themeColor in ThemeColors) 
			jsonObject.Add(themeColor.Name, JsonValue.Create("#" + themeColor.Color.ToUInt32().ToString("x8", CultureInfo.InvariantCulture).ToUpper()));
        
		return jsonObject.ToJsonString(new JsonSerializerOptions
		{
			WriteIndented = true
		});
	}

	private void OnColorChange(ThemeColor themeColor, Color newColor, Color previousColor)
	{
		IEditorCommand command = new ColorChangeCommand(themeColor, ResourceDictionary, previousColor, newColor);
		ExecuteCommand(command);
	}

	private ThemeColor CreateThemeColor(string key, Color color)
	{
		ThemeColor themeColor = new(key, color);
		themeColor.ColorChanged += OnColorChange;

		return themeColor;
	}
}