using Avalonia;
using Avalonia.Controls;

namespace PleasantUI.Controls;

/// <summary>
/// A custom border control.
/// </summary>
public class PleasantBorder : Border
{
    /// <summary>
    /// Defines the levels of background elevation.
    /// </summary>
    public enum BackgroundLeveling
    {
        /// <summary>
        /// Activates the level 1 background of the border. 
        /// </summary>
        Level1 = 1,
        
        /// <summary>
        /// Activates the level 2 background of the border. 
        /// </summary>
        Level2 = 2,
        
        /// <summary>
        /// Activates the level 3 background of the border. 
        /// </summary>
        Level3 = 3,
        
        /// <summary>
        /// Activates the level 4 background of the border. 
        /// </summary>
        Level4 = 4
    }
    
    /// <summary>
    /// Identifies the <see cref="BackgroundLevel"/> styled property.
    /// </summary>
    public static readonly StyledProperty<BackgroundLeveling> BackgroundLevelProperty =
        AvaloniaProperty.Register<PleasantBorder, BackgroundLeveling>(nameof(BackgroundLevel), BackgroundLeveling.Level1);
    
    /// <summary>
    /// Gets or sets the background leveling of the border.
    /// </summary>
    public BackgroundLeveling BackgroundLevel
    {
        get => GetValue(BackgroundLevelProperty);
        set => SetValue(BackgroundLevelProperty, value);
    }
    
    /// <inheritdoc />
    protected override Type StyleKeyOverride { get; } = typeof(PleasantBorder);
}