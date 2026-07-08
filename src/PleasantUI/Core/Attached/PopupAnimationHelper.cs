using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.LogicalTree;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PleasantUI.Core.Attached;

internal static class PopupAnimationHelper
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<object, Popup, bool>("IsEnabled");

    public static readonly AttachedProperty<PopupPlacementAnimationCollection?> AnimationsProperty =
        AvaloniaProperty.RegisterAttached<object, Popup, PopupPlacementAnimationCollection?>("Animations");

    static PopupAnimationHelper()
    {
        Popup.IsOpenProperty.Changed.AddClassHandler<Popup, bool>(OnIsOpenChanged);
    }

    public static bool GetIsEnabled(Popup element) => element.GetValue(IsEnabledProperty);
    
    public static void SetIsEnabled(Popup element, bool value) => element.SetValue(IsEnabledProperty, value);

    public static PopupPlacementAnimationCollection? GetAnimations(Popup element) => element.GetValue(AnimationsProperty);

    public static void SetAnimations(Popup element, PopupPlacementAnimationCollection? value) => element.SetValue(AnimationsProperty, value);

    private static void OnIsOpenChanged(Popup popup, AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (!e.NewValue.Value || !GetIsEnabled(popup))
            return;

        ToolTip? toolTip = popup.FindLogicalDescendantOfType<ToolTip>();
        
        if (toolTip != null)
            return;
        
        PopupPlacementAnimationCollection? collection = GetAnimations(popup);
        if (collection == null) return;

        if (popup.Child is not Visual visualChild)
            return;
        
        Animation? animation = collection.GetAnimationForPlacement(popup.Placement, popup.PlacementGravity);

        if (animation != null)
            _ = animation.RunAsync(visualChild);
    }
}

internal class PopupPlacementAnimationCollection
{
    public Animation? Default { get; set; }
    public Animation? Bottom { get; set; }
    public Animation? Top { get; set; }
    public Animation? Left { get; set; }
    public Animation? Right { get; set; }
    public Animation? Center { get; set; }

    public Animation? GetAnimationForPlacement(PlacementMode placement, PopupGravity gravity)
    {
        if (placement != PlacementMode.AnchorAndGravity)
            return placement switch
            {
                PlacementMode.Bottom or PlacementMode.BottomEdgeAlignedLeft or PlacementMode.BottomEdgeAlignedRight =>
                    Bottom ?? Default,
                PlacementMode.Top or PlacementMode.TopEdgeAlignedLeft or PlacementMode.TopEdgeAlignedRight => Top ??
                    Default,
                PlacementMode.Left or PlacementMode.LeftEdgeAlignedTop or PlacementMode.LeftEdgeAlignedBottom => Left ??
                    Default,
                PlacementMode.Right or PlacementMode.RightEdgeAlignedTop or PlacementMode.RightEdgeAlignedBottom =>
                    Right ?? Default,
                PlacementMode.Center => Center ?? Default,
                _ => Default
            };
        
        if (gravity.HasFlag(PopupGravity.Bottom))
            return Bottom ?? Default;
            
        if (gravity.HasFlag(PopupGravity.Top))
            return Top ?? Default;

        if (gravity.HasFlag(PopupGravity.Right))
            return Right ?? Default;
                
        if (gravity.HasFlag(PopupGravity.Left))
            return Left ?? Default;

        return Center ?? Default;
    }
}