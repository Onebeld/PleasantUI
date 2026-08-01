/*
 * SPDX-FileCopyrightText: 2026 Dmitry Zhutkov (Onebeld) <onebeld@gmail.com>
 * SPDX-FileCopyrightText: 2025 AvaloniaCommunity <https://github.com/AvaloniaCommunity>
 * SPDX-License-Identifier: MIT
 *
 * Derived from:
 * https://github.com/AvaloniaCommunity/Material.Avalonia/blob/master/Material.Ripple/RippleEffect.cs
 */

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Rendering.Composition;

namespace PleasantUI.Controls;

/// <summary>
/// A control that provides a ripple effect on pointer interaction.
/// </summary>
public class RippleEffect : ContentControl
{
    private CompositionContainerVisual? _container;
    private readonly List<CompositionCustomVisual> _activeRipples = new();
    private CancellationTokenSource? _detachCts;
    
	/// <summary>
	/// Defines the <see cref="RippleFill" /> property.
	/// </summary>
	public static readonly StyledProperty<IBrush?> RippleFillProperty =
        AvaloniaProperty.Register<RippleEffect, IBrush?>(nameof(RippleFill), inherits: true,
            defaultValue: Brushes.White);

	/// <summary>
	/// Defines the <see cref="RippleOpacity" /> property.
	/// </summary>
	public static readonly StyledProperty<double> RippleOpacityProperty =
        AvaloniaProperty.Register<RippleEffect, double>(nameof(RippleOpacity), inherits: true, defaultValue: 0.6);

	/// <summary>
	/// Defines the <see cref="RaiseRippleCenter" /> property.
	/// </summary>
	public static readonly StyledProperty<bool> RaiseRippleCenterProperty =
        AvaloniaProperty.Register<RippleEffect, bool>(nameof(RaiseRippleCenter));

	/// <summary>
	/// Defines the <see cref="IsAllowedRaiseRipple" /> property.
	/// </summary>
	public static readonly StyledProperty<bool> IsAllowedRaiseRippleProperty =
        AvaloniaProperty.Register<RippleEffect, bool>(nameof(IsAllowedRaiseRipple), true);

	/// <summary>
	/// Defines the <see cref="UseTransitions" /> property.
	/// </summary>
	public static readonly StyledProperty<bool> UseTransitionsProperty =
        AvaloniaProperty.Register<RippleEffect, bool>(nameof(UseTransitions), true);

    /// <summary>
    /// Gets or sets the brush used to fill the ripple.
    /// </summary>
    public IBrush? RippleFill
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
    /// Initializes a new instance of the <see cref="RippleEffect" /> class.
    /// </summary>
    public RippleEffect()
    {
        AddHandler(LostFocusEvent, LostFocusHandler);
        AddHandler(PointerReleasedEvent, PointerReleasedHandler);
        AddHandler(PointerPressedEvent, PointerPressedHandler);
        AddHandler(PointerCaptureLostEvent, PointerCaptureLostHandler);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _detachCts = new CancellationTokenSource();

        CompositionVisual thisVisual = ElementComposition.GetElementVisual(this)!;
        _container = thisVisual.Compositor.CreateContainerVisual();
        _container.Size = new Vector(Bounds.Width, Bounds.Height);
        ElementComposition.SetElementChildVisual(this, _container);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        
        _detachCts?.Cancel();
        _detachCts?.Dispose();
        _detachCts = null;
        
        _activeRipples.Clear();

        if (_container is { } container)
            container.Children.Clear();

        _container = null;
        ElementComposition.SetElementChildVisual(this, null);
    }

    /// <inheritdoc />
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        if (_container is { } container)
        {
            Vector newSize = new(e.NewSize.Width, e.NewSize.Height);
            if (newSize != default)
            {
                container.Size = newSize;
                foreach (CompositionVisual child in container.Children) child.Size = newSize;
            }
        }
    }

    private void PointerPressedHandler(object? sender, PointerPressedEventArgs e)
    {
        (double x, double y) = e.GetPosition(this);
        if (_container is null || x < 0 || x > Bounds.Width || y < 0 || y > Bounds.Height) return;

        if (!IsAllowedRaiseRipple)
            return;

        CompositionCustomVisual? r = CreateRipple(x, y, RaiseRippleCenter);
        if (r is null) return;

        _activeRipples.Add(r);
        _container.Children.Add(r);
        r.SendHandlerMessage(RippleHandler.FirstStepMessage);
    }

    private void LostFocusHandler(object? sender, RoutedEventArgs e) => RemoveAllRipples();

    private void PointerReleasedHandler(object? sender, PointerReleasedEventArgs e) => RemoveAllRipples();

    private void PointerCaptureLostHandler(object? sender, PointerCaptureLostEventArgs e) => RemoveAllRipples();

    private void RemoveAllRipples()
    {
        if (_activeRipples.Count == 0) return;

        for (int i = _activeRipples.Count - 1; i >= 0; i--)
        {
            CompositionCustomVisual ripple = _activeRipples[i];
            _ = OnReleaseHandlerAsync(ripple);
        }
    
        _activeRipples.Clear();
    }

    private async Task OnReleaseHandlerAsync(CompositionCustomVisual r)
    {
        // Fade out ripple
        r.SendHandlerMessage(RippleHandler.SecondStepMessage);

        // Remove ripple from canvas to finalize ripple instance
        CompositionContainerVisual? container = _container;
        CancellationToken token = _detachCts?.Token ?? CancellationToken.None;

        try
        {
            await Task.Delay(Ripple.Duration, token);

            if (!token.IsCancellationRequested && container is not null)
                container.Children.Remove(r);
        }
        catch (OperationCanceledException)
        {
            
        }
    }

    private CompositionCustomVisual? CreateRipple(double x, double y, bool center)
    {
        double w = Bounds.Width;
        double h = Bounds.Height;
        bool t = UseTransitions;

        if (center)
        {
            x = w / 2;
            y = h / 2;
        }
        
        IBrush fillBrush = RippleFill ?? Brushes.White;
        IImmutableBrush immutableFill = fillBrush.ToImmutable();

        RippleHandler handler = new(
            immutableFill,
            Ripple.Easing,
            Ripple.Duration,
            RippleOpacity,
            CornerRadius,
            x, y, w, h, t);

        CompositionVisual? elementVisual = ElementComposition.GetElementVisual(this);
        if (elementVisual is null)
            return null;
        
        CompositionCustomVisual visual = elementVisual.Compositor.CreateCustomVisual(handler);
        visual.Size = new Vector(Bounds.Width, Bounds.Height);
        return visual;
    }
}