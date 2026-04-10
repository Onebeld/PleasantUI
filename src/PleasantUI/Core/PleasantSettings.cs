using System.Reflection;
using System.Runtime.Serialization;
using Avalonia.Collections;
using PleasantUI.Core.Settings;

namespace PleasantUI.Core;

/// <summary>
/// Represents the type of PleasantUI version.
/// </summary>
public enum PleasantVersionType
{
    /// <summary>
    /// Stable release (e.g., 5.2.1)
    /// </summary>
    Stable,
    
    /// <summary>
    /// Bug fix release (e.g., 5.2.1-fix)
    /// </summary>
    BugFix,
    
    /// <summary>
    /// Alpha pre-release (e.g., 5.2.1-alpha)
    /// </summary>
    Alpha,
    
    /// <summary>
    /// Beta pre-release (e.g., 5.2.1-beta)
    /// </summary>
    Beta,
    
    /// <summary>
    /// Release candidate (e.g., 5.2.1-rc)
    /// </summary>
    ReleaseCandidate,
    
    /// <summary>
    /// Canary build (e.g., 5.2.1-canary-20260410-161942)
    /// </summary>
    Canary
}

/// <summary>
/// Represents the settings for the PleasantUI library. This class manages various settings related to themes, windows
/// and accent colors.
/// </summary>
public class PleasantSettings : ViewModelBase
{
    private AvaloniaList<uint> _colorPalettes = [];

    private uint _numericalAccentColor;
    private bool _preferUserAccentColor;

    private string _theme = "System";
    private Guid? _customThemeId;
    private string _language = "en";

    private WindowSettings _windowSettings;

    /// <summary>
    /// Gets the singleton instance of the PleasantSettings class.
    /// </summary>
    public static PleasantSettings? Current { get; set; }

    /// <summary>
    /// Gets the version string from the assembly (numeric format).
    /// </summary>
    public static string Version => 
        Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";

    /// <summary>
    /// Gets the informational version from the assembly (includes pre-release tags).
    /// Uses InformationalVersion which is set from PackageVersion in Package.props.
    /// </summary>
    public static string InformationalVersion =>
        Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? Version;

    /// <summary>
    /// Gets the type of the current PleasantUI version based on the version string.
    /// </summary>
    public static PleasantVersionType VersionType
    {
        get
        {
            string version = InformationalVersion;
            
            if (version.Contains("-canary-"))
                return PleasantVersionType.Canary;
            
            if (version.EndsWith("-fix"))
                return PleasantVersionType.BugFix;
            
            if (version.Contains("-alpha"))
                return PleasantVersionType.Alpha;
            
            if (version.Contains("-beta"))
                return PleasantVersionType.Beta;
            
            if (version.Contains("-rc"))
                return PleasantVersionType.ReleaseCandidate;
            
            // If it contains a hyphen but none of the above, treat as generic pre-release
            if (version.Contains("-"))
                return PleasantVersionType.Alpha;
            
            return PleasantVersionType.Stable;
        }
    }

    /// <summary>
    /// Gets a display-friendly description of the version type.
    /// </summary>
    public static string VersionTypeDescription => VersionType switch
    {
        PleasantVersionType.Stable => "Stable Release",
        PleasantVersionType.BugFix => "Bug Fix Release",
        PleasantVersionType.Alpha => "Alpha Pre-Release",
        PleasantVersionType.Beta => "Beta Pre-Release",
        PleasantVersionType.ReleaseCandidate => "Release Candidate",
        PleasantVersionType.Canary => "Canary Build",
        _ => "Unknown"
    };

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
    /// Gets or sets the current language code (e.g., "en", "ru").
    /// </summary>
    [DataMember]
    public string Language
    {
        get => _language;
        set => SetProperty(ref _language, value);
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
    }
}