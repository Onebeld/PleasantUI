# ProgressRing

A circular progress indicator supporting both determinate and indeterminate states with an animated arc. Inherits from `RangeBase` so `Minimum`, `Maximum`, and `Value` work the same as `ProgressBar`.

## Basic usage

```xml
<!-- Indeterminate (spinning) -->
<ProgressRing IsIndeterminate="True" />

<!-- Determinate (50%) -->
<ProgressRing Value="50" />

<!-- Custom size -->
<ProgressRing IsIndeterminate="True" Width="64" Height="64" />
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Value` | `double` | `0` | Current progress value (inherited from `RangeBase`) |
| `Minimum` | `double` | `0` | Minimum value |
| `Maximum` | `double` | `100` | Maximum value |
| `IsIndeterminate` | `bool` | `false` | Spinning animation — use when duration is unknown |
| `Thickness` | `double` | `3` | Stroke width of the arc |
| `PreserveAspect` | `bool` | `true` | Keeps the ring square — enforces `MinWidth`/`MinHeight` of 32 |
| `StartAngle` | `double` | `0` | Starting angle of the arc in degrees |
| `EndAngle` | `double` | `360` | Ending angle of the arc in degrees |

## Sizes

```xml
<!-- Small inline indicator -->
<ProgressRing IsIndeterminate="True" Width="16" Height="16" Thickness="2" />

<!-- Standard -->
<ProgressRing IsIndeterminate="True" Width="32" Height="32" />

<!-- Large hero indicator -->
<ProgressRing IsIndeterminate="True" Width="96" Height="96" Thickness="6" />
```

## Binding to a value

```xml
<ProgressRing Value="{Binding UploadProgress}"
              Minimum="0"
              Maximum="100" />
```

## Custom arc range (partial ring)

```xml
<!-- Bottom half only -->
<ProgressRing Value="50"
              StartAngle="180"
              EndAngle="360" />
```

## Styling

The ring uses `AccentLinearGradientBrush` for the arc foreground and `ControlFillColor3` for the track by default. Override via standard property setters:

```xml
<ProgressRing IsIndeterminate="True"
              Foreground="{DynamicResource SystemFillColorSuccess}"
              Background="Transparent" />
```
