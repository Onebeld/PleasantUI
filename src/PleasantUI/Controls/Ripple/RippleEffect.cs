using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;

namespace PleasantUI.Controls;

/// <summary>
/// A control that provides a ripple effect on pointer interaction.
/// </summary>
public class RippleEffect : ContentControl
{
	private bool _isCancelled;

	private CompositionContainerVisual? _container;
	private CompositionCustomVisual? _last;
	private byte _pointers;

	/// <summary>
	/// Defines the <see cref="RippleFill"/> property.
	/// </summary>
	public static readonly StyledProperty<IBrush> RippleFillProperty =
		AvaloniaProperty.Register<RippleEffect, IBrush>(nameof(RippleFill), inherits: true,
			defaultValue: Brushes.White);

	/// <summary>
	/// Defines the <see cref="RippleOpacity"/> property.
	/// </summary>
	public static readonly StyledProperty<double> RippleOpacityProperty =
		AvaloniaProperty.Register<RippleEffect, double>(nameof(RippleOpacity), inherits: true, defaultValue: 0.6);

	/// <summary>
	/// Defines the <see cref="RaiseRippleCenter"/> property.
	/// </summary>
	public static readonly StyledProperty<bool> RaiseRippleCenterProperty =
		AvaloniaProperty.Register<RippleEffect, bool>(nameof(RaiseRippleCenter));

	/// <summary>
	/// Defines the <see cref="IsAllowedRaiseRipple"/> property.
	/// </summary>
	public static readonly StyledProperty<bool> IsAllowedRaiseRippleProperty =
		AvaloniaProperty.Register<RippleEffect, bool>(nameof(IsAllowedRaiseRipple), defaultValue: true);

	/// <summary>
	/// Defines the <see cref="UseTransitions"/> property.
	/// </summary>
	public static readonly StyledProperty<bool> UseTransitionsProperty =
		AvaloniaProperty.Register<RippleEffect, bool>(nameof(UseTransitions), defaultValue: true);

	/// <summary>
	/// Gets or sets the brush used to fill the ripple.
	/// </summary>
	public IBrush RippleFill
	{
		get => GetValue(RippleFillProperty);
		set => SetValue(RippleFillProperty, value);
	}

	/// <summary>
	/// Gets or sets the opacity of the ripple.
	/// </summary>
	public double RippleOpacity
	{
		get => GetValue(RippleOpacityProperty);
		set => SetValue(RippleOpacityProperty, value);
	}

	/// <summary>
	/// Gets or sets a value indicating whether the ripple should originate from the center of the control.
	/// </summary>
	public bool RaiseRippleCenter
	{
		get => GetValue(RaiseRippleCenterProperty);
		set => SetValue(RaiseRippleCenterProperty, value);
	}

	/// <summary>
	/// Gets or sets a value indicating whether the ripple effect is allowed.
	/// </summary>
	public bool IsAllowedRaiseRipple
	{
		get => GetValue(IsAllowedRaiseRippleProperty);
		set => SetValue(IsAllowedRaiseRippleProperty, value);
	}

	/// <summary>
	/// Gets or sets a value indicating whether to use transitions for the ripple effect.
	/// </summary>
	public bool UseTransitions
	{
		get => GetValue(UseTransitionsProperty);
		set => SetValue(UseTransitionsProperty, value);
	}

	static RippleEffect()
	{
		BackgroundProperty.OverrideDefaultValue<RippleEffect>(Brushes.Transparent);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="RippleEffect"/> class.
	/// </summary>
	public RippleEffect()
	{
		AddHandler(LostFocusEvent, LostFocusHandler);
		AddHandler(PointerReleasedEvent, PointerReleasedHandler);
		AddHandler(PointerPressedEvent, PointerPressedHandler);
		AddHandler(PointerCaptureLostEvent, PointerCaptureLostHandler);
	}

	/// <inheritdoc/>
	protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnAttachedToVisualTree(e);

		CompositionVisual thisVisual = ElementComposition.GetElementVisual(this)!;
		_container = thisVisual.Compositor.CreateContainerVisual();
		_container.Size = new Vector(Bounds.Width, Bounds.Height);
		ElementComposition.SetElementChildVisual(this, _container);
	}

	/// <inheritdoc/>
	protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnDetachedFromVisualTree(e);

		_container = null;
		ElementComposition.SetElementChildVisual(this, null);
	}

	/// <inheritdoc/>
	protected override void OnSizeChanged(SizeChangedEventArgs e)
	{
		base.OnSizeChanged(e);

		if (_container is { } container)
		{
			Vector newSize = new Vector(e.NewSize.Width, e.NewSize.Height);
			if (newSize != default)
			{
				container.Size = newSize;
				foreach (CompositionVisual? child in container.Children)
				{
					child.Size = newSize;
				}
			}
		}
	}

	private void PointerPressedHandler(object? sender, PointerPressedEventArgs e)
	{
		(double x, double y) = e.GetPosition(this);
		if (_container is null || x < 0 || x > Bounds.Width || y < 0 || y > Bounds.Height)
		{
			return;
		}

		_isCancelled = false;

		if (!IsAllowedRaiseRipple)
			return;

		if (_pointers != 0)
			return;

		// Only first pointer can arrive a ripple
		_pointers++;
		CompositionCustomVisual r = CreateRipple(x, y, RaiseRippleCenter);
		_last = r;

		// Attach ripple instance to canvas
		_container.Children.Add(r);
		r.SendHandlerMessage(RippleHandler.FirstStepMessage);

		if (_isCancelled)
		{
			RemoveLastRipple();
		}
	}

	private void LostFocusHandler(object? sender, RoutedEventArgs e)
	{
		_isCancelled = true;
		RemoveLastRipple();
	}

	private void PointerReleasedHandler(object? sender, PointerReleasedEventArgs e)
	{
		_isCancelled = true;
		RemoveLastRipple();
	}

	private void PointerCaptureLostHandler(object? sender, PointerCaptureLostEventArgs e)
	{
		_isCancelled = true;
		RemoveLastRipple();
	}

	private void RemoveLastRipple()
	{
		if (_last == null)
			return;

		_pointers--;

		// This way to handle pointer released is pretty tricky
		// could have more better way to improve
		OnReleaseHandler(_last);
		_last = null;
	}

	private void OnReleaseHandler(CompositionCustomVisual r)
	{
		// Fade out ripple
		r.SendHandlerMessage(RippleHandler.SecondStepMessage);

		// Remove ripple from canvas to finalize ripple instance
		CompositionContainerVisual? container = _container;
		DispatcherTimer.RunOnce(() => { container?.Children.Remove(r); }, Ripple.Duration, DispatcherPriority.Render);
	}

	private CompositionCustomVisual CreateRipple(double x, double y, bool center)
	{
		double w = Bounds.Width;
		double h = Bounds.Height;
		bool t = UseTransitions;

		if (center)
		{
			x = w / 2;
			y = h / 2;
		}

		RippleHandler handler = new(
			RippleFill.ToImmutable(),
			Ripple.Easing,
			Ripple.Duration,
			RippleOpacity,
			CornerRadius,
			x, y, w, h, t);

		CompositionCustomVisual visual =
			ElementComposition.GetElementVisual(this)!.Compositor.CreateCustomVisual(handler);
		visual.Size = new Vector(Bounds.Width, Bounds.Height);
		return visual;
	}
}