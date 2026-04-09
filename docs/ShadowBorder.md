# ShadowBorder

A decorator control that renders a drop shadow behind its child content. Provides customizable shadow color, blur radius, offset, and corner radius with cached bitmap rendering for performance.

## Basic usage

```xml
<ShadowBorder ShadowColor="#80000000"
              BlurRadius="12"
              Offset="0,4"
              CornerRadius="8">
    <Border Background="White" CornerRadius="8" Padding="16">
        <TextBlock Text="Content with shadow" />
    </Border>
</ShadowBorder>
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `ShadowColor` | `Color` | `Colors.Black` | Color of the shadow |
| `BlurRadius` | `double` | `12` | Blur radius of the shadow in pixels |
| `Offset` | `Vector` | `0,4` | X/Y offset of the shadow |
| `CornerRadius` | `CornerRadius` | `0` | Corner radius for the shadow (matches content corners) |

## Shadow color

Customize the shadow color:

```xml
<!-- Black shadow (default) -->
<ShadowBorder ShadowColor="Black" />

<!-- Semi-transparent black -->
<ShadowBorder ShadowColor="#80000000" />

<!-- Colored shadow -->
<ShadowBorder ShadowColor="#4000AAFF" />
```

## Blur radius

Control the softness of the shadow:

```xml
<!-- Sharp shadow -->
<ShadowBorder BlurRadius="4" />

<!-- Soft shadow (default) -->
<ShadowBorder BlurRadius="12" />

<!-- Very soft shadow -->
<ShadowBorder BlurRadius="24" />
```

## Offset

Adjust the shadow position:

```xml
<!-- Downward shadow (default) -->
<ShadowBorder Offset="0,4" />

<!-- Upward shadow -->
<ShadowBorder Offset="0,-4" />

<!-- Rightward shadow -->
<ShadowBorder Offset="4,0" />

<!-- Diagonal shadow -->
<ShadowBorder Offset="2,2" />
```

## Corner radius

Match the shadow to rounded content:

```xml
<ShadowBorder CornerRadius="8">
    <Border Background="White" CornerRadius="8" Padding="16">
        <!-- Content -->
    </Border>
</ShadowBorder>
```

## Performance

The shadow bitmap is cached and only regenerated when:
- The child content size changes
- Shadow properties (color, blur, offset, corner radius) change
- The control is detached from the visual tree

This caching ensures good performance even with complex shadows.

## Example

```xml
<ShadowBorder ShadowColor="#40000000"
              BlurRadius="16"
              Offset="0,8"
              CornerRadius="12">
    <Border Background="White"
            CornerRadius="12"
            Padding="20"
            BoxShadow="none">
        <StackPanel Spacing="8">
            <TextBlock Text="Card Title" FontWeight="Bold" FontSize="18" />
            <TextBlock Text="Card content goes here" />
        </StackPanel>
    </Border>
</ShadowBorder>
```
