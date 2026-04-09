# BackdropBlurBorder

A control that blurs the background using the Skia API. Provides a frosted glass effect with customizable blur radius, tint opacity, and border styling.

## Basic usage

```xml
<BackdropBlurBorder Background="#80000000"
                   BlurRadius="15"
                   TintOpacity="0.5"
                   CornerRadius="8"
                   BorderThickness="1"
                   BorderBrush="#40000000">
    <TextBlock Text="Content with blur effect" />
</BackdropBlurBorder>
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Background` | `IBrush?` | — | Background brush used for the tint overlay (must be a solid color) |
| `BlurRadius` | `double` | `15` | Radius of the blur effect in pixels |
| `TintOpacity` | `double` | `0.5` | Opacity of the tint overlay (0.0 to 1.0) |
| `CornerRadius` | `CornerRadius` | `0` | Corner radius for rounded corners |
| `BorderThickness` | `Thickness` | `0` | Thickness of the border |
| `BorderBrush` | `IBrush?` | — | Brush for the border |

## Blur effect

Adjust the blur radius to control the strength of the blur effect:

```xml
<!-- Light blur -->
<BackdropBlurBorder BlurRadius="5" />

<!-- Medium blur (default) -->
<BackdropBlurBorder BlurRadius="15" />

<!-- Heavy blur -->
<BackdropBlurBorder BlurRadius="30" />
```

## Tint opacity

Control the opacity of the background tint:

```xml
<!-- Subtle tint -->
<BackdropBlurBorder Background="#80000000" TintOpacity="0.3" />

<!-- Medium tint (default) -->
<BackdropBlurBorder Background="#80000000" TintOpacity="0.5" />

<!-- Strong tint -->
<BackdropBlurBorder Background="#80000000" TintOpacity="0.8" />
```

## Border styling

Add a border with rounded corners:

```xml
<BackdropBlurBorder CornerRadius="12"
                   BorderThickness="2"
                   BorderBrush="#40000000">
    <!-- Content -->
</BackdropBlurBorder>
```

## Notes

- The `Background` property must be a solid color (`ISolidColorBrush`) for the blur effect to work properly
- The control updates every frame to prevent visual artifacts during animations, which may impact performance
- The blur effect is rendered using Skia's image filter API
