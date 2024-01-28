using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace PleasantUI.Transitions;

public class PleasantPageSlide : AvaloniaObject, IPageTransition
{
    /// <summary>
    /// The axis on which the PleasantPageSlide should occur
    /// </summary>
    public enum SlideAxis
    {
        Horizontal,
        Vertical
    }
    
    public static readonly StyledProperty<bool> ForwardProperty =
        AvaloniaProperty.Register<PleasantPageSlide, bool>(nameof(Forward), true);
    
    public static readonly StyledProperty<SlideAxis> OrientationProperty =
        AvaloniaProperty.Register<PleasantPageSlide, SlideAxis>(nameof(Orientation), SlideAxis.Horizontal);
    
    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<PleasantPageSlide, TimeSpan>(nameof(Duration), TimeSpan.FromMilliseconds(300));
    
    public TimeSpan Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }
    
    public bool Forward 
    {
        get => GetValue(ForwardProperty);
        set => SetValue(ForwardProperty, value);
    }
    
    public SlideAxis Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public virtual async Task Start(Visual? from, Visual? to, bool forward, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        
        Visual parent = GetVisualParent(from, to);
        double distance = Orientation == SlideAxis.Horizontal ? parent.Bounds.Width : parent.Bounds.Height;
        StyledProperty<double> translateProperty = Orientation == SlideAxis.Horizontal ? TranslateTransform.XProperty : TranslateTransform.YProperty;
        
        if (from is null || to is null)
            return;
        
        to.IsVisible = false;

        Animation animationFrom = new()
        {
            Easing = new CubicEaseIn(),
            Children =
            {
                new KeyFrame()
                {
                    Setters =
                    {
                        new Setter
                        {
                            Property = translateProperty,
                            Value = 0d
                        }
                    },
                    Cue = new Cue(0d)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter
                        {
                            Property = translateProperty,
                            Value = Forward ? -distance : distance
                        }
                    },
                    Cue = new Cue(1d)
                }
            },
            Duration = Duration
        };

        await animationFrom.RunAsync(from, cancellationToken);

        from.IsVisible = false;
        to.IsVisible = true;

        var animationTo = new Animation
        {
            Easing = new CubicEaseOut(),
            Children =
            {
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter
                        {
                            Property = translateProperty,
                            Value = Forward ? distance : -distance
                        }
                    },
                    Cue = new Cue(0d)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter
                        {
                            Property = translateProperty,
                            Value = 0d
                        }
                    },
                    Cue = new Cue(1d)
                }
            },
            Duration = Duration
        };

        await animationTo.RunAsync(to, cancellationToken);

        if (!cancellationToken.IsCancellationRequested)
            from.IsVisible = false;
    }
    
    /// <summary>
    /// Gets the common visual parent of the two control.
    /// </summary>
    /// <param name="from">The from control.</param>
    /// <param name="to">The to control.</param>
    /// <returns>The common parent.</returns>
    /// <exception cref="ArgumentException">
    /// The two controls do not share a common parent.
    /// </exception>
    /// <remarks>
    /// Any one of the parameters may be null, but not both.
    /// </remarks>
    protected static Visual GetVisualParent(Visual? from, Visual? to)
    {
        Visual? p1 = (from ?? to)!.GetVisualParent();
        Visual? p2 = (to ?? from)!.GetVisualParent();

        if (p1 != null && p2 != null && p1 != p2)
        {
            throw new ArgumentException("Controls for PageSlide must have same parent.");
        }

        return p1 ?? throw new InvalidOperationException("Cannot determine visual parent.");
    }
}