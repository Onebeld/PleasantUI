using System.Runtime.Serialization;
using System.Text.Json;
using Avalonia.Collections;
using PleasantUI.Core.Constants;
using PleasantUI.Core.GenerationContexts;
using PleasantUI.Core.Settings;

namespace PleasantUI.Core;

/// <summary>
/// Represents the settings for the PleasantUI library. This class manages various settings related to themes, windows,
/// rendering, and accent colors.
/// </summary>
public class PleasantSettings : ViewModelBase
{
    private AvaloniaList<uint> _colorPalettes = [];
    private Guid? _customThemeId;
    private uint _numericalAccentColor;
    private bool _preferUserAccentColor;
    private RenderSettings _renderSettings = null!;
    private string _theme = "System";
    private WindowSettings _windowSettings = null!;

    /// <summary>
    /// Gets the singleton instance of the PleasantSettings class.
    /// </summary>
    public static PleasantSettings Instance { get; }

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
    
    static PleasantSettings()
    {
        
        if (!Directory.Exists(PleasantDirectories.Settings))
            Directory.CreateDirectory(PleasantDirectories.Settings);

        if (File.Exists(Path.Combine(PleasantDirectories.Settings, PleasantFileNames.Settings)))
            try
            {
                using FileStream fileStream =
                    File.OpenRead(Path.Combine(PleasantDirectories.Settings, PleasantFileNames.Settings));
                Instance = JsonSerializer.Deserialize(fileStream,
                    PleasantSettingsGenerationContext.Default.PleasantSettings)!;

                return;
            }
            catch
            {
                // ignored
            }

        Instance = new PleasantSettings
        {
            _windowSettings = new WindowSettings(),
            _renderSettings = new RenderSettings()
        };

        Setup();
    }

    /// <summary>
    /// Saves all library settings to its own folder
    /// </summary>
    public static void Save()
    {
        using FileStream fileStream =
            File.Create(Path.Combine(PleasantDirectories.Settings, PleasantFileNames.Settings));
        JsonSerializer.Serialize(fileStream, Instance,
            PleasantSettingsGenerationContext.Default.PleasantSettings);
    }

    /// <summary>
    /// Resets all settings to defaults
    /// </summary>
    public static void Reset()
    {
        Setup();

        Instance.WindowSettings = new WindowSettings();
        Instance.RenderSettings = new RenderSettings();
    }

    private static void Setup()
    {
        OperatingSystem operatingSystem = Environment.OSVersion;

        if (operatingSystem.Platform is PlatformID.Win32NT)
        {
            Version currentVersion = operatingSystem.Version;

            if (currentVersion >= new Version(10, 0, 10586))
            {
                Instance.WindowSettings.EnableBlur = true;
                Instance.WindowSettings.EnableCustomTitleBar = true;
            }
            else
            {
                Instance.WindowSettings.EnableBlur = false;
                Instance.WindowSettings.EnableCustomTitleBar = false;
            }
        }
        else if (operatingSystem.Platform is PlatformID.MacOSX)
        {
            Instance.WindowSettings.EnableBlur = true;
            Instance.WindowSettings.EnableCustomTitleBar = true;
        }
        else
        {
            Instance.WindowSettings.EnableBlur = false;
            Instance.WindowSettings.EnableCustomTitleBar = false;
        }
    }
}