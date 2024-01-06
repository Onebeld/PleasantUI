using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using PleasantUI.Core;
using PleasantUI.Core.Enums;
using PleasantUI.Core.Exceptions;
using PleasantUI.Core.Helpers;
using PleasantUI.Extensions.Media;

namespace PleasantUI;

public class PleasantTheme : Style
{
    private const int AccentColorCount = 3;
    private const float AccentColorPercentStep = 0.15f;
    
    private IPlatformSettings? _platformSettings;

    private ResourceDictionary? _accentColorsDictionary;
    private ResourceDictionary? _foregroundAccentColorsDictionary;
    
    public PleasantTheme()
    {
        AvaloniaXamlLoader.Load(this);
        
        Init();
    }

    public void UpdateAccentColors(Color accentColor)
    {
        if (_platformSettings is null) return;

        float lightPercent = 0.25f;
        float darkPercent = -0.35f;
        
        List<Color> lightColors = new();
        List<Color> darkColors = new();

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

    public void UpdateTheme()
    {
        if (_platformSettings is null) return;
        
        ResolveTheme(_platformSettings);
    }

    private void Init()
    {
        if (Application.Current is null)
            throw new ApplicationNotInitializedException("Application.Current is not initialized. Create an instance of the PleasantTheme class after initializing Application.");
        
        _platformSettings = Application.Current.PlatformSettings;

        if (_platformSettings is null) return;
        
        _platformSettings.ColorValuesChanged += PlatformSettingsOnColorValuesChanged;

        ResolveTheme(_platformSettings);
        ResolveAccentColor(_platformSettings);
    }

    private void ResolveTheme(IPlatformSettings platformSettings)
    {
        ThemeVariant? themeVariant;

        if (PleasantSettings.Instance.Theme is Theme.System)
        {
            themeVariant = GetThemeFromIPlatformSettings(platformSettings);
        }
        else
        {
            themeVariant = PleasantSettings.Instance.Theme is Theme.Light ? 
                ThemeVariant.Light : ThemeVariant.Dark;
        }
        
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