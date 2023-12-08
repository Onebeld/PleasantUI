using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace PleasantUI.Controls;

/// <summary>
/// Represents a control that displays information with an associated icon.
/// </summary>
public class InformationBlock : ContentControl
{
    public static readonly StyledProperty<Geometry> IconProperty =
        AvaloniaProperty.Register<InformationBlock, Geometry>(nameof(Icon));
    
    /// <summary>
    /// Represents the icon associated with the count of items, indicating the number of items associated with that icon.
    /// </summary>
    public Geometry Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}