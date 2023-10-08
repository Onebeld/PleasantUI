using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text.Json;
using Avalonia.Collections;
using PleasantUI.Core.Constants;
using PleasantUI.Core.Enums;
using PleasantUI.Core.Settings;

namespace PleasantUI.Core;

public class PleasantSettings : ViewModelBase
{
    public static PleasantSettings Instance { get; }

    private uint _numericalAccentColor = 9426;
    private bool _preferUserAccentColor;
    private Theme _theme = Theme.System;
    private WindowSettings _windowSettings = null!;
    private RenderSettings _renderSettings = null!;
    private AvaloniaList<uint> _savedColorPalette = new();

    static PleasantSettings()
    {
        if (!Directory.Exists(PleasantDirectories.Settings))
            Directory.CreateDirectory(PleasantDirectories.Settings);

        if (File.Exists(Path.Combine(PleasantDirectories.Settings, PleasantFileNames.Settings)))
        {
            try
            {
                using FileStream fileStream = File.OpenRead(Path.Combine(PleasantDirectories.Settings, PleasantFileNames.Settings));
                Instance = JsonSerializer.Deserialize(fileStream, PleasantSettingsGenerationContext.Default.PleasantSettings)!;
                
                return;
            }
            catch
            {
                // ignored
            }
        }

        Instance = new PleasantSettings
        {
            _windowSettings = new WindowSettings(),
            _renderSettings = new RenderSettings()
        };
        
        Setup();
    }

    /// <summary>
    /// Gets or sets the color in numerical form
    /// </summary>
    [DataMember]
    public uint NumericalAccentColor
    {
        get => _numericalAccentColor;
        set => RaiseAndSet(ref _numericalAccentColor, value);
    }

    /// <summary>
    /// Gets or sets a setting that allows or disallows the use of the accent color from the system
    /// </summary>
    [DataMember]
    public bool PreferUserAccentColor
    {
        get => _preferUserAccentColor;
        set => RaiseAndSet(ref _preferUserAccentColor, value);
    }

    /// <summary>
    /// Gets or sets the theme setting
    /// </summary>
    [DataMember]
    public Theme Theme
    {
        get => _theme;
        set => RaiseAndSet(ref _theme, value);
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
            
            RaiseAndSet(ref _windowSettings, value);
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
            
            RaiseAndSet(ref _renderSettings, value);
        }
    }
    
    [DataMember]
    public AvaloniaList<uint> SavedColorPalette
    {
        get => _savedColorPalette;
        set => RaiseAndSet(ref _savedColorPalette, value);
    }

    /// <summary>
    /// Saves all library settings to its own folder
    /// </summary>
    public void Save()
    {
        using FileStream fileStream = File.Create(Path.Combine(PleasantDirectories.Settings, PleasantFileNames.Settings));
        JsonSerializer.Serialize(fileStream, Instance, PleasantSettingsGenerationContext.Default.PleasantSettings);
    }

    /// <summary>
    /// Resets all settings to defaults
    /// </summary>
    public void Reset()
    {
        Setup();

        WindowSettings = new WindowSettings();
        RenderSettings = new RenderSettings();
    }

    private static void Setup()
    {
        OperatingSystem operatingSystem = Environment.OSVersion;

        if (operatingSystem.Platform is PlatformID.Win32NT)
        {
            Version currentVersion = operatingSystem.Version;

            if (currentVersion >= new Version(10, 22000))
            {
                Instance.WindowSettings.EnableBlur = true;
                Instance.WindowSettings.EnableCustomTitleBar = true;
            }
            else if (currentVersion >= new Version(10, 0, 10586))
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
            Instance.WindowSettings.EnableBlur = true;
            Instance.WindowSettings.EnableCustomTitleBar = true;
        }
    }
}