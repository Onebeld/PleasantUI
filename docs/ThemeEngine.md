# Theme Engine

PleasantUI ships 10 built-in themes, a System mode that follows the OS, and a full custom theme editor with create/edit/export/import support.

## Built-in themes

`Light`, `Dark`, `Mint`, `Strawberry`, `Ice`, `Sunny`, `Spruce`, `Cherry`, `Cave`, `Lunar`, `System`

## Switching theme in code

```csharp
// Switch to a built-in theme by name
var theme = PleasantTheme.Themes.First(t => t.Name == "Dark");
PleasantSettings.Current!.Theme = theme.ThemeVariant;
```

## Binding a theme selector in AXAML

```xml
<ComboBox ItemsSource="{x:Static PleasantTheme.Themes}"
          SelectedItem="{Binding SelectedTheme}">
    <ComboBox.ItemTemplate>
        <DataTemplate DataType="models:Theme">
            <TextBlock Text="{Localize {CompiledBinding Name}, Context=Theme}" />
        </DataTemplate>
    </ComboBox.ItemTemplate>
</ComboBox>
```

## Custom themes

### Creating programmatically

```csharp
var colors = PleasantTheme.GetThemeTemplateDictionary(); // start from Light
colors["AccentColor"] = Color.Parse("#FF6B35");          // override accent

var custom = new CustomTheme
{
    Name   = "My Theme",
    Colors = colors.ToColorPalette()
};

PleasantTheme.CustomThemes.Add(custom);
PleasantTheme.SelectedCustomTheme = custom;
```

### Using the built-in editor

```csharp
// Open the theme editor window (from PleasantUI.ToolKit)
CustomTheme? result = await ThemeEditorWindow.EditTheme(window, existingTheme);
if (result is not null)
    PleasantTheme.CustomThemes.Add(result);
```

### Editing an existing custom theme

```csharp
CustomTheme? updated = await ThemeEditorWindow.EditTheme(window, myTheme);
if (updated is not null)
    PleasantTheme.EditCustomTheme(myTheme, updated);
```

## Accent color

The accent color is automatically derived from the OS accent on Windows/macOS, or can be set manually:

```csharp
PleasantSettings.Current!.NumericalAccentColor = 0xFF6B35FF; // ARGB
PleasantSettings.Current!.PreferUserAccentColor = true;
```

Light/dark accent variants (`AccentLightColor1`–`AccentLightColor3`, `AccentDarkColor1`–`AccentDarkColor3`) and gradient brushes are generated automatically.

## Settings persistence

`PleasantSettings` is loaded from and saved to disk automatically on desktop. The settings file lives at:

```
%AppData%/[AppName]/Settings/Settings.json   (Windows)
~/.config/[AppName]/Settings/Settings.json   (Linux/macOS)
```

Key settings:

| Setting | Description |
|---|---|
| `Theme` | Active `ThemeVariant` |
| `NumericalAccentColor` | User accent color (ARGB int) |
| `PreferUserAccentColor` | Use user color instead of OS accent |
| `CustomThemeId` | ID of the active custom theme |
| `WindowSettings.EnableBlur` | Global blur toggle |
| `WindowSettings.EnableCustomTitleBar` | Global custom title bar toggle |
| `WindowSettings.OpacityLevel` | Background opacity (0–1) |

## Color tokens

All theme colors are exposed as `DynamicResource` keys:

| Key | Usage |
|---|---|
| `BackgroundColor1`–`4` | Window/panel backgrounds (lightest to darkest) |
| `ControlFillColor1`–`3` | Control fill states (normal, hover, pressed) |
| `TextFillColor1`–`3` | Text (primary, secondary, disabled) |
| `AccentColor` | Primary accent |
| `AccentLightColor1`–`3` | Lighter accent variants |
| `AccentLinearGradientBrush` | Gradient brush for accent buttons |
| `ControlBorderColor` | Default border color |
| `SystemFillColorCritical` | Error red |
| `SystemFillColorSuccess` | Success green |
| `SystemFillColorCaution` | Warning yellow |
| `DangerColor` | Danger button background |
