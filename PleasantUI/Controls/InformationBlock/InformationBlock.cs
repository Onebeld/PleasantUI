using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace PleasantUI.Controls;

public class InformationBlock : ContentControl
{
    public static readonly StyledProperty<Geometry> IconProperty =
        AvaloniaProperty.Register<InformationBlock, Geometry>(nameof(Icon));
    
    public Geometry Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}