using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using PleasantUI.Core;
using PleasantUI.Core.Constants;
using PleasantUI.Core.Enums;
using PleasantUI.Core.Helpers;
using PleasantUI.Extensions.Media;

namespace PleasantUI;

/// <summary>
/// Includes the pleasant theme in an application
/// </summary>
public class PleasantTheme : Styles, IResourceNode
{
    /// <summary>
    /// Specifies how many accent colors PleasantTheme will create (for both light and dark at the same time)
    /// </summary>
    private const int AccentColorCount = 3;
    
    /// <summary>
    /// Percentage at which the brightness of the color is increased or decreased per step
    /// </summary>
    private const float AccentColorPercentStep = 0.15f;
    
    private IPlatformSettings? _platformSettings;

    private ResourceDictionary? _accentColorsDictionary;
    private ResourceDictionary? _foregroundAccentColorsDictionary;
    
    private readonly ResourceDictionary _mainResourceDictionary;

    public AvaloniaList<ThemeVariant> CustomThemeVariants { get; } = new();
    
    public ThemeVariant? SelectedCustomThemeVariant { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PleasantTheme"/> class
    /// </summary>
    /// <param name="serviceProvider">The parent's service provider</param>
    public PleasantTheme(IServiceProvider? serviceProvider = null)
    {
        AvaloniaXamlLoader.Load(serviceProvider, this);

        _mainResourceDictionary = (Resources as ResourceDictionary)!;
        
        LoadCustomThemes();
        
        Init();
    }

    public string GetThemeTemplate()
    {
        ResourceDictionary lightTheme = (_mainResourceDictionary.ThemeDictionaries[ThemeVariant.Light] as ResourceDictionary)!;

        JsonObject jsonObject = new();

        foreach (KeyValuePair<object, object?> color in lightTheme)
        {
            if (color.Value is not Color)
                continue;
            
            jsonObject.Add(color.Key.ToString(), JsonValue.Create(color.Value.ToString().ToUpper()));
        }
        
        return jsonObject.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
    
    private void Init()
    {
        if (Application.Current is null)
            throw new NullReferenceException("Current application is not initialized. You need to load the xaml in the Initialize method");
        
        _platformSettings = Application.Current.PlatformSettings;

        if (_platformSettings is null) return;
        
        _platformSettings.ColorValuesChanged += PlatformSettingsOnColorValuesChanged;
        PleasantSettings.Instance.PropertyChanged += PleasantSettingsOnPropertyChanged;
        
        AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;

        ResolveTheme(_platformSettings);
        ResolveAccentColor(_platformSettings);
    }

    private void LoadCustomThemes()
    {
        if (!Directory.Exists(PleasantDirectories.Themes))
            Directory.CreateDirectory(PleasantDirectories.Themes);
        
        ClearCustomThemes();
        CustomThemeVariants.Clear();
        
        foreach (string file in Directory.EnumerateFiles(PleasantDirectories.Themes))
        {
            if (!file.EndsWith(".json"))
                continue;
            
            string fileName = Path.GetFileNameWithoutExtension(file);
            
            ResourceDictionary resourceDictionary = new();
            
            Dictionary<string, string> colors = ReadThemeFile(file);

            foreach (KeyValuePair<string,string> color in colors) 
                resourceDictionary.Add(color.Key, Color.Parse(color.Value));

            ThemeVariant themeVariant = new(fileName, ThemeVariant.Light);
            CustomThemeVariants.Add(themeVariant);
            
            _mainResourceDictionary.ThemeDictionaries.Add(themeVariant, resourceDictionary);
        }
    }

    private Dictionary<string, string> ReadThemeFile(string path)
    {
        Dictionary<string, string> dictionary = new();
        
        string json = File.ReadAllText(path);

        using JsonDocument jsonDocument = JsonDocument.Parse(json);
        JsonElement rootElement = jsonDocument.RootElement;

        foreach (JsonProperty jsonProperty in rootElement.EnumerateObject())
        {
            string key = jsonProperty.Name;
            string colorHex = jsonProperty.Value.ToString();
                
            dictionary.Add(key, colorHex);
        }

        return dictionary;
    }

    private void ClearCustomThemes()
    {
        foreach (KeyValuePair<ThemeVariant,IThemeVariantProvider> themeVariantProvider in _mainResourceDictionary.ThemeDictionaries)
        {
            if ((string)themeVariantProvider.Key.Key == "Light" || (string)themeVariantProvider.Key.Key == "Dark")
                continue;

            _mainResourceDictionary.Remove(themeVariantProvider);
        }
    }

    private void CurrentDomainOnProcessExit(object? sender, EventArgs e)
    {
        PleasantSettings.Save();
    }

    private void PleasantSettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_platformSettings is null) return;
        
        switch (e.PropertyName)
        {
            case nameof(PleasantSettings.Instance.Theme):
                ResolveTheme(_platformSettings);
                break;
            case nameof(PleasantSettings.Instance.NumericalAccentColor):
                UpdateAccentColors(Color.FromUInt32(PleasantSettings.Instance.NumericalAccentColor));
                break;
        }
    }

    private void UpdateAccentColors(Color accentColor)
    {
        if (_platformSettings is null) return;

        float lightPercent = 0.20f;
        float darkPercent = -0.20f;
        
        List<Color> lightColors = [];
        List<Color> darkColors = [];

        for (int i = 0; i < AccentColorCount; i++)
        {
            lightColors.Add(accentColor.GetLightenPercent(lightPercent));
            darkColors.Add(accentColor.GetLightenPercent(darkPercent));
            
            lightPercent += AccentColorPercentStep;
            darkPercent += AccentColorPercentStep;
        }
        
        UpdateAccentColors(accentColor, lightColors, darkColors);
        
        UpdateForegroundAccentColors(accentColor, lightColors, darkColors);
    }

    private void ResolveTheme(IPlatformSettings platformSettings)
    {
        ThemeVariant? themeVariant = PleasantSettings.Instance.Theme switch
        {
            Theme.System => GetThemeFromIPlatformSettings(platformSettings),
            Theme.Custom => SelectedCustomThemeVariant,
            
            Theme.Light => ThemeVariant.Light,
            Theme.Dark => ThemeVariant.Dark,

            Theme.Mint => PleasantThemes.Mint,
            Theme.Strawberry => PleasantThemes.Strawberry,
            
            _ => throw new ArgumentOutOfRangeException()
        };

        if (Application.Current is not null)
            Application.Current.RequestedThemeVariant = themeVariant;
    }

    private void ResolveAccentColor(IPlatformSettings platformSettings)
    {
        if (!PleasantSettings.Instance.PreferUserAccentColor)
            PleasantSettings.Instance.NumericalAccentColor = platformSettings.GetColorValues().AccentColor1.ToUInt32();

        Color accentColor = Color.FromUInt32(PleasantSettings.Instance.NumericalAccentColor);
        UpdateAccentColors(accentColor);
    }

    private void PlatformSettingsOnColorValuesChanged(object? sender, PlatformColorValues e)
    {
        if (PleasantSettings.Instance.Theme is Theme.System && Application.Current is not null)
        {
            ThemeVariant themeVariant = e.ThemeVariant is PlatformThemeVariant.Light ?
                ThemeVariant.Light : ThemeVariant.Dark;

            Application.Current.RequestedThemeVariant = themeVariant;
        }

        if (!PleasantSettings.Instance.PreferUserAccentColor)
        {
            Color accentColor = e.AccentColor1;

            PleasantSettings.Instance.NumericalAccentColor = accentColor.ToUInt32();
            
            UpdateAccentColors(accentColor);
        }
    }
    
    private ThemeVariant GetThemeFromIPlatformSettings(IPlatformSettings platformSettings)
    {
        return platformSettings.GetColorValues().ThemeVariant is PlatformThemeVariant.Light ?
                ThemeVariant.Light : ThemeVariant.Dark;
    }

    private Color GetForegroundFromAccent(Color accentColor)
    {
        double lum = ColorHelper.GetRelativeLuminance(accentColor);
        return lum <= 0.2 ? Colors.White : Colors.Black;
    }

    private void UpdateAccentColors(Color accentColor, List<Color> lightAccentColors, List<Color> darkAccentColors)
    {
        if (_accentColorsDictionary is not null)
            Resources.MergedDictionaries.Remove(_accentColorsDictionary);

        _accentColorsDictionary = new ResourceDictionary
        {
            { "SystemAccentColor", accentColor }
        };
        
        foreach (Color lightAccentColor in lightAccentColors)
            _accentColorsDictionary.Add($"SystemAccentLightColor{lightAccentColors.IndexOf(lightAccentColor) + 1}", lightAccentColor);
        
        foreach (Color darkAccentColor in darkAccentColors)
            _accentColorsDictionary.Add($"SystemAccentDarkColor{darkAccentColors.IndexOf(darkAccentColor) + 1}", darkAccentColor);
        
        Resources.MergedDictionaries.Add(_accentColorsDictionary);
    }
    
    private void UpdateForegroundAccentColors(Color accentColor, List<Color> lightAccentColors, List<Color> darkAccentColors)
    {
        if (_foregroundAccentColorsDictionary is not null)
            Resources.MergedDictionaries.Remove(_foregroundAccentColorsDictionary);

        _foregroundAccentColorsDictionary = new ResourceDictionary
        {
            { "ForegroundAccentColor", GetForegroundFromAccent(accentColor) },
        };

        foreach (Color lightAccentColor in lightAccentColors)
            _foregroundAccentColorsDictionary.Add($"ForegroundAccentLightColor{lightAccentColors.IndexOf(lightAccentColor) + 1}", GetForegroundFromAccent(lightAccentColor));
        
        foreach (Color darkAccentColor in darkAccentColors)
            _foregroundAccentColorsDictionary.Add($"ForegroundAccentDarkColor{darkAccentColors.IndexOf(darkAccentColor) + 1}", GetForegroundFromAccent(darkAccentColor));
        
        Resources.MergedDictionaries.Add(_foregroundAccentColorsDictionary);
    }
}