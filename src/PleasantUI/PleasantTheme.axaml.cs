using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using PleasantUI.Core;
using PleasantUI.Core.Constants;
using PleasantUI.Core.Extensions;
using PleasantUI.Core.Extensions.Media;
using PleasantUI.Core.GenerationContexts;
using PleasantUI.Core.Models;
using PleasantUI.Core.Settings;

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

    private readonly bool _isInitialized;

    private ResourceDictionary? _accentColorsDictionary;

    private IPlatformSettings? _platformSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="PleasantTheme" /> class
    /// </summary>
    /// <param name="serviceProvider">The parent's service provider</param>
    public PleasantTheme(IServiceProvider? serviceProvider = null)
    {
        AvaloniaXamlLoader.Load(serviceProvider, this);
        
        PleasantSettings.Initialize(new AppSettingsProvider<PleasantSettings>(), PleasantSettingsGenerationContext.Default);
        PleasantSettings.Current = PleasantSettings.Load(Path.Combine(PleasantDirectories.Settings, PleasantFileNames.Settings));

        _mainResourceDictionary = (Resources as ResourceDictionary)!;

        CustomThemes.CollectionChanged += CustomThemesOnCollectionChanged;

        GetPleasantThemes();

        LoadCustomThemes();

        Init();

        _isInitialized = true;
    }

    /// <summary>
    /// Gets or sets the selected custom theme.
    /// </summary>
    /// <value>The selected custom theme.</value>
    public static CustomTheme? SelectedCustomTheme
    {
        get => _customTheme;
        set
        {
            _customTheme = value;

            PleasantSettings.Current.CustomThemeId = value?.Id;

            _customThemeChanged?.Invoke();
        }
    }

    /// <summary>
    /// Gets the list of custom themes that can be used in the application.
    /// </summary>
    public static AvaloniaList<CustomTheme> CustomThemes { get; } = new();

    /// <summary>
    /// Gets the array of available themes.
    /// </summary>
    public static Theme[] Themes { get; private set; } = null!;

    private void CustomThemesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (!_isInitialized || _platformSettings is null) return;

        UpdateCustomThemes();

        if (e.Action is NotifyCollectionChangedAction.Remove)
            ResolveTheme(_platformSettings);
    }

    /// <summary>
    /// Returns a dictionary of colors for the default theme.
    /// </summary>
    /// <returns>A dictionary of colors for the default theme.</returns>
    public static Dictionary<string, Color> GetThemeTemplateDictionary()
    {
        return GetColorsDictionary(Themes.First(theme => theme.Name == "Light"));
    }

    /// <summary>
    /// Gets the count of keys in the default theme.
    /// </summary>
    /// <returns>The count of keys in the default theme.</returns>
    public static int CountOfKeysTheme()
    {
        Theme theme = Themes.First(theme => theme.Name == "Light");
        
        if (theme.ThemeVariant is null)
            throw new NullReferenceException("Theme Variant is null");
        
        ResourceDictionary lightThemeBasicColors =
            (_mainResourceDictionary.ThemeDictionaries[theme.ThemeVariant] as ResourceDictionary)!;

        return lightThemeBasicColors.Keys.Count;
    }

    /// <summary>
    /// Returns a dictionary of colors for the theme.
    /// </summary>
    /// <param name="theme">The theme to get the colors for.</param>
    /// <returns>A dictionary of colors for the theme.</returns>
    /// <remarks>
    /// This method returns a dictionary of colors for the theme, which can be used to override the default theme colors.
    /// The dictionary will contain all the colors from the default theme, as well as any custom colors that have been set.
    /// </remarks>
    public static Dictionary<string, Color> GetColorsDictionary(Theme theme)
    {
        if (Application.Current is null)
            throw new NullReferenceException("Application.Current is null");

        if (theme.ThemeVariant is null)
            throw new NullReferenceException("Theme Variant is null");

        // Getting basic colors
        ResourceDictionary lightThemeBasicColors =
            (_mainResourceDictionary.ThemeDictionaries[theme.ThemeVariant] as ResourceDictionary)!;

        ResourceDictionary? lightThemeCustomColors = null;
        
        if (Application.Current.Resources.ThemeDictionaries.ContainsKey(theme.ThemeVariant))
            lightThemeCustomColors = (Application.Current.Resources.ThemeDictionaries[theme.ThemeVariant] as ResourceDictionary)!;

        Dictionary<string, Color> newDictionary = lightThemeBasicColors.ToDictionary<string, Color>();

        if (lightThemeCustomColors is null)
            return newDictionary;

        // Adding custom colors
        foreach (KeyValuePair<object, object?> pair in lightThemeCustomColors)
            newDictionary.Add((string)pair.Key, (Color)pair.Value!);

        return newDictionary;
    }

    /// <summary>
    /// Updates the custom themes dictionary with the current list of custom themes.
    /// </summary>
    /// <remarks>
    /// This method is called when the list of custom themes changes.
    /// It clears the custom themes dictionary and adds each custom theme
    /// to the dictionary with its theme variant as the key and
    /// its colors as the value.
    /// </remarks>
    public void UpdateCustomThemes()
    {
        ClearCustomThemes();

        foreach (CustomTheme customTheme in CustomThemes)
            _mainResourceDictionary.ThemeDictionaries.Add(customTheme.ThemeVariant,
                customTheme.Colors.ToResourceDictionary());
    }

    /// <summary>
    /// Edits a custom theme.
    /// </summary>
    /// <param name="currentCustomTheme">The current custom theme.</param>
    /// <param name="newCustomTheme">The new custom theme.</param>
    /// <remarks>
    /// This method first updates the name and colors of the current custom theme.
    /// Then it clears the custom themes dictionary and re-adds each custom theme
    /// to the dictionary with its theme variant as the key and
    /// its colors as the value.
    /// Finally, it resolves the theme to apply the new colors.
    /// </remarks>
    public void EditCustomTheme(CustomTheme currentCustomTheme, CustomTheme newCustomTheme)
    {
        if (_platformSettings is null) return;

        currentCustomTheme.Name = newCustomTheme.Name;
        currentCustomTheme.Colors = newCustomTheme.Colors;

        ClearCustomThemes();

        foreach (CustomTheme customTheme in CustomThemes)
            _mainResourceDictionary.ThemeDictionaries.Add(customTheme.ThemeVariant,
                customTheme.Colors.ToResourceDictionary());

        ResolveTheme(_platformSettings);
    }

    private void Init()
    {
        if (Application.Current is null)
            throw new NullReferenceException(
                "Current application is not initialized. You need to load the xaml in the Initialize method");

        _platformSettings = Application.Current.PlatformSettings;

        if (_platformSettings is null) return;

        _platformSettings.ColorValuesChanged += PlatformSettingsOnColorValuesChanged;
        PleasantSettings.Current.PropertyChanged += PleasantSettingsOnPropertyChanged;
        _customThemeChanged += () => ResolveTheme(_platformSettings);

        AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;

        if (PleasantSettings.Current.CustomThemeId is not null)
            SelectedCustomTheme = CustomThemes.FirstOrDefault(x => x.Id == PleasantSettings.Current.CustomThemeId);

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
                _mainResourceDictionary.ThemeDictionaries.Add(customTheme.ThemeVariant,
                    customTheme.Colors.ToResourceDictionary());
            }
        }
        catch (Exception)
        {
            Logger.Sink?.Log(LogEventLevel.Error, "Theme", this, "Error when loading themes");
        }
    }

    private void GetPleasantThemes()
    {
        int countThemes = _mainResourceDictionary.ThemeDictionaries.Count;

        Themes = new Theme[countThemes + 2];

        Themes[0] = new Theme("System", null);

        for (int i = 0; i < countThemes; i++)
        {
            KeyValuePair<ThemeVariant, IThemeVariantProvider> element =
                _mainResourceDictionary.ThemeDictionaries.ElementAt(i);

            Themes[i + 1] = new Theme(element.Key.Key.ToString(), element.Key);
        }

        Themes[countThemes + 1] = new Theme("Custom", null);
    }

    private void ClearCustomThemes()
    {
        foreach (KeyValuePair<ThemeVariant, IThemeVariantProvider> themeVariantProvider in _mainResourceDictionary
                     .ThemeDictionaries)
        {
            if (Array.Exists(Themes, theme => theme.Name == (string)themeVariantProvider.Key.Key))
                continue;

            _mainResourceDictionary.ThemeDictionaries.Remove(themeVariantProvider.Key);
        }
    }

    private void CurrentDomainOnProcessExit(object? sender, EventArgs e)
    {
        PleasantSettings.Save(PleasantSettings.Current, Path.Combine(PleasantDirectories.Settings, PleasantFileNames.Settings));
        PleasantThemesLoader.Save();
    }

    private void PleasantSettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_platformSettings is null) return;

        switch (e.PropertyName)
        {
            case nameof(PleasantSettings.Current.Theme):
                ResolveTheme(_platformSettings);
                break;
            case nameof(PleasantSettings.Current.NumericalAccentColor):
                UpdateAccentColors(Color.FromUInt32(PleasantSettings.Current.NumericalAccentColor));
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
    }

    private void ResolveTheme(IPlatformSettings platformSettings)
    {
        ThemeVariant? themeVariant;

        if (PleasantSettings.Current.Theme == "Custom")
            themeVariant = SelectedCustomTheme?.ThemeVariant;
        else if (PleasantSettings.Current.Theme == "System")
            themeVariant = GetThemeFromIPlatformSettings(platformSettings);
        else
            themeVariant = Themes.FirstOrDefault(theme => theme.Name == PleasantSettings.Current.Theme)?.ThemeVariant;

        if (Application.Current is not null)
            Application.Current.RequestedThemeVariant = themeVariant;
    }

    private void ResolveAccentColor(IPlatformSettings platformSettings)
    {
        if (!PleasantSettings.Current.PreferUserAccentColor)
            PleasantSettings.Current.NumericalAccentColor = platformSettings.GetColorValues().AccentColor1.ToUInt32();

        Color accentColor = Color.FromUInt32(PleasantSettings.Current.NumericalAccentColor);
        UpdateAccentColors(accentColor);
    }

    private void PlatformSettingsOnColorValuesChanged(object? sender, PlatformColorValues e)
    {
        if (PleasantSettings.Current.Theme == "System" && Application.Current is not null)
        {
            ThemeVariant themeVariant =
                e.ThemeVariant is PlatformThemeVariant.Light ? ThemeVariant.Light : ThemeVariant.Dark;

            Application.Current.RequestedThemeVariant = themeVariant;
        }

        if (!PleasantSettings.Current.PreferUserAccentColor)
        {
            Color accentColor = e.AccentColor1;

            PleasantSettings.Current.NumericalAccentColor = accentColor.ToUInt32();

            UpdateAccentColors(accentColor);
        }
    }

    private ThemeVariant GetThemeFromIPlatformSettings(IPlatformSettings platformSettings)
    {
        return platformSettings.GetColorValues().ThemeVariant is PlatformThemeVariant.Light
            ? ThemeVariant.Light
            : ThemeVariant.Dark;
    }

    private void UpdateAccentColors(Color accentColor, List<Color> lightAccentColors, List<Color> darkAccentColors)
    {
        if (_accentColorsDictionary is not null)
            Resources.MergedDictionaries.Remove(_accentColorsDictionary);

        _accentColorsDictionary = new ResourceDictionary
        {
            { "AccentColor", accentColor }
        };

        foreach (Color lightAccentColor in lightAccentColors)
            _accentColorsDictionary.Add($"AccentLightColor{lightAccentColors.IndexOf(lightAccentColor) + 1}",
                lightAccentColor);

        foreach (Color darkAccentColor in darkAccentColors)
            _accentColorsDictionary.Add($"AccentDarkColor{darkAccentColors.IndexOf(darkAccentColor) + 1}",
                darkAccentColor);

        Resources.MergedDictionaries.Add(_accentColorsDictionary);
    }
}