using Avalonia.Media;
using CommunityToolkit.Mvvm.Messaging.Messages;
using PleasantUI.Core.Models;

namespace PleasantUI.ToolKit.Messages;

/// <summary>
/// Represents a message that is sent when a <see cref="ThemeColor" /> has changed.
/// </summary>
public class ColorChangedMessage : ValueChangedMessage<Color>
{
    /// <summary>
    /// Gets the <see cref="ThemeColor" /> that has changed.
    /// </summary>
    public ThemeColor ThemeColor { get; }
    
    /// <summary>
    /// Gets the previous color value.
    /// </summary>
    public Color PreviousValue { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ColorChangedMessage" /> class.
    /// </summary>
    /// <param name="value">The new color value.</param>
    /// <param name="themeColor">The <see cref="ThemeColor" /> that has changed.</param>
    /// <param name="previousValue">The previous color value.</param>
    public ColorChangedMessage(Color value, ThemeColor themeColor, Color previousValue) : base(value)
    {
        ThemeColor = themeColor;
        PreviousValue = previousValue;
    }
}