# PleasantBorder

A `Border` subclass with a `BackgroundLevel` property that maps to one of four themed background elevation levels (`BackgroundColor1`–`BackgroundColor4`). Useful for building layered card/panel layouts without hardcoding brush keys.

## Basic usage

```xml
<!-- Level 1 — lightest (window background) -->
<PleasantBorder BackgroundLevel="Level1" CornerRadius="8" Padding="16">
    <TextBlock Text="Level 1" />
</PleasantBorder>

<!-- Level 2 — slightly elevated panel -->
<PleasantBorder BackgroundLevel="Level2" CornerRadius="8" Padding="16">
    <TextBlock Text="Level 2" />
</PleasantBorder>

<!-- Level 3 — card surface -->
<PleasantBorder BackgroundLevel="Level3" CornerRadius="8" Padding="16">
    <TextBlock Text="Level 3" />
</PleasantBorder>

<!-- Level 4 — darkest / most elevated -->
<PleasantBorder BackgroundLevel="Level4" CornerRadius="8" Padding="16">
    <TextBlock Text="Level 4" />
</PleasantBorder>
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `BackgroundLevel` | `BackgroundLeveling` | `Level1` | Which background color token to use |

All standard `Border` properties (`CornerRadius`, `BorderBrush`, `BorderThickness`, `Padding`, `Child`, etc.) are inherited.

## BackgroundLeveling values

| Value | Maps to |
|---|---|
| `Level1` | `BackgroundColor1` (lightest) |
| `Level2` | `BackgroundColor2` |
| `Level3` | `BackgroundColor3` |
| `Level4` | `BackgroundColor4` (darkest) |

## Nested elevation example

```xml
<PleasantBorder BackgroundLevel="Level1" CornerRadius="12" Padding="20">
    <StackPanel Spacing="12">
        <TextBlock Text="Window" FontWeight="SemiBold" />
        <PleasantBorder BackgroundLevel="Level2" CornerRadius="8" Padding="16">
            <StackPanel Spacing="8">
                <TextBlock Text="Panel" />
                <PleasantBorder BackgroundLevel="Level3" CornerRadius="6" Padding="12">
                    <TextBlock Text="Card" />
                </PleasantBorder>
            </StackPanel>
        </PleasantBorder>
    </StackPanel>
</PleasantBorder>
```
