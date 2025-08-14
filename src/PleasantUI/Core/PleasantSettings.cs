using System.Runtime.Serialization;
using Avalonia.Collections;
using PleasantUI.Core.Settings;

namespace PleasantUI.Core;

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