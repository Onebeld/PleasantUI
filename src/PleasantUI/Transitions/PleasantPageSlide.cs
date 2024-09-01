using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace PleasantUI.Transitions;

/// <summary>
/// A transition that slides pages in and out horizontally or vertically.
/// </summary>
public class PleasantPageSlide : AvaloniaObject, IPageTransition
{
    /// <summary>
    /// The axis on which the PleasantPageSlide should occur
    /// </summary>
    public enum SlideAxis
    {
        /// <summary>
        /// The slide should occur horizontally.
        /// </summary>
        Horizontal,
        
        /// <summary>
        /// The slide should occur vertically.
        /// </summary>
        Vertical
    }
    
    /// <summary>
    /// Defines the <see cref="Forward"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ForwardProperty =
        AvaloniaProperty.Register<PleasantPageSlide, bool>(nameof(Forward), true);
    
    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly StyledProperty<SlideAxis> OrientationProperty =
        AvaloniaProperty.Register<PleasantPageSlide, SlideAxis>(nameof(Orientation));
    
    /// <summary>
    /// Defines the <see cref="Duration"/> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<PleasantPageSlide, TimeSpan>(nameof(Duration), TimeSpan.FromMilliseconds(300));
    
    /// <summary>
    /// Gets or sets the duration of the transition.
    /// </summary>
    public TimeSpan Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether the transition should slide forward or backward.
    /// </summary>
    public bool Forward 
    {
        get => GetValue(ForwardProperty);
        set => SetValue(ForwardProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the orientation of the slide.
    /// </summary>
    public SlideAxis Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Starts the transition.
    /// </summary>
    /// <param name="from">The visual to transition from.</param>
    /// <param name="to">The visual to transition to.</param>
    /// <param name="forward">Whether the transition is going forward.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
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

        Animation animationTo = new()
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
    private static Visual GetVisualParent(Visual? from, Visual? to)
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