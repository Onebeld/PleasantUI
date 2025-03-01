using System.Runtime.Serialization;
using Avalonia.Collections;
using PleasantUI.Core.Settings;

namespace PleasantUI.Core;

/// <summary>
/// Represents the settings for the PleasantUI library. This class manages various settings related to themes, windows,
/// rendering, and accent colors.
/// </summary>
public class PleasantSettings : SettingsBase<PleasantSettings>
{
    private AvaloniaList<uint> _colorPalettes = [];

    private uint _numericalAccentColor;
    private bool _preferUserAccentColor;

    private string _theme = "System";
    private Guid? _customThemeId;

    private RenderSettings _renderSettings;
    private WindowSettings _windowSettings;

    /// <summary>
    /// Gets the singleton instance of the PleasantSettings class.
    /// </summary>
    public static PleasantSettings? Current { get; set; }

    /// <summary>
    /// Gets or sets the color in numerical form
    /// </summary>
    [DataMember]
    public uint NumericalAccentColor
    {
        get => _numericalAccentColor;
        set => SetProperty(ref _numericalAccentColor, value);
    }

    /// <summary>
    /// Gets or sets a setting that allows or disallows the use of the accent color from the system
    /// </summary>
    [DataMember]
    public bool PreferUserAccentColor
    {
        get => _preferUserAccentColor;
        set => SetProperty(ref _preferUserAccentColor, value);
    }

    /// <summary>
    /// Gets or sets the theme setting
    /// </summary>
    [DataMember]
    public string Theme
    {
        get => _theme;
        set => SetProperty(ref _theme, value);
    }

    /// <summary>
    /// Gets or sets the ID of the custom theme.
    /// </summary>
    [DataMember]
    public Guid? CustomThemeId
    {
        get => _customThemeId;
        set => SetProperty(ref _customThemeId, value);
    }

    /// <summary>
    /// Gets settings for all windows
    /// </summary>
    [DataMember]
    public WindowSettings WindowSettings
    {
        get => _windowSettings;
        set
        {
            if (value is null)
                throw new NullReferenceException("WindowSettings is null");

            SetProperty(ref _windowSettings, value);
        }
    }

    /// <summary>
    /// Gets the rendering settings
    /// </summary>
    [DataMember]
    public RenderSettings RenderSettings
    {
        get => _renderSettings;
        set
        {
            if (value is null)
                throw new NullReferenceException("RenderSettings is null");

            SetProperty(ref _renderSettings, value);
        }
    }

    /// <summary>
    /// Gets or sets the list of color palettes.
    /// </summary>
    [DataMember]
    public AvaloniaList<uint> ColorPalettes
    {
        get => _colorPalettes;
        set => SetProperty(ref _colorPalettes, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PleasantSettings"/> class 
    /// with default window and render settings.
    /// </summary>
    public PleasantSettings()
    {
        _windowSettings = new WindowSettings();
        _renderSettings = new RenderSettings();
    }

    /*private static void Setup()
    {
        OperatingSystem operatingSystem = Environment.OSVersion;

        if (operatingSystem.Platform is PlatformID.Win32NT)
        {
            Version currentVersion = operatingSystem.Version;

            if (currentVersion >= new Version(10, 0, 10586))
            {
                Current.WindowSettings.EnableBlur = true;
                Current.WindowSettings.EnableCustomTitleBar = true;
            }
            else
            {
                Current.WindowSettings.EnableBlur = false;
                Current.WindowSettings.EnableCustomTitleBar = false;
            }
        }
        else if (operatingSystem.Platform is PlatformID.MacOSX)
        {
            Current.WindowSettings.EnableBlur = true;
            Current.WindowSettings.EnableCustomTitleBar = true;
        }
        else
        {
            Current.WindowSettings.EnableBlur = false;
            Current.WindowSettings.EnableCustomTitleBar = false;
        }
    }*/
}