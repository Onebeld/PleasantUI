using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace PleasantUI.Core.Attached;

/// <summary>
/// A container class for attached properties for working with icons
/// </summary>
public abstract class IconHelper : AvaloniaObject
{
    /// <summary>
    /// Defines the Width attached property.
    /// </summary>
    public static readonly AttachedProperty<double> WidthProperty =
        AvaloniaProperty.RegisterAttached<IconHelper, Control, double>("Width", defaultBindingMode: BindingMode.TwoWay, inherits: true, defaultValue: double.NaN);
    
    /// <summary>
    /// Defines the Height attached property.
    /// </summary>
    public static readonly AttachedProperty<double> HeightProperty =
        AvaloniaProperty.RegisterAttached<IconHelper, Control, double>("Height", defaultBindingMode: BindingMode.TwoWay, inherits: true, defaultValue: double.NaN);
    
    /// <summary>
    /// Defines the FontSize attached property.
    /// </summary>
    public static readonly AttachedProperty<double> FontSizeProperty =
        AvaloniaProperty.RegisterAttached<IconHelper, Control, double>("FontSize", 1, defaultBindingMode: BindingMode.TwoWay, inherits: true);
    
    /// <summary>
    /// Defines the IconSize attached property.
    /// </summary>
    public static readonly AttachedProperty<double> IconSizeProperty =
        AvaloniaProperty.RegisterAttached<IconHelper, Control, double>("IconSize", 16, defaultBindingMode: BindingMode.TwoWay, inherits: true);
    
    static IconHelper() { }

    /// <summary>
    /// Gets the width of the icon
    /// </summary>
    /// <param name="obj"> The current instance of the container class</param>
    /// <returns>Icon width</returns>
    public static double GetWidth(AvaloniaObject obj) => obj.GetValue(WidthProperty);

    /// <summary>
    /// Sets the width of the icon
    /// </summary>
    /// <param name="obj"> The current instance of the container class</param>
    /// <param name="value">Icon width</param>
    public static void SetWidth(AvaloniaObject obj, double value) => obj.SetValue(WidthProperty, value);

    /// <summary>
    /// Gets the height of the icon
    /// </summary>
    /// <param name="obj"> The current instance of the container class</param>
    /// <returns>Icon height</returns>
    public static double GetHeight(AvaloniaObject obj) => obj.GetValue(HeightProperty);

    /// <summary>
    /// Sets the height of the icon
    /// </summary>
    /// <param name="obj"> The current instance of the container class</param>
    /// <param name="value">Icon height</param>
    public static void SetHeight(AvaloniaObject obj, double value) => obj.SetValue(HeightProperty, value);
    
    /// <summary>
    /// Gets the icon font size for a converted <see cref="PathIcon"/>
    /// </summary>
    /// <param name="obj">The current instance of the container class</param>
    /// <returns></returns>
    public static double GetFontSize(AvaloniaObject obj) =>  obj.GetValue(FontSizeProperty);
    
    /// <summary>
    /// Sets the icon font size for a converted <see cref="PathIcon"/>
    /// </summary>
    /// <param name="obj">The current instance of the container class</param>
    /// <param name="value">Icon font size</param>
    public static void SetFontSize(AvaloniaObject obj, double value) => obj.SetValue(FontSizeProperty, value);
    
    /// <summary>
    /// Gets the icon font size for a converted <see cref="PathIcon"/>
    /// </summary>
    /// <param name="obj">The current instance of the container class</param>
    /// <returns></returns>
    public static double GetIconSize(AvaloniaObject obj) =>  obj.GetValue(IconSizeProperty);
    
    /// <summary>
    /// Sets the icon size for a converted <see cref="PathIcon"/>
    /// </summary>
    /// <param name="obj">The current instance of the container class</param>
    /// <param name="value">Icon size</param>
    public static void SetIconSize(AvaloniaObject obj, double value) => obj.SetValue(IconSizeProperty, value);
}