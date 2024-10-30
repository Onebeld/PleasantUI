using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace PleasantUI.Controls;

/// <summary>
/// A control used to indicate the progress of an operation.
/// </summary>
/// <remarks>
/// Reference: https://github.com/ymg2006/FluentAvalonia.ProgressRing
/// </remarks>
[PseudoClasses(":preserveaspect", ":indeterminate")]
public class ProgressRing : RangeBase
{
    /// <summary>
    /// Defines the <see cref="IsIndeterminate" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsIndeterminateProperty =
        ProgressBar.IsIndeterminateProperty.AddOwner<ProgressRing>();

    /// <summary>
    /// Defines the <see cref="PreserveAspect" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> PreserveAspectProperty =
        AvaloniaProperty.Register<ProgressRing, bool>(nameof(PreserveAspect), true);

    /// <summary>
    /// Defines the <see cref="ValueAngle" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ValueAngleProperty =
        AvaloniaProperty.Register<ProgressRing, double>(nameof(ValueAngle));

    /// <summary>
    /// Defines the <see cref="StartAngle" /> property.
    /// </summary>
    public static readonly StyledProperty<double> StartAngleProperty =
        AvaloniaProperty.Register<ProgressRing, double>(nameof(StartAngle));

    /// <summary>
    /// Defines the <see cref="Thickness" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ThicknessProperty =
        AvaloniaProperty.Register<ProgressRing, double>(nameof(Thickness), 3);

    /// <summary>
    /// Defines the <see cref="EndAngle" /> property.
    /// </summary>
    public static readonly StyledProperty<double> EndAngleProperty =
        AvaloniaProperty.Register<ProgressRing, double>(nameof(EndAngle), 360);

    /// <summary>
    /// Gets or sets a value indicating whether the property is in an indeterminate state.
    /// </summary>
    /// <value>
    /// <c>true</c> if the property is in an indeterminate state; otherwise, <c>false</c>.
    /// </value>
    public bool IsIndeterminate
    {
        get => GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the aspect ratio of the progress ring should be preserved.
    /// </summary>
    /// <value>
    /// <c>true</c> if the aspect ratio should be preserved; otherwise, <c>false</c>.
    /// </value>
    public bool PreserveAspect
    {
        get => GetValue(PreserveAspectProperty);
        set => SetValue(PreserveAspectProperty, value);
    }

    /// <summary>
    /// Gets the angle representing the current value of the progress ring.
    /// </summary>
    public double ValueAngle
    {
        get => GetValue(ValueAngleProperty);
        private set => SetValue(ValueAngleProperty, value);
    }

    /// <summary>
    /// Gets or sets the starting angle of the progress ring.
    /// </summary>
    public double StartAngle
    {
        get => GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    /// <summary>
    /// Gets or sets the ending angle of the progress ring.
    /// </summary>
    public double EndAngle
    {
        get => GetValue(EndAngleProperty);
        set => SetValue(EndAngleProperty, value);
    }

    /// <summary>
    /// Gets or sets the thickness of the progress ring.
    /// </summary>
    public double Thickness
    {
        get => GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }
    
    static ProgressRing()
    {
        ValueProperty.Changed.AddClassHandler<ProgressRing>(OnValuePropertyChanged);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressRing" /> class.
    /// </summary>
    public ProgressRing()
    {
        UpdatePseudoClasses(IsIndeterminate, PreserveAspect);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsIndeterminateProperty)
            PseudoClasses.Set(":indeterminate", change.NewValue as bool? ?? default);
        else if (change.Property == PreserveAspectProperty)
            UpdatePseudoClasses(null, change.NewValue as bool? ?? default);
    }

    private static void OnValuePropertyChanged(ProgressRing sender, AvaloniaPropertyChangedEventArgs e)
    {
        sender.ValueAngle = ((double)e.NewValue - sender.Minimum) * (sender.EndAngle - sender.StartAngle) /
                            (sender.Maximum - sender.Minimum);
    }

    private void UpdatePseudoClasses(bool? isIndeterminate, bool? preserveAspect)
    {
        if (isIndeterminate.HasValue) PseudoClasses.Set(":indeterminate", isIndeterminate.Value);

        if (preserveAspect.HasValue) PseudoClasses.Set(":preserveaspect", preserveAspect.Value);
    }
}