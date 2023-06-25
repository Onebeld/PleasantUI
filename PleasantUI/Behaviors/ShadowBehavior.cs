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
    public static readonly AttachedProperty<BoxShadow?> BoxShadowProperty =
        AvaloniaProperty.RegisterAttached<ShadowBehavior, BoxShadow?>("BoxShadow", typeof(BoxShadow));

    public static readonly AttachedProperty<bool> EnableShadowingProperty =
        AvaloniaProperty.RegisterAttached<ShadowBehavior, bool>("EnableShadowing", typeof(bool));

    static ShadowBehavior()
    {
        BoxShadowProperty.Changed.Subscribe(x => HandleChanged(x.Sender));
        EnableShadowingProperty.Changed.Subscribe(x => HandleChanged(x.Sender));
    }

    public static void SetBoxShadow(AvaloniaObject element, BoxShadow? boxShadow)
    {
        element.SetValue(BoxShadowProperty, boxShadow);
    }

    public static BoxShadow? GetBoxShadow(AvaloniaObject element)
    {
        return element.GetValue(BoxShadowProperty);
    }

    public static void SetEnableShadowing(AvaloniaObject element, bool value)
    {
        element.SetValue(EnableShadowingProperty, value);
    }

    public static bool GetEnableShadowing(AvaloniaObject element)
    {
        return element.GetValue(EnableShadowingProperty);
    }

    private static void HandleChanged(AvaloniaObject element)
    {
        BoxShadow? boxShadow = element.GetValue(BoxShadowProperty);
        bool enabledShadowing = element.GetValue(EnableShadowingProperty);

        if (enabledShadowing && boxShadow is not null)
        {
            element.SetValue(Border.BoxShadowProperty, new BoxShadows(boxShadow.Value));
        }
        else element.SetValue(Border.BoxShadowProperty, default);
    }
}