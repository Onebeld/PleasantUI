using System.Numerics;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Reactive;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace PleasantUI.Core.Attached;

public static class CompositionAnimationBehavior
{
    // =========================
    // Attached Properties
    // =========================

    public static readonly AttachedProperty<double> DurationProperty =
        AvaloniaProperty.RegisterAttached<Visual, double>(
            "Duration",
            typeof(CompositionAnimationBehavior),
            200);

    public static readonly AttachedProperty<float> InitialOpacityProperty =
        AvaloniaProperty.RegisterAttached<Visual, float>(
            "InitialOpacity",
            typeof(CompositionAnimationBehavior),
            0f);

    public static readonly AttachedProperty<float> InitialScaleProperty =
        AvaloniaProperty.RegisterAttached<Visual, float>(
            "InitialScale",
            typeof(CompositionAnimationBehavior),
            0.8f);
    
    
    public static readonly AttachedProperty<bool> UseEntranceAnimationsProperty =
        AvaloniaProperty.RegisterAttached<Visual, bool>(
            "UseEntranceAnimations",
            typeof(CompositionAnimationBehavior),
            false);
    public static readonly AttachedProperty<bool> UseImplicitAnimationsProperty =
        AvaloniaProperty.RegisterAttached<Visual, bool>(
            "UseImplicitAnimations",
            typeof(CompositionAnimationBehavior),
            false);
    
    public static readonly AttachedProperty<bool> UseImplicitAnimationsChildrenProperty =
        AvaloniaProperty.RegisterAttached<Visual, bool>(
            "UseImplicitChildrenAnimations",
            typeof(CompositionAnimationBehavior),
            false);

    // =========================
    // Get / Set
    // =========================

    public static double GetDuration(AvaloniaObject o) => o.GetValue(DurationProperty);
    public static void SetDuration(AvaloniaObject o, double v) => o.SetValue(DurationProperty, v);

    public static float GetInitialOpacity(AvaloniaObject o) => o.GetValue(InitialOpacityProperty);
    public static void SetInitialOpacity(AvaloniaObject o, float v) => o.SetValue(InitialOpacityProperty, v);

    public static float GetInitialScale(AvaloniaObject o) => o.GetValue(InitialScaleProperty);
    public static void SetInitialScale(AvaloniaObject o, float v) => o.SetValue(InitialScaleProperty, v);

    public static bool GetUseImplicitAnimations(AvaloniaObject o) => o.GetValue(UseImplicitAnimationsProperty);
    public static void SetUseImplicitAnimations(AvaloniaObject o, bool v) => o.SetValue(UseImplicitAnimationsProperty, v);

    public static bool GetUseEntranceAnimations(AvaloniaObject o) => o.GetValue(UseEntranceAnimationsProperty);
    public static void SetUseEntranceAnimations(AvaloniaObject o, bool v) => o.SetValue(UseEntranceAnimationsProperty, v);

    public static bool GetUseImplicitChildrenAnimations(AvaloniaObject o) => o.GetValue(UseImplicitAnimationsChildrenProperty);
    public static void SetUseImplicitChildrenAnimations(AvaloniaObject o, bool v) => o.SetValue(UseImplicitAnimationsChildrenProperty, v);

    
    // =========================
    // Init
    // =========================

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
        {
            v.AttachedToVisualTree += VOnAttachedToVisualTree;
        }
        else
        {
            v.AttachedToVisualTree -= VOnAttachedToVisualTree;
        }
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

    // =========================
    // Main Entry
    // =========================

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

        void BoundsChanged(object? _, AvaloniaPropertyChangedEventArgs __)
        {
            if (visual.Bounds.Width <= 0 || visual.Bounds.Height <= 0)
                return;

            visual.PropertyChanged -= BoundsChanged;
            StartEntranceAnimation(visual);
        }

        visual.PropertyChanged += BoundsChanged;
    }

    private static void InitializeImplicitAnimation(Visual visual)
    {
        Dispatcher.UIThread.Post(() => StartImplicitAnimation(visual), DispatcherPriority.Render);
    }

    private static void StartEntranceAnimation(Visual visual)
    {
        var composition = ElementComposition.GetElementVisual(visual);
        var compositor = composition.Compositor;

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
        composition.Offset = composition.Offset;
        
        Compositor compositor = composition.Compositor;
        
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