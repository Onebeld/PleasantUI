using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Xaml.Interactivity;

namespace PleasantUI.Behaviors;

/// <summary>
/// Defines <see cref="Avalonia.Media.BoxShadow"/> behavior for the control
/// </summary>
public class ShadowBehavior : Behavior<Control>
{
    public static readonly StyledProperty<BoxShadow> BoxShadowProperty =
        AvaloniaProperty.Register<ShadowBehavior, BoxShadow>(nameof(BoxShadow));

    public static readonly StyledProperty<bool> EnableShadowingProperty =
        AvaloniaProperty.Register<ShadowBehavior, bool>(nameof(EnableShadowing));

    public BoxShadow BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }

    /// <summary>
    /// Specifies whether the shadow will be shown on the control
    /// </summary>
    public bool EnableShadowing
    {
        get => GetValue(EnableShadowingProperty);
        set => SetValue(EnableShadowingProperty, value);
    }

    protected override void OnAttachedToVisualTree()
    {
        if (EnableShadowing)
            ApplyShadow();
        else AssociatedObject?.SetValue(Border.BoxShadowProperty, default);
    }

    protected override void OnDetachedFromVisualTree()
    {
        AssociatedObject?.SetValue(Border.BoxShadowProperty, default);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        
        if (AssociatedObject is null) return;

        if (EnableShadowing)
            ApplyShadow();
        else
            AssociatedObject?.SetValue(Border.BoxShadowProperty, default);
    }

    private void ApplyShadow() => AssociatedObject?.SetValue(Border.BoxShadowProperty, new BoxShadows(BoxShadow));
}