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
    private WindowSettings _windowSettings;

    /// <summary>
    /// Gets the singleton instance of the PleasantSettings class.
    /// </summary>
    public static PleasantSettings? Current { get; set; }

    /// <summary>
    /// Gets or sets whether effects that make the interface more visually appealing are enabled.
    /// </summary>
    [DataMember]
    public bool EnableEffects
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    /// <summary>
    /// Gets or sets the color in numerical form
    /// </summary>
    [DataMember]
    public uint NumericalAccentColor
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Gets or sets a setting that allows or disallows the use of the accent color from the system
    /// </summary>
    [DataMember]
    public bool PreferUserAccentColor
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Gets or sets the theme setting
    /// </summary>
    [DataMember]
    public string Theme
    {
        get;
        set => SetProperty(ref field, value);
    } = "System";

    /// <summary>
    /// Gets or sets the ID of the custom theme.
    /// </summary>
    [DataMember]
    public Guid? CustomThemeId
    {
        get;
        set => SetProperty(ref field, value);
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
        get;
        set => SetProperty(ref field, value);
    } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="PleasantSettings"/> class 
    /// with default window and render settings.
    /// </summary>
    public PleasantSettings()
    {
        _windowSettings = new WindowSettings();
    }
}