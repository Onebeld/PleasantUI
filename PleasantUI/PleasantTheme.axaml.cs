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
    private IPlatformSettings? _platformSettings;

    private ResourceDictionary? _accentColorsDictionary;
    private ResourceDictionary? _foregroundAccentColorsDictionary;
    
    public PleasantTheme()
    {
        AvaloniaXamlLoader.Load(this);
        
        Init();
    }

    public void UpdateAccentColors(Color accent)
    {
        if (_platformSettings is null) return;
        
        float light1Percent;
        float light2Percent;
        float light3Percent;
        float dark1Percent;
        float dark2Percent;
        float dark3Percent;

        PlatformThemeVariant platformThemeVariant = _platformSettings.GetColorValues().ThemeVariant;

        if (PleasantSettings.Instance.Theme is Theme.Dark 
            || PleasantSettings.Instance.Theme is Theme.System && platformThemeVariant is PlatformThemeVariant.Dark)
        {
            dark3Percent = -0.35f;
            dark2Percent = -0.20f;
            dark1Percent = -0.05f;
            // 0
            light1Percent = 0.25f;
            light2Percent = 0.40f;
            light3Percent = 0.55f;
        }
        else
        {
            dark3Percent = -0.35f;
            dark2Percent = -0.20f;
            dark1Percent = -0.05f;
            // 0
            light1Percent = 0.15f;
            light2Percent = 0.30f;
            light3Percent = 0.45f;
        }

        Color accentLight1 = accent.LightenPercent(light1Percent);
        Color accentLight2 = accent.LightenPercent(light2Percent);
        Color accentLight3 = accent.LightenPercent(light3Percent);
        Color accentDark1 = accent.LightenPercent(dark1Percent);
        Color accentDark2 = accent.LightenPercent(dark2Percent);
        Color accentDark3 = accent.LightenPercent(dark3Percent);

        UpdateAccentColors(
            accent,
            accentLight1,
            accentLight2,
            accentLight3,
            accentDark1,
            accentDark2,
            accentDark3);
        
        UpdateForegroundAccentColors(
                GetForegroundFromAccent(accent),
                GetForegroundFromAccent(accentLight1),
                GetForegroundFromAccent(accentLight2),
                GetForegroundFromAccent(accentLight3),
                GetForegroundFromAccent(accentDark1),
                GetForegroundFromAccent(accentDark2),
                GetForegroundFromAccent(accentDark3));
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

        Color color = Color.FromUInt32(PleasantSettings.Instance.NumericalAccentColor);
        UpdateAccentColors(color);
    }

    private void PlatformSettingsOnColorValuesChanged(object sender, PlatformColorValues e)
    {
        if (PleasantSettings.Instance.Theme is Theme.System && Application.Current is not null)
        {
            ThemeVariant themeVariant = e.ThemeVariant is PlatformThemeVariant.Light ?
                ThemeVariant.Light : ThemeVariant.Dark;

            Application.Current.RequestedThemeVariant = themeVariant;
        }

        if (!PleasantSettings.Instance.PreferUserAccentColor)
        {
            Color color = e.AccentColor1;

            PleasantSettings.Instance.NumericalAccentColor = color.ToUInt32();
            
            UpdateAccentColors(color);
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

    private void UpdateAccentColors(
        Color accent,
        Color light1,
        Color light2,
        Color light3,
        Color dark1,
        Color dark2,
        Color dark3)
    {
        if (_accentColorsDictionary is not null)
            Resources.MergedDictionaries.Remove(_accentColorsDictionary);

        _accentColorsDictionary = new ResourceDictionary
        {
            { "SystemAccentColor", accent },
            
            { "SystemAccentLightColor1", light1 },
            { "SystemAccentLightColor2", light2 },
            { "SystemAccentLightColor3", light3 },
            { "SystemAccentDarkColor1", dark1 },
            { "SystemAccentDarkColor2", dark2 },
            { "SystemAccentDarkColor3", dark3 }
        };
        
        Resources.MergedDictionaries.Add(_accentColorsDictionary);
    }
    
    private void UpdateForegroundAccentColors(
        Color accent,
        Color light1,
        Color light2,
        Color light3,
        Color dark1,
        Color dark2,
        Color dark3)
    {
        if (_foregroundAccentColorsDictionary is not null)
            Resources.MergedDictionaries.Remove(_foregroundAccentColorsDictionary);

        _foregroundAccentColorsDictionary = new ResourceDictionary
        {
            { "ForegroundAccentColor", accent },
            
            { "ForegroundAccentLightColor1", light1 },
            { "ForegroundAccentLightColor2", light2 },
            { "ForegroundAccentLightColor3", light3 },
            { "ForegroundAccentDarkColor1", dark1 },
            { "ForegroundAccentDarkColor2", dark2 },
            { "ForegroundAccentDarkColor3", dark3 }
        };
        
        Resources.MergedDictionaries.Add(_foregroundAccentColorsDictionary);
    }
}