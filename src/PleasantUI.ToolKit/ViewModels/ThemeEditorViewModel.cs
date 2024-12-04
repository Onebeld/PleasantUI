using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using PleasantUI.Controls;
using PleasantUI.Core.Extensions;
using PleasantUI.Core.Interfaces;
using PleasantUI.Core.Localization;
using PleasantUI.Core.Models;
using PleasantUI.ToolKit.Commands;
using PleasantUI.ToolKit.Commands.ThemeEditor;
using PleasantUI.ToolKit.Messages;
using PleasantUI.ToolKit.Models;

namespace PleasantUI.ToolKit.ViewModels;

/// <summary>
/// ViewModel for the <see cref="PleasantUI.ToolKit.ThemeEditorWindow" />.
/// </summary>
public class ThemeEditorViewModel : ObservableObject
{
    private readonly ThemeEditorWindow _themeEditorWindow;
    private readonly IPleasantWindow _pleasantWindowParent;
    
    private readonly Stack<IEditorCommand> _redoStack = new();
    private readonly Stack<IEditorCommand> _undoStack = new();
    
    private AvaloniaList<Theme> _themes = new();
    private CustomTheme _customTheme;
    private string _themeName;

    /// <summary>
    /// Gets the resource dictionary containing the theme colors.
    /// </summary>
    public readonly ResourceDictionary ResourceDictionary;
    
    /// <summary>
    /// Gets or sets the custom theme being edited.
    /// </summary>
    public CustomTheme CustomTheme
    {
        get => _customTheme;
        set => SetProperty(ref _customTheme, value);
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
        set => SetProperty(ref _themes, value);
    }

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
    /// Initializes a new instance of the <see cref="ThemeEditorViewModel" /> class.
    /// </summary>
    /// <param name="windowParent">The parent window.</param>
    /// <param name="themeEditorWindow">The theme editor window.</param>
    /// <param name="customTheme">The custom theme to edit (optional).</param>
    public ThemeEditorViewModel(IPleasantWindow windowParent, ThemeEditorWindow themeEditorWindow,
        CustomTheme? customTheme)
    {
        _customTheme = customTheme ?? new CustomTheme(null, "NewTheme", PleasantTheme.GetThemeTemplateDictionary());
        _themeName = _customTheme.Name;

        ResourceDictionary = new ResourceDictionary();

        _pleasantWindowParent = windowParent;

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

        themeEditorWindow.ThemeVariantScope.Resources.ThemeDictionaries.Add(_customTheme.ThemeVariant,
            ResourceDictionary);
        _themeEditorWindow = themeEditorWindow;

        OnPropertyChanged(nameof(ColorsJson));
        RegisterMessengers();
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
    public async Task CopyThemeAsync()
    {
        await TopLevel.GetTopLevel(_pleasantWindowParent as Visual)?.Clipboard?.SetTextAsync(ColorsJson)!;

        Geometry? icon = ResourceExtensions.GetResource<Geometry>("CopyRegular");

        if (!Localizer.Instance.TryGetString("ThemeCopiedToClipboard", out string text))
            text = "Theme copied to clipboard";

        PleasantSnackbar.Show(_pleasantWindowParent, text, icon: icon);
    }

    /// <summary>
    /// Parses a JSON string from the clipboard and applies its colors to the current theme.
    /// </summary>
    public async Task PasteThemeAsync()
    {
        string? data = await TopLevel.GetTopLevel(_pleasantWindowParent as Visual)?.Clipboard?.GetTextAsync()!;

        ParseColorsFromJson(data);
    }

    /// <summary>
    /// Opens a file picker for the user to select a JSON file containing a theme, and applies the theme's colors to the current theme.
    /// </summary>
    public async Task ImportThemeAsync()
    {
        TopLevel? topLevel = TopLevel.GetTopLevel(_pleasantWindowParent as Visual);

        if (topLevel is null)
            return;

        IReadOnlyList<IStorageFile> pickerFileTypes = await topLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                FileTypeFilter = [new FilePickerFileType("JSON") { Patterns = ["*.json"] }],
                AllowMultiple = false
            });

        if (pickerFileTypes.Count == 0)
            return;

        string json = File.ReadAllText(pickerFileTypes[0].Path.LocalPath);

        ParseColorsFromJson(json);
    }

    /// <summary>
    /// Opens a save file picker for the user to select a location to export the current theme as a JSON file.
    /// </summary>
    public async Task ExportThemeAsync()
    {
        TopLevel? topLevel = TopLevel.GetTopLevel(_pleasantWindowParent as Visual);

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

        if (!Localizer.Instance.TryGetString("ThemeExported", out string themeExportedText))
            themeExportedText = "Theme exported";

        PleasantSnackbar.Show(_pleasantWindowParent, themeExportedText, icon: fileExportIcon,
            notificationType: NotificationType.Success);
    }

    /// <summary>
    /// Changes the name of the theme.
    /// </summary>
    /// <param name="newName">The new name of the theme.</param>
    public void ChangeThemeName(string newName)
    {
        _themeName = newName;
        OnPropertyChanged(nameof(ThemeName));
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

    /// <summary>
    /// Applies the theme colors from the specified JSON file.
    /// </summary>
    /// <param name="file">The JSON file to apply the theme colors from.</param>
    public void DropFile(IStorageItem file)
    {
        string json = File.ReadAllText(file.Path.AbsolutePath);

        ParseColorsFromJson(json);
    }

    private void ParseColorsFromJson(string? json)
    {
        JsonDocument jsonDocument;

        try
        {
            if (json is null)
                throw new ArgumentNullException(nameof(json));
            
            jsonDocument = JsonDocument.Parse(json);
        }
        catch (Exception)
        {
            Logger.Sink?.Log(LogEventLevel.Error, "Theme", this, "Error when parsing colors from json");

            Geometry? closeCircleIcon = ResourceExtensions.GetResource<Geometry>("CloseCircleRegular");

            if (!Localizer.Instance.TryGetString("ThemeImportError", out string themeImportErrorText))
                themeImportErrorText = "An error occurred while importing the theme";

            PleasantSnackbar.Show(_pleasantWindowParent, themeImportErrorText, icon: closeCircleIcon,
                notificationType: NotificationType.Error);

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

        if (!Localizer.Instance.TryGetString("ThemeImported", out string themeImportedText))
            themeImportedText = "The theme has been successfully imported";

        PleasantSnackbar.Show(_pleasantWindowParent, themeImportedText, icon: fileImportIcon,
            notificationType: NotificationType.Success);
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
        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));

        OnPropertyChanged(nameof(ColorsJson));
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
            jsonObject.Add(themeColor.Name,
                JsonValue.Create("#" + themeColor.Color.ToUInt32().ToString("x8", CultureInfo.InvariantCulture)
                    .ToUpper()));

        return jsonObject.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private ThemeColor CreateThemeColor(string key, Color color) => new(key, color);

    private void RegisterMessengers()
    {
        WeakReferenceMessenger.Default.Register<ThemeEditorViewModel, AsyncRequestColorMessage>(this, static (recipient, message) =>
        {
            async Task<Color?> ReceiveAsync(ThemeEditorViewModel recipient, AsyncRequestColorMessage requestColorMessage)
            {
                Color? newColor = await ColorPickerWindow.SelectColor(recipient._pleasantWindowParent, requestColorMessage.PreviousColor.ToUInt32());
                return newColor;
            }
            
            message.Reply(ReceiveAsync(recipient, message));
        });
        
        WeakReferenceMessenger.Default.Register<ThemeEditorViewModel, ColorChangedMessage>(this, (recipient, message) =>
        {
            IEditorCommand command = new ColorChangeCommand(message.ThemeColor, ResourceDictionary, message.PreviousValue, message.Value);
            recipient.ExecuteCommand(command);
        });
        
        WeakReferenceMessenger.Default.Register<ThemeEditorViewModel, AsyncClipboardSetColorMessage>(this, static (recipient, message) =>
        {
            async Task<bool> ReceiveAsync(ThemeEditorViewModel recipient, AsyncClipboardSetColorMessage setColorMessage)
            {
                await TopLevel.GetTopLevel(recipient._pleasantWindowParent as Visual)?.Clipboard
                    ?.SetTextAsync(setColorMessage.Color.ToString().ToUpper())!;
                return true;
            }

            message.Reply(ReceiveAsync(recipient, message));
            
            Geometry? icon = ResourceExtensions.GetResource<Geometry>("CopyRegular");

            if (!Localizer.Instance.TryGetString("ColorCopiedToClipboard", out string text))
                text = "The color is copied to the clipboard";

            PleasantSnackbar.Show(recipient._pleasantWindowParent, text, icon: icon);
        });
        
        WeakReferenceMessenger.Default.Register<ThemeEditorViewModel, AsyncRequestClipboardColorMessage>(this, (recipient, message) =>
        {
            async Task<Color?> ReceiveAsync(ThemeEditorViewModel viewModel)
            {
                string? data = await TopLevel.GetTopLevel(viewModel._pleasantWindowParent as Visual)?.Clipboard
                    ?.GetTextAsync()!;

                if (!Color.TryParse(data, out Color newColor))
                    return null;

                return newColor;
            }

            message.Reply(ReceiveAsync(recipient));
        });
    }
}