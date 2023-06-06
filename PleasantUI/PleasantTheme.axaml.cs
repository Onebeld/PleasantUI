using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using PleasantUI.Core;
using PleasantUI.Core.Enums;
using PleasantUI.Core.Exceptions;
using PleasantUI.Extensions.Media;

namespace PleasantUI;

public class PleasantTheme : Style
{
    private IPlatformSettings? _platformSettings;

    private ResourceDictionary? _accentColorsDictionary;
    
    public PleasantTheme()
    {
        AvaloniaXamlLoader.Load(this);
        
        Init();
    }

    public void UpdateTheme()
    {
        if (_platformSettings is null) return;
        
        ResolveTheme(_platformSettings);
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

        UpdateAccentColors(
            accent,
            accent.LightenPercent(light1Percent),
            accent.LightenPercent(light2Percent),
            accent.LightenPercent(light3Percent),
            accent.LightenPercent(dark1Percent),
            accent.LightenPercent(dark2Percent),
            accent.LightenPercent(dark3Percent),
            accent.InvertColor(),
            accent.InvertColor().LightenPercent(light1Percent),
            accent.InvertColor().LightenPercent(light2Percent),
            accent.InvertColor().LightenPercent(light3Percent),
            accent.InvertColor().LightenPercent(dark1Percent),
            accent.InvertColor().LightenPercent(dark2Percent),
            accent.InvertColor().LightenPercent(dark3Percent));
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

    private void UpdateAccentColors(
        Color accent,
        Color light1,
        Color light2,
        Color light3,
        Color dark1,
        Color dark2,
        Color dark3,
        Color invertAccent,
        Color invertLight1,
        Color invertLight2,
        Color invertLight3,
        Color invertDark1,
        Color invertDark2,
        Color invertDark3)
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
            { "SystemAccentDarkColor3", dark3 },
            
            { "SystemInvertAccentColor", invertAccent },
            
            { "SystemInvertAccentLightColor1", invertLight1 },
            { "SystemInvertAccentLightColor2", invertLight2 },
            { "SystemInvertAccentLightColor3", invertLight3 },
            { "SystemInvertAccentDarkColor1", invertDark1 },
            { "SystemInvertAccentDarkColor2", invertDark2 },
            { "SystemInvertAccentDarkColor3", invertDark3 },
        };
        
        Resources.MergedDictionaries.Add(_accentColorsDictionary);
    }
}