using System.Numerics;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Reactive;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace PleasantUI.Core.Attached;

/// <summary>
/// A static attached property class for managing composition rendering animations
/// </summary>
public static class CompositionAnimationBehavior
{
    /// <summary>
    /// Defines the Duration attached property.
    /// </summary>
    public static readonly AttachedProperty<double> DurationProperty =
        AvaloniaProperty.RegisterAttached<Visual, double>("Duration", typeof(CompositionAnimationBehavior), 200);

    /// <summary>
    /// Defines the InitialOpacity attached property.
    /// </summary>
    public static readonly AttachedProperty<float> InitialOpacityProperty =
        AvaloniaProperty.RegisterAttached<Visual, float>("InitialOpacity", typeof(CompositionAnimationBehavior));

    /// <summary>
    /// Defines the InitialScale attached property.
    /// </summary>
    public static readonly AttachedProperty<float> InitialScaleProperty =
        AvaloniaProperty.RegisterAttached<Visual, float>("InitialScale", typeof(CompositionAnimationBehavior), 0.8f);
    
    /// <summary>
    /// Defines the UseEntranceAnimations attached property.
    /// </summary>
    public static readonly AttachedProperty<bool> UseEntranceAnimationsProperty =
        AvaloniaProperty.RegisterAttached<Visual, bool>("UseEntranceAnimations", typeof(CompositionAnimationBehavior));
    
    /// <summary>
    /// Defines the UseImplicitAnimations attached property.
    /// </summary>
    public static readonly AttachedProperty<bool> UseImplicitAnimationsProperty =
        AvaloniaProperty.RegisterAttached<Visual, bool>("UseImplicitAnimations", typeof(CompositionAnimationBehavior));
    
    /// <summary>
    /// Defines the UseImplicitChildrenAnimations attached property.
    /// </summary>
    public static readonly AttachedProperty<bool> UseImplicitAnimationsChildrenProperty =
        AvaloniaProperty.RegisterAttached<Visual, bool>("UseImplicitChildrenAnimations", typeof(CompositionAnimationBehavior));

    /// <summary>
    /// Gets the duration of the animation
    /// </summary>
    /// <param name="o">Control</param>
    /// <returns>Duration of the animation</returns>
    public static double GetDuration(AvaloniaObject o) => o.GetValue(DurationProperty);
    
    /// <summary>
    /// Sets the duration of the animation
    /// </summary>
    /// <param name="o">Control</param>
    /// <param name="v">Duration of the animation</param>
    public static void SetDuration(AvaloniaObject o, double v) => o.SetValue(DurationProperty, v);

    /// <summary>
    /// Gets the initial opacity value of the animation
    /// </summary>
    /// <param name="o">Control</param>
    /// <returns>Initial opacity value</returns>
    public static float GetInitialOpacity(AvaloniaObject o) => o.GetValue(InitialOpacityProperty);

    /// <summary>
    /// Sets the initial opacity value of the animation
    /// </summary>
    /// <param name="o">Control</param>
    /// <param name="v">Initial opacity value</param>
    public static void SetInitialOpacity(AvaloniaObject o, float v) => o.SetValue(InitialOpacityProperty, v);

    /// <summary>
    /// Gets the initial scale value of the animation
    /// </summary>
    /// <param name="o">Control</param>
    /// <returns>Initial scale value</returns>
    public static float GetInitialScale(AvaloniaObject o) => o.GetValue(InitialScaleProperty);

    /// <summary>
    /// Sets the initial scale value of the animation
    /// </summary>
    /// <param name="o">Control</param>
    /// <param name="v">Initial scale value</param>
    public static void SetInitialScale(AvaloniaObject o, float v) => o.SetValue(InitialScaleProperty, v);

    /// <summary>
    /// Gets whether to use implicit animation
    /// </summary>
    /// <param name="o">Control</param>
    /// <returns>Whether to use implicit animation</returns>
    public static bool GetUseImplicitAnimations(AvaloniaObject o) => o.GetValue(UseImplicitAnimationsProperty);

    /// <summary>
    /// Sets whether to use implicit animation
    /// </summary>
    /// <param name="o">Control</param>
    /// <param name="v">Whether to use implicit animation</param>
    public static void SetUseImplicitAnimations(AvaloniaObject o, bool v) => o.SetValue(UseImplicitAnimationsProperty, v);

    /// <summary>
    /// Sets whether to use entrance animation
    /// </summary>
    /// <param name="o">Control</param>
    /// <returns>Whether to use entrance animation</returns>
    public static bool GetUseEntranceAnimations(AvaloniaObject o) => o.GetValue(UseEntranceAnimationsProperty);

    /// <summary>
    /// Sets whether to use entrance animation
    /// </summary>
    /// <param name="o">Control</param>
    /// <param name="v">Whether to use entrance animation</param>
    public static void SetUseEntranceAnimations(AvaloniaObject o, bool v) => o.SetValue(UseEntranceAnimationsProperty, v);

    /// <summary>
    /// Gets whether to use implicit animations for child controls of parent control
    /// </summary>
    /// <param name="o">Parent control</param>
    /// <returns>Whether to use implicit animations for child controls</returns>
    public static bool GetUseImplicitChildrenAnimations(AvaloniaObject o) => o.GetValue(UseImplicitAnimationsChildrenProperty);
    
    /// <summary>
    /// Sets whether to use implicit animations for child controls of parent control
    /// </summary>
    /// <param name="o">Parent control</param>
    /// <param name="v">Whether to use implicit animations for child controls</param>
    public static void SetUseImplicitChildrenAnimations(AvaloniaObject o, bool v) => o.SetValue(UseImplicitAnimationsChildrenProperty, v);

    static CompositionAnimationBehavior()
    {
        UseEntranceAnimationsProperty.Changed.Subscribe(
            new AnonymousObserver<AvaloniaPropertyChangedEventArgs<bool>>(OnNext));

        UseImplicitAnimationsProperty.Changed.Subscribe(
            new AnonymousObserver<AvaloniaPropertyChangedEventArgs<bool>>(OnNext));
        
        UseImplicitAnimationsChildrenProperty.Changed.Subscribe(
            new AnonymousObserver<AvaloniaPropertyChangedEventArgs<bool>>(OnNextChildren));
    }

    private static void OnNextChildren(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (e.Sender is not Visual v)
            return;

        if (e.NewValue.Value)
            v.AttachedToVisualTree += VOnAttachedToVisualTree;
        else
            v.AttachedToVisualTree -= VOnAttachedToVisualTree;
    }

    private static void VOnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is not Visual visual)
            return;

        foreach (Visual child in visual.GetVisualChildren())
            InitializeImplicitAnimation(child);
    }

    private static void OnNext(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (e.Sender is not Visual v)
            return;

        if (GetUseEntranceAnimations(v) || GetUseImplicitAnimations(v))
            v.AttachedToVisualTree += OnAttached;
        else
            v.AttachedToVisualTree -= OnAttached;
    }

    private static void OnAttached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is not Visual visual)
            return;
        
        if (GetUseEntranceAnimations(visual))
            InitializeEntranceAnimation(visual);
        if (GetUseImplicitAnimations(visual))
            InitializeImplicitAnimation(visual);
    }

    private static void InitializeEntranceAnimation(Visual visual)
    {
        if (visual.Bounds is { Width: > 0, Height: > 0 })
        {
            StartEntranceAnimation(visual);
            return;
        }

        visual.PropertyChanged += BoundsChanged;
        return;

        void BoundsChanged(object? _, AvaloniaPropertyChangedEventArgs __)
        {
            if (visual.Bounds.Width <= 0 || visual.Bounds.Height <= 0)
                return;

            visual.PropertyChanged -= BoundsChanged;
            StartEntranceAnimation(visual);
        }
    }

    private static void InitializeImplicitAnimation(Visual visual)
    {
        // We skip one frame so that the rendering gets the Offset value for the control we want to animate,
        // otherwise the control will move to the zero point first
        Dispatcher.UIThread.Post(() => StartImplicitAnimation(visual), DispatcherPriority.Render);
    }

    private static void StartEntranceAnimation(Visual visual)
    {
        CompositionVisual? composition = ElementComposition.GetElementVisual(visual);
        Compositor? compositor = composition?.Compositor;

        if (compositor == null || composition == null)
            return;
        
        double durationMs = GetDuration(visual);
        float initialOpacity = GetInitialOpacity(visual);
        float initialScale = GetInitialScale(visual);

        composition.CenterPoint = new Vector3(
            (float)visual.Bounds.Width / 2f,
            (float)visual.Bounds.Height / 2f,
            0);
        
        ScalarKeyFrameAnimation opacityAnim = compositor.CreateScalarKeyFrameAnimation();
        opacityAnim.Duration = TimeSpan.FromMilliseconds(durationMs);
        opacityAnim.InsertKeyFrame(0f, initialOpacity);
        opacityAnim.InsertKeyFrame(1f, 1f, new CubicEaseOut());

        Vector3KeyFrameAnimation scaleAnim = compositor.CreateVector3KeyFrameAnimation();
        scaleAnim.Duration = TimeSpan.FromMilliseconds(durationMs);
        scaleAnim.InsertKeyFrame(0f, new Vector3(initialScale, initialScale, 1));
        scaleAnim.InsertKeyFrame(1f, new Vector3(1, 1, 1), new CubicEaseOut());

        composition.StartAnimation("Opacity", opacityAnim);
        composition.StartAnimation("Scale", scaleAnim);
    }

    private static void StartImplicitAnimation(Visual visual)
    {
        CompositionVisual? composition = ElementComposition.GetElementVisual(visual);
        Compositor? compositor = composition?.Compositor;

        if (compositor == null || composition == null)
            return;
        
        double durationMs = GetDuration(visual);
        
        Vector3KeyFrameAnimation offset = compositor.CreateVector3KeyFrameAnimation();
        offset.Duration = TimeSpan.FromMilliseconds(durationMs);
        offset.Target = "Offset";
        offset.InsertExpressionKeyFrame(1f, "this.FinalValue", new CubicEaseOut());

        ImplicitAnimationCollection implicitAnimations = compositor.CreateImplicitAnimationCollection();
        implicitAnimations["Offset"] = offset;
        composition.ImplicitAnimations = implicitAnimations;
    }
}