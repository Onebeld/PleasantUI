# ThemePreviewVariantScope

A specialized `ThemeVariantScope` that ensures the correct display of the current theme. Provides a workaround for a potential issue where the initially selected theme is not displayed correctly.

## Basic usage

```xml
<controls:ThemePreviewVariantScope RequestedThemeVariant="Light">
    <!-- Content with the applied theme -->
    <StackPanel Spacing="8">
        <TextBlock Text="Themed content" />
        <Button Content="Button" />
    </StackPanel>
</controls:ThemePreviewVariantScope>
```

## Properties

Inherits all properties from `Avalonia.Styling.ThemeVariantScope`:
- `RequestedThemeVariant` - The requested theme variant (Light, Dark, or custom)
- `ActualThemeVariant` - The actual theme variant currently applied

## Purpose

This control addresses a theme rendering issue where the initially selected theme might not display correctly. It achieves this by briefly switching to a default theme variant during template application, then switching back to the requested theme.

## Theme variants

Apply different theme variants:

```xml
<!-- Light theme -->
<controls:ThemePreviewVariantScope RequestedThemeVariant="Light" />

<!-- Dark theme -->
<controls:ThemePreviewVariantScope RequestedThemeVariant="Dark" />

<!-- Custom theme -->
<controls:ThemePreviewVariantScope RequestedThemeVariant="MyCustomTheme" />
```

## Example

```xml
<controls:ThemePreviewVariantScope RequestedThemeVariant="{Binding CurrentTheme}">
    <Grid>
        <TextBlock Text="This content uses the current theme" />
        <Button Content="Themed Button" />
    </Grid>
</controls:ThemePreviewVariantScope>
```

## Notes

- This control is typically used in theme preview scenarios or when theme switching issues occur
- The workaround is applied automatically during template application
- Inherits all standard `ThemeVariantScope` functionality
