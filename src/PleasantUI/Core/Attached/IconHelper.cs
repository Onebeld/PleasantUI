using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace PleasantUI.Core.Attached;

public class IconHelper : AvaloniaObject
{
    public static readonly AttachedProperty<double> WidthProperty =
        AvaloniaProperty.RegisterAttached<IconHelper, Control, double>("Width", defaultBindingMode: BindingMode.TwoWay);
    
    public static readonly AttachedProperty<double> HeightProperty =
        AvaloniaProperty.RegisterAttached<IconHelper, Control, double>("Height", defaultBindingMode: BindingMode.TwoWay);
    
    public static readonly AttachedProperty<double> FontSizeProperty =
        AvaloniaProperty.RegisterAttached<IconHelper, Control, double>("FontSize", 1, defaultBindingMode: BindingMode.TwoWay);

    public static double GetWidth(AvaloniaObject obj) => obj.GetValue(WidthProperty);

    public static void SetWidth(AvaloniaObject obj, double value) => obj.SetValue(WidthProperty, value);

    public static double GetHeight(AvaloniaObject obj) => obj.GetValue(HeightProperty);

    public static void SetHeight(AvaloniaObject obj, double value) => obj.SetValue(HeightProperty, value);
    
    public static double GetFontSize(AvaloniaObject obj) =>  obj.GetValue(FontSizeProperty);
    
    public static void SetFontSize(AvaloniaObject obj, double value) => obj.SetValue(FontSizeProperty, value);
}