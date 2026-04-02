![GitHub License](https://img.shields.io/github/license/onebeld/PleasantUI?style=flat-square)
![GitHub repo size](https://img.shields.io/github/repo-size/onebeld/PleasantUI?style=flat-square)
![Nuget](https://img.shields.io/nuget/dt/PleasantUI?style=flat-square&logo=nuget)
![GitHub release (with filter)](https://img.shields.io/github/v/release/onebeld/PleasantUI?style=flat-square)

<img align="center" src="https://github.com/Onebeld/PleasantUI/assets/44552715/c8354beb-5b4b-4ce6-acbb-eb2b5e6a23e1">

# PleasantUI

> **Repositories:** [Original (Onebeld)](https://github.com/Onebeld/PleasantUI) · [Fork (ghudulf)](https://github.com/ghudulf/PleasantUI)

PleasantUI is a cross-platform UI theme and control library for [Avalonia](https://github.com/AvaloniaUI/Avalonia), inspired by Microsoft Fluent Design and the WinUI/UWP visual language. It completely re-styles every standard Avalonia control and adds a suite of custom controls, a multi-theme engine with custom theme support, a reactive localization system, and a custom window chrome — all AOT-compatible with no `rd.xml` required.

The project has been in active development since 2021, originally as part of the [Regul](https://github.com/Onebeld/Regul) and [Regul Save Cleaner](https://github.com/Onebeld/RegulSaveCleaner) projects.

---

## Features

### Complete Fluent-style control theming

Every standard Avalonia control gets a full Fluent Design makeover — rounded corners, layered fill colors, smooth pointer-over and pressed transitions, and accent color integration:

| Control | Control | Control |
|---|---|---|
| Button (+ AppBar, Accent, Danger variants) | CheckBox | RadioButton |
| ToggleButton / ToggleSwitch | RepeatButton / ButtonSpinner | Slider |
| TextBox / AutoCompleteBox | NumericUpDown | ComboBox / DropDownButton |
| Calendar / CalendarDatePicker / TimePicker | DataGrid | ListBox / TreeView |
| Expander | TabControl / TabItem | ScrollBar / ScrollViewer |
| ProgressBar | Menu / ContextMenu | ToolTip |
| Carousel | Separator | NotificationCard |

### Custom Pleasant controls

Controls built from scratch that go beyond what Avalonia ships:

| Control | Description |
|---|---|
| `PleasantWindow` | Custom window chrome with a Fluent title bar, subtitle, custom icon/title content, optional blur, content-extends-into-titlebar, and macOS caption override |
| `NavigationView` / `NavigationViewItem` | Collapsible side navigation panel, similar to WinUI NavigationView |
| `PleasantTabView` / `PleasantTabItem` | Chromium-style tab strip with add/close buttons and scrollable tab bar |
| `ContentDialog` | Modal overlay dialog with bottom button panel and smooth scroll content area |
| `PleasantSnackbar` | Temporary non-intrusive notification bar |
| `ProgressRing` | Circular progress indicator — both determinate and indeterminate with animated arc |
| `OptionsDisplayItem` | Settings-style row with header, description, icon, action button slot, navigation chevron, and expandable content |
| `InformationBlock` | Compact pill-shaped label combining an icon and a value |
| `MarkedTextBox` / `MarkedNumericUpDown` | Input controls with inline label/unit markers |
| `RippleEffect` | Material-style ripple click feedback |
| `SmoothScrollViewer` | ScrollViewer with inertia gesture support |
| `PleasantMiniWindow` | Lightweight floating window |

### Theme engine

- Built-in themes: **Light**, **Dark**, **Mint**, **Strawberry**, **Ice**, **Sunny**, **Spruce**, **Cherry**, **Cave**, **Lunar**
- **System** mode — follows the OS light/dark preference automatically
- **Custom themes** — create, edit, export, import, and persist your own color palettes via the built-in `ThemeEditorWindow`
- Accent color follows the OS accent or can be overridden per-user; light/dark variants and gradient pairs are generated automatically
- Settings are persisted to disk automatically on desktop; mobile apps can save manually

### Localization system

- `Localizer` singleton backed by .NET `ResourceManager` — add any number of `.resx` resource files
- `{Localize Key}` AXAML markup extension binds reactively — switching language updates every bound string instantly without reloading views
- `Localizer.TrDefault(key, fallback)` for safe lookups that fall back to a raw string instead of an error message
- `LocalizationChanged` event for view models and code-behind to react to language switches

---

## Packages

| Package | Description |
|---|---|
| `PleasantUI` | Core theme, all control styles, Pleasant controls, theme engine, localization |
| `PleasantUI.ToolKit` | `MessageBox`, `ThemeEditorWindow`, color picker utilities |
| `PleasantUI.MaterialIcons` | Material Design icon geometry library for use with `PathIcon` |
| `PleasantUI.DataGrid` | Fluent-styled DataGrid extension |

---

## Documentation

Detailed reference docs for each control are in the [`docs/`](docs/) folder:

| Doc | Controls |
|---|---|
| [PleasantWindow](docs/PleasantWindow.md) | `PleasantWindow`, `IPleasantSplashScreen` |
| [PleasantMiniWindow](docs/PleasantMiniWindow.md) | `PleasantMiniWindow` |
| [NavigationView](docs/NavigationView.md) | `NavigationView`, `NavigationViewItem` |
| [PleasantTabView](docs/PleasantTabView.md) | `PleasantTabView`, `PleasantTabItem` |
| [ContentDialog](docs/ContentDialog.md) | `ContentDialog` |
| [MessageBox](docs/MessageBox.md) | `MessageBox` (ToolKit) |
| [PleasantDialog](docs/PleasantDialog.md) | `PleasantDialog` (ToolKit) |
| [PleasantSnackbar](docs/PleasantSnackbar.md) | `PleasantSnackbar` |
| [ProgressRing](docs/ProgressRing.md) | `ProgressRing` |
| [OptionsDisplayItem](docs/OptionsDisplayItem.md) | `OptionsDisplayItem` |
| [InformationBlock](docs/InformationBlock.md) | `InformationBlock` |
| [DataGrid](docs/DataGrid.md) | `PleasantUI.DataGrid` package |
| [Localization](docs/Localization.md) | `Localizer`, `{Localize}` markup extension |
| [Theme Engine](docs/ThemeEngine.md) | `PleasantTheme`, custom themes, color tokens |

---

## Getting Started

### Install

> **Note:** Until [Onebeld/PleasantUI#6](https://github.com/Onebeld/PleasantUI/pull/6) is merged upstream, this fork targets Avalonia 12 and is ahead of the upstream Avalonia 11 packages. Use the fork packages if you need Avalonia 12 support.

**This fork (Avalonia 12, recommended)**

Published under the `pieckenst.Avalonia12.*` prefix on NuGet:

```xml
<PackageReference Include="pieckenst.Avalonia12.PleasantUI" Version="5.1.0-alpha4.2" />
<PackageReference Include="pieckenst.Avalonia12.PleasantUI.DataGrid" Version="5.1.0-alpha4.2" />
<PackageReference Include="pieckenst.Avalonia12.PleasantUI.MaterialIcons" Version="5.1.0-alpha4.2" />
<PackageReference Include="pieckenst.Avalonia12.PleasantUI.ToolKit" Version="5.1.0-alpha4.2" />
```

**Original upstream (Avalonia 11)**

Only `PleasantUI` and `PleasantUI.DataGrid` are published upstream — `MaterialIcons` and `ToolKit` are exclusive to this fork.

```xml
<PackageReference Include="PleasantUI" Version="5.0.0-alpha3" />
<PackageReference Include="PleasantUI.DataGrid" Version="5.0.0-alpha2" />
```

### Add the theme

In your `App.axaml`, add `PleasantTheme` to your styles:

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="YourApp.App">
    <Application.Styles>
        <PleasantTheme />
    </Application.Styles>
</Application>
```

### Initialize correctly

Make sure `AvaloniaXamlLoader.Load(this)` is called in `Initialize()`:

```csharp
public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this); // required
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
```

### Use PleasantWindow

Replace `Window` with `PleasantWindow` to get the custom Fluent title bar:

```csharp
using PleasantUI.Controls;

public partial class MainWindow : PleasantWindow
{
    public MainWindow() => InitializeComponent();
}
```

```xml
<PleasantWindow xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
                xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
                mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
                x:Class="YourApp.Views.MainWindow"
                Title="Avalonia Application">
</PleasantWindow>
```

Key `PleasantWindow` properties:

| Property | Type | Description |
|---|---|---|
| `TitleBarType` | `Classic` / `ClassicExtended` | Title bar layout style |
| `ExtendsContentIntoTitleBar` | `bool` | Lets content render behind the title bar |
| `Subtitle` | `string` | Secondary text shown next to the title |
| `DisplayIcon` | `object` | Custom icon content in the title bar |
| `DisplayTitle` | `object` | Custom title content (e.g. a `PathIcon`) |
| `EnableBlur` | `bool` | Acrylic/blur window background |
| `CaptionButtons` | enum | Which caption buttons to show |
| `LeftTitleBarContent` | `object` | Content injected left of the title |

---

## Localization

Register your `.resx` resource managers in your `Application` constructor:

```csharp
public App()
{
    Localizer.AddRes(new ResourceManager(typeof(Properties.Localizations.App)));
    Localizer.ChangeLang("en");
}
```

Use `{Localize Key}` in AXAML — updates live when the language changes:

```xml
<TextBlock Text="{Localize WelcomeMessage}" />
<Button Content="{Localize SaveButton}" />
```

Switch language at runtime:

```csharp
Localizer.ChangeLang("ru");
```

Safe lookup with fallback in code-behind:

```csharp
string title = Localizer.TrDefault("DialogTitle", "Confirm");
```

---

## Button variants

```xml
<Button Content="Default" />
<Button Theme="{StaticResource AccentButtonTheme}" Content="Accent" />
<Button Theme="{StaticResource DangerButtonTheme}" Content="Danger" />
<Button Theme="{StaticResource AppBarButtonTheme}" Content="AppBar" />
```

---

## OptionsDisplayItem

```xml
<!-- Navigation row -->
<OptionsDisplayItem Header="Account"
                    Description="Manage your account"
                    Icon="{x:Static MaterialIcons.AccountOutline}"
                    Navigates="True" />

<!-- Row with action control -->
<OptionsDisplayItem Header="Dark mode"
                    Icon="{x:Static MaterialIcons.WeatherNight}">
    <OptionsDisplayItem.ActionButton>
        <ToggleSwitch />
    </OptionsDisplayItem.ActionButton>
</OptionsDisplayItem>

<!-- Expandable row -->
<OptionsDisplayItem Header="Advanced" Expands="True">
    <OptionsDisplayItem.Content>
        <StackPanel>
            <CheckBox Content="Enable feature X" />
        </StackPanel>
    </OptionsDisplayItem.Content>
</OptionsDisplayItem>
```

---

## Screenshots

[Regul Save Cleaner](https://github.com/Onebeld/RegulSaveCleaner)

![image](https://github.com/Onebeld/PleasantUI/assets/44552715/72544683-228f-4d1d-9465-e0401828bd5d)

[OlibKey](https://github.com/Onebeld/OlibKey)

![image](https://github.com/Onebeld/OlibKey/assets/44552715/c6f78465-0e3a-4757-ba03-903e93ec3e04)

---

## Credits

- [AvaloniaUI](https://github.com/AvaloniaUI/Avalonia)
- Some controls inspired by PieroCastillo's [Aura.UI](https://github.com/PieroCastillo/Aura.UI)
- [ProgressRing](https://github.com/ymg2006/FluentAvalonia.ProgressRing) by ymg2006
- Built with [JetBrains Rider](https://www.jetbrains.com/rider/)

<img src="https://github.com/Onebeld/PleasantUI/assets/44552715/c6bcf430-4153-4f72-bcca-e97e5cdce491" width="360" align="right"/>
