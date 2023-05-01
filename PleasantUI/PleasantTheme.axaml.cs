using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using PleasantUI.Core;
using PleasantUI.Core.Enums;
using PleasantUI.Extensions.Media;

#pragma warning disable CS0618

namespace PleasantUI;

public class PleasantTheme : Style
{
    private readonly IPlatformSettings? _platformSettings;

    private ResourceDictionary? _accentColorsDictionary;
    
    public PleasantTheme()
    {
        _platformSettings = AvaloniaLocator.Current.GetService<IPlatformSettings>();
        
        Init();
    }

    public void UpdateTheme()
    {
        if (_platformSettings is null) return;
        
        ResolveTheme(_platformSettings);
    }
    
    public void UpdateAccentColors(Color accent)
    {
        UpdateAccentColors(
            accent,
            accent.LightenPercent(0.15f),
            accent.LightenPercent(0.30f),
            accent.LightenPercent(0.45f),
            accent.LightenPercent(-0.15f),
            accent.LightenPercent(-0.30f),
            accent.LightenPercent(-0.45f),
            accent.InvertColor(),
            accent.InvertColor().LightenPercent(0.15f),
            accent.InvertColor().LightenPercent(0.30f),
            accent.InvertColor().LightenPercent(0.45f),
            accent.InvertColor().LightenPercent(-0.15f),
            accent.InvertColor().LightenPercent(-0.30f),
            accent.InvertColor().LightenPercent(-0.45f));
    }

    private void Init()
    {
        AvaloniaXamlLoader.Load(this);

        AvaloniaLocator.CurrentMutable.Bind<PleasantTheme>().ToConstant(this);
        
        IPlatformSettings? platformSettings = AvaloniaLocator.Current.GetService<IPlatformSettings>();
        
        if (platformSettings is null) return;
        
        platformSettings.ColorValuesChanged += PlatformSettingsOnColorValuesChanged;

        ResolveTheme(platformSettings);
        ResolveAccentColor(platformSettings);
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
            PleasantSettings.Instance.NumericalAccentColor = platformSettings.GetColorValues().AccentColor1.ToUint32();

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

            PleasantSettings.Instance.NumericalAccentColor = color.ToUint32();
            
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
            { "SystemAccentColorLight1", light1 },
            { "SystemAccentColorLight2", light2 },
            { "SystemAccentColorLight3", light3 },
            { "SystemAccentColorDark1", dark1 },
            { "SystemAccentColorDark2", dark2 },
            { "SystemAccentColorDark3", dark3 },
            
            { "SystemInvertAccentColor", invertAccent },
            { "SystemInvertAccentLight1", invertLight1 },
            { "SystemInvertAccentLight2", invertLight2 },
            { "SystemInvertAccentLight3", invertLight3 },
            { "SystemInvertAccentDark1", invertDark1 },
            { "SystemInvertAccentDark2", invertDark2 },
            { "SystemInvertAccentDark3", invertDark3 }
        };
        
        Resources.MergedDictionaries.Add(_accentColorsDictionary);
    }
}