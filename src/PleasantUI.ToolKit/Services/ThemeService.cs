using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Logging;
using Avalonia.Media;
using PleasantUI.Core.Models;
using PleasantUI.Core.Models.Interfaces;
using PleasantUI.ToolKit.Commands;
using PleasantUI.ToolKit.Commands.ThemeEditor;
using PleasantUI.ToolKit.Models;
using PleasantUI.ToolKit.Services.Interfaces;

namespace PleasantUI.ToolKit.Services;

public class ThemeService : IThemeService
{
    private readonly TopLevel _topLevel;
    
    private readonly Stack<IEditorCommand> _redoStack = new();
    private readonly Stack<IEditorCommand> _undoStack = new();

    private Dictionary<string, Action<ThemeColor>> _actions;
    
    private string _themeName;

    public IEventAggregator EventAggregator { get; }
    
    public AvaloniaList<ThemeColor> ThemeColors { get; } = new();

    public ResourceDictionary ResourceDictionary { get; } = new();

    public string JsonColors => GetJsonTheme(ThemeName, ThemeColors);

    public bool CanUndo => _undoStack.Count > 0;

    public bool CanRedo => _redoStack.Count > 0;

    public string ThemeName
    {
        get => _themeName;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                return;
            
            ExecuteCommand(new ThemeNameChangeCommand(this, _themeName, value));
        }
    }

    public ThemeService(TopLevel? topLevel, IEventAggregator eventAggregator)
    {
        ArgumentNullException.ThrowIfNull(topLevel);

        _topLevel = topLevel;
        EventAggregator = eventAggregator;
    }

    public void CreateThemeColors(CustomTheme customTheme)
    {
        foreach (KeyValuePair<string, Color> pair in customTheme.Colors)
        {
            ResourceDictionary.Add(pair.Key, pair.Value);
            ThemeColors.Add(new ThemeColor(pair.Key, pair.Value, EventAggregator));
        }
    }

    public void ChangeColor(ThemeColor themeColor, Color newColor, Color previousColor)
    {
        ExecuteCommand(new ColorChangeCommand(themeColor, ResourceDictionary, previousColor, newColor));
    }

    public async Task CopyColorAsync(Color color)
    {
        await _topLevel.Clipboard?.SetTextAsync(ConvertColorToHex(color));
    }

    public async Task<Color?> PasteColorAsync()
    {
        string? text = await _topLevel.Clipboard?.GetTextAsync();

        if (text is null)
            return null;

        if (Color.TryParse(text, out Color color))
            return color;

        return null;
    }

    public TopLevel GetTopLevel()
    {
        return _topLevel;
    }

    public async Task CopyThemeAsync()
    {
        _topLevel.Clipboard?.SetTextAsync(JsonColors);
    }

    public async Task<bool> PasteThemeAsync()
    {
        string? data = await _topLevel.Clipboard?.GetTextAsync();

        return ImportJson(data);
    }

    public async Task ExportThemeAsync(string path)
    {
        await File.WriteAllTextAsync(path, JsonColors);
    }

    public bool Undo()
    {
        if (!CanUndo) return false;

        IEditorCommand command = _undoStack.Pop();
        command.Undo();
        _redoStack.Push(command);

        return true;
    }

    public bool Redo()
    {
        if (!CanRedo) return false;

        IEditorCommand command = _redoStack.Pop();
        command.Redo();
        _undoStack.Push(command);

        return true;
    }

    public bool ImportJson(string? json)
    {
        (Dictionary<string, Color>? colors, string? themeName) = ParseColors(json);
        
        if (colors is null)
            return false;
        
        ExecuteCommand(new ThemeChangeCommand(this, GetDictionaryFromThemeColors(), colors, ThemeName, themeName));

        return true;
    }

    private string GetJsonTheme(string themeName, ICollection<ThemeColor> themeColors)
    {
        JsonObject jsonObject = new()
        {
            {
                "ThemeName", JsonValue.Create(themeName)
            }
        };

        foreach (ThemeColor themeColor in themeColors)
        {
            JsonValue value = JsonValue.Create(ConvertColorToHex(themeColor.Color));
            
            jsonObject.Add(themeColor.Name, value);
        }

        return jsonObject.ToJsonString(new JsonSerializerOptions()
        {
            WriteIndented = true
        });
    }

    public void GetColorsFromTheme(ITheme theme)
    {
        Dictionary<string, Color> colors = PleasantTheme.GetColorsDictionary(theme);
        
        ExecuteCommand(new ThemeChangeCommand(this, GetDictionaryFromThemeColors(), colors, ThemeName, theme.Name));
    }

    public void ChangeThemeName(string name)
    {
        _themeName = name;
    }

    private static string ConvertColorToHex(Color color)
    {
        return $"#{color.ToUInt32().ToString("x8", CultureInfo.InvariantCulture).ToUpper()}";
    }

    private (Dictionary<string, Color>?, string? themeName) ParseColors(string? json)
    {
        JsonDocument jsonDocument;

        try
        {
            ArgumentNullException.ThrowIfNull(json);
            jsonDocument = JsonDocument.Parse(json);
        }
        catch (Exception)
        {
            Logger.Sink?.Log(LogEventLevel.Error, "Theme", this, "Error when parsing colors from json");

            return (null, null);
        }

        string? themeName = null;
        Dictionary<string, Color> colors = new();
        
        if (jsonDocument.RootElement.TryGetProperty(nameof(ThemeName), out JsonElement element))
            themeName = element.GetString();

        foreach (JsonProperty property in jsonDocument.RootElement.EnumerateObject())
        {
            string name = property.Name;
            string? hexColor = property.Value.GetString();
            
            if (hexColor is null)
                continue;
            
            if (!Color.TryParse(hexColor, out Color color))
                continue;

            colors.Add(name, color);
        }

        jsonDocument.Dispose();

        return (colors, themeName);
    }

    private void ExecuteCommand(IEditorCommand command)
    {
        command.Redo();
        
        _undoStack.Push(command);
        _redoStack.Clear();
    }

    private Dictionary<string, Color> GetDictionaryFromThemeColors()
    {
        return ThemeColors.ToDictionary(themeColor => themeColor.Name, themeColor => themeColor.Color);
    }
}