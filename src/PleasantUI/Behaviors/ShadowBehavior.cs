using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PleasantUI.Reactive;

namespace PleasantUI.Behaviors;

/// <summary>
/// Defines <see cref="Avalonia.Media.BoxShadow"/> behavior for the control
/// </summary>
public class ShadowBehavior : AvaloniaObject
{
    /// <summary>
    /// Defines the <see cref="GetBoxShadow" /> and <see cref="SetBoxShadow" /> methods.
    /// </summary>
    public static readonly AttachedProperty<BoxShadow?> BoxShadowProperty =
        AvaloniaProperty.RegisterAttached<ShadowBehavior, BoxShadow?>("BoxShadow", typeof(BoxShadow));

    /// <summary>
    /// Defines the <see cref="GetEnableShadowing" /> and <see cref="SetEnableShadowing" /> methods.
    /// </summary>
    public static readonly AttachedProperty<bool> EnableShadowingProperty =
        AvaloniaProperty.RegisterAttached<ShadowBehavior, bool>("EnableShadowing", typeof(bool));

    static ShadowBehavior()
    {
        BoxShadowProperty.Changed.Subscribe(x => HandleChanged(x.Sender));
        EnableShadowingProperty.Changed.Subscribe(x => HandleChanged(x.Sender));
    }

    /// <summary>
    /// Sets the box shadow of the specified <see cref="AvaloniaObject"/>.
    /// </summary>
    /// <param name="element">The AvaloniaObject.</param>
    /// <param name="boxShadow">The <see cref="BoxShadow"/> to set. Use <see langword="null"/> to remove the box shadow.</param>
    public static void SetBoxShadow(AvaloniaObject element, BoxShadow? boxShadow)
    {
        element.SetValue(BoxShadowProperty, boxShadow);
    }

    /// <summary>
    /// Retrieves the value of the BoxShadow property for the specified AvaloniaObject.
    /// </summary>
    /// <param name="element">The AvaloniaObject to get the BoxShadow value from.</param>
    /// <returns>
    /// The BoxShadow value of the specified AvaloniaObject if set; otherwise, null.
    /// </returns>
    public static BoxShadow? GetBoxShadow(AvaloniaObject element)
    {
        return element.GetValue(BoxShadowProperty);
    }

    /// <summary>
    /// Sets the value of the EnableShadowing attached property for the specified <see cref="AvaloniaObject"/>.
    /// </summary>
    /// <param name="element">The AvaloniaObject to set the value for.</param>
    /// <param name="value">The value to set.</param>
    public static void SetEnableShadowing(AvaloniaObject element, bool value)
    {
        element.SetValue(EnableShadowingProperty, value);
    }

    /// <summary>
    /// Gets the value of the EnableShadowing property for the specified <see cref="AvaloniaObject"/>.
    /// </summary>
    /// <param name="element">The AvaloniaObject to get the value from.</param>
    /// <returns>The value of the EnableShadowing property for the specified AvaloniaObject.</returns>
    public static bool GetEnableShadowing(AvaloniaObject element)
    {
        return element.GetValue(EnableShadowingProperty);
    }

    private static void HandleChanged(AvaloniaObject element)
    {
        BoxShadow? boxShadow = element.GetValue(BoxShadowProperty);
        bool enabledShadowing = element.GetValue(EnableShadowingProperty);

        if (enabledShadowing && boxShadow is not null)
            element.SetValue(Border.BoxShadowProperty, new BoxShadows(boxShadow.Value));
        else element.SetValue(Border.BoxShadowProperty, default);
    }
}