using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using PleasantUI.Core;
using PleasantUI.Core.Helpers;
using PleasantUI.Core.Models;
using PleasantUI.Core.Structures;
using PleasantUI.Extensions;
using PleasantUI.Extensions.Media;
using Serilog;

namespace PleasantUI;

/// <summary>
/// Includes the pleasant theme in an application
/// </summary>
public class PleasantTheme : Styles
{
    private const float AccentColorPercentStep = 0.15f;
    private const int AccentColorCount = 3;
    
    private static ResourceDictionary _mainResourceDictionary = null!;
    private static CustomTheme? _customTheme;
    private static Action? _customThemeChanged;
    
    private ResourceDictionary? _accentColorsDictionary;
    private ResourceDictionary? _foregroundAccentColorsDictionary;
    
    private IPlatformSettings? _platformSettings;
    
    private readonly bool _isInitialized;

    public static CustomTheme? SelectedCustomTheme
    {
        get => _customTheme;
        set
        {
            _customTheme = value;

            PleasantSettings.Instance.CustomThemeId = value?.Id;
            
            _customThemeChanged?.Invoke();
        }
    }
    
    public static AvaloniaList<CustomTheme> CustomThemes { get; } = new();

    public static Theme[] Themes { get; private set; } = null!;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PleasantTheme"/> class
    /// </summary>
    /// <param name="serviceProvider">The parent's service provider</param>
    public PleasantTheme(IServiceProvider? serviceProvider = null)
    {
        AvaloniaXamlLoader.Load(serviceProvider, this);

        _mainResourceDictionary = (Resources as ResourceDictionary)!;
        
        CustomThemes.CollectionChanged += CustomThemesOnCollectionChanged;
        
        GetPleasantThemes();
        
        LoadCustomThemes();
        
        Init();

        _isInitialized = true;
    }

    private void CustomThemesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (!_isInitialized || _platformSettings is null) return;
        
        UpdateCustomThemes();
        
        if (e.Action is NotifyCollectionChangedAction.Remove)
            ResolveTheme(_platformSettings);
    }

    public static Dictionary<string, Color> GetThemeTemplateDictionary()
    {
        return GetColorsDictionary(Themes.First(theme => theme.Name == "Light"));
    }

    public static Dictionary<string, Color> GetColorsDictionary(Theme theme)
    {
        if (Application.Current is null)
            throw new NullReferenceException("Application.Current is null");
        
        if (theme.ThemeVariant is null)
            throw new NullReferenceException("Theme Variant is null");
        
        // Getting basic colors
        ResourceDictionary lightThemeBasicColors = (_mainResourceDictionary.ThemeDictionaries[theme.ThemeVariant] as ResourceDictionary)!;

        ResourceDictionary? lightThemeCustomColors = null;

        try
        {
            lightThemeCustomColors = (Application.Current.Resources.ThemeDictionaries[theme.ThemeVariant] as ResourceDictionary)!;
        }
        catch { }

        Dictionary<string, Color> newDictionary = lightThemeBasicColors.ToDictionary<string, Color>();

        if (lightThemeCustomColors is not null)
        {
            foreach (KeyValuePair<object, object?> pair in lightThemeCustomColors) 
                newDictionary.Add((string)pair.Key, (Color)pair.Value);
        }

        return newDictionary;
    }

    public void UpdateCustomThemes()
    {
        ClearCustomThemes();

        foreach (CustomTheme customTheme in CustomThemes)
            _mainResourceDictionary.ThemeDictionaries.Add(customTheme.ThemeVariant, customTheme.Colors.ToResourceDictionary());
    }

    public void EditCustomTheme(CustomTheme currentCustomTheme, CustomTheme newCustomTheme)
    {
        if (_platformSettings is null) return;
        
        currentCustomTheme.Name = newCustomTheme.Name;
        currentCustomTheme.Colors = newCustomTheme.Colors;
        
        ClearCustomThemes();
        
        foreach (CustomTheme customTheme in CustomThemes) 
            _mainResourceDictionary.ThemeDictionaries.Add(customTheme.ThemeVariant, customTheme.Colors.ToResourceDictionary());
        
        ResolveTheme(_platformSettings);
    }
    
    private void Init()
    {
        if (Application.Current is null)
            throw new NullReferenceException("Current application is not initialized. You need to load the xaml in the Initialize method");
        
        _platformSettings = Application.Current.PlatformSettings;

        if (_platformSettings is null) return;
        
        _platformSettings.ColorValuesChanged += PlatformSettingsOnColorValuesChanged;
        PleasantSettings.Instance.PropertyChanged += PleasantSettingsOnPropertyChanged;
        _customThemeChanged += () => ResolveTheme(_platformSettings);
        
        AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;

        if (PleasantSettings.Instance.CustomThemeId is not null) 
            SelectedCustomTheme = CustomThemes.FirstOrDefault(x => x.Id == PleasantSettings.Instance.CustomThemeId);

        ResolveTheme(_platformSettings);
        ResolveAccentColor(_platformSettings);
    }

    private void LoadCustomThemes()
    {
        ClearCustomThemes();
        CustomThemes.Clear();
        
        try
        {
            CustomTheme[] customThemes = PleasantThemesLoader.Load();
            
            foreach (CustomTheme customTheme in customThemes)
            {
                CustomThemes.Add(customTheme);
                _mainResourceDictionary.ThemeDictionaries.Add(customTheme.ThemeVariant, customTheme.Colors.ToResourceDictionary());
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Error when loading themes");
        }
        
    }

    private void GetPleasantThemes()
    {
        int countThemes = _mainResourceDictionary.ThemeDictionaries.Count;

        Themes = new Theme[countThemes + 2];

        Themes[0] = new Theme("System", null);

        for (int i = 0; i < countThemes; i++)
        {
            KeyValuePair<ThemeVariant, IThemeVariantProvider> element = _mainResourceDictionary.ThemeDictionaries.ElementAt(i);
            
            Themes[i + 1] = new Theme(element.Key.Key.ToString(), element.Key);
        }

        Themes[countThemes + 1] = new Theme("Custom", null);
    }

    private void ClearCustomThemes()
    {
        foreach (KeyValuePair<ThemeVariant,IThemeVariantProvider> themeVariantProvider in _mainResourceDictionary.ThemeDictionaries)
        {
            if (Array.Exists(Themes, theme => theme.Name == (string)themeVariantProvider.Key.Key))
                continue;

            _mainResourceDictionary.ThemeDictionaries.Remove(themeVariantProvider.Key);
        }
    }

    private void CurrentDomainOnProcessExit(object? sender, EventArgs e)
    {
        PleasantSettings.Save();
        PleasantThemesLoader.Save();
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
        ThemeVariant? themeVariant;
        
        if (PleasantSettings.Instance.Theme == "Custom") 
            themeVariant = SelectedCustomTheme?.ThemeVariant;
        else if (PleasantSettings.Instance.Theme == "System")
            themeVariant = GetThemeFromIPlatformSettings(platformSettings);
        else
            themeVariant = Themes.FirstOrDefault(theme => theme.Name == PleasantSettings.Instance.Theme)?.ThemeVariant;

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
        if (PleasantSettings.Instance.Theme == "System" && Application.Current is not null)
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
            { "ForegroundAccentColor", GetForegroundFromAccent(accentColor) }
        };

        foreach (Color lightAccentColor in lightAccentColors)
            _foregroundAccentColorsDictionary.Add($"ForegroundAccentLightColor{lightAccentColors.IndexOf(lightAccentColor) + 1}", GetForegroundFromAccent(lightAccentColor));
        
        foreach (Color darkAccentColor in darkAccentColors)
            _foregroundAccentColorsDictionary.Add($"ForegroundAccentDarkColor{darkAccentColors.IndexOf(darkAccentColor) + 1}", GetForegroundFromAccent(darkAccentColor));
        
        Resources.MergedDictionaries.Add(_foregroundAccentColorsDictionary);
    }
}