# RippleEffect

A `ContentControl` that renders a Material-style ripple animation on pointer press. Wrap any control with it to add click feedback.

## Basic usage

```xml
<RippleEffect>
    <Button Content="Click me" />
</RippleEffect>
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `RippleFill` | `IBrush` | `White` | Brush used to fill the ripple circle (inheritable) |
| `RippleOpacity` | `double` | `0.6` | Opacity of the ripple (inheritable) |
| `RaiseRippleCenter` | `bool` | `false` | When true, the ripple always originates from the center instead of the pointer position |
| `IsAllowedRaiseRipple` | `bool` | `true` | Enables or disables the ripple effect entirely |
| `UseTransitions` | `bool` | `true` | Whether to animate the ripple with easing transitions |

## Custom color

```xml
<RippleEffect RippleFill="{DynamicResource AccentColor}" RippleOpacity="0.4">
    <Border Background="{DynamicResource ControlFillColor1}"
            CornerRadius="8" Padding="16">
        <TextBlock Text="Tap me" />
    </Border>
</RippleEffect>
```

## Center ripple (e.g. icon buttons)

```xml
<RippleEffect RaiseRippleCenter="True">
    <PathIcon Data="{DynamicResource HeartRegular}" Width="24" Height="24" />
</RippleEffect>
```

## Disable ripple conditionally

```xml
<RippleEffect IsAllowedRaiseRipple="{Binding IsInteractive}">
    <Button Content="Action" />
</RippleEffect>
```

## Notes

- `RippleFill` and `RippleOpacity` are inheritable — set them on a parent container to apply to all nested `RippleEffect` controls.
- The ripple uses Avalonia's Composition API and renders on the GPU layer, so it does not affect layout.
- Only the first pointer contact triggers a ripple; subsequent simultaneous touches are ignored until the first is released.
