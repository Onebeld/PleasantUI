# InformationBlock

A compact pill-shaped label that combines an icon and a value. Useful for displaying counts, statuses, or metadata inline.

## Usage

```xml
<InformationBlock Content="42" Icon="{DynamicResource StarRegular}" />

<InformationBlock Content="Online" Icon="{DynamicResource CheckmarkRegular}" />

<InformationBlock Content="{Binding FileCount}"
                  Icon="{DynamicResource FileRegular}" />
```

## Properties

| Property | Type | Description |
|---|---|---|
| `Content` | `object` | The value displayed next to the icon (inherited from `ContentControl`) |
| `Icon` | `Geometry` | Icon geometry shown on the left |

## Styling

The pill uses `ControlFillColor1` background with a 1px border and `RoundedControlCornerRadius` (999 — fully rounded). Override via standard setters:

```xml
<InformationBlock Content="Error"
                  Icon="{DynamicResource ErrorCircleRegular}"
                  Background="{DynamicResource SystemFillColorCritical}"
                  Foreground="White" />
```
