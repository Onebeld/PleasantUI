![GitHub License](https://img.shields.io/github/license/onebeld/PleasantUI?style=flat-square)
![GitHub repo size](https://img.shields.io/github/repo-size/onebeld/PleasantUI?style=flat-square)
![Nuget](https://img.shields.io/nuget/dt/PleasantUI?style=flat-square&logo=nuget)
![GitHub release (with filter)](https://img.shields.io/github/v/release/onebeld/PleasantUI?style=flat-square)

<img align="center" src="https://i.imgur.com/Sr3crB8.png">

![Imgur](https://i.imgur.com/7HXbYEo.png)

# PleasantUI

> **Repositories:** [Original (Onebeld)](https://github.com/Onebeld/PleasantUI) · [Fork (ghudulf)](https://github.com/ghudulf/PleasantUI)

PleasantUI is a cross-platform UI theme and control library for [Avalonia](https://github.com/AvaloniaUI/Avalonia), inspired by Microsoft Fluent Design and the WinUI/UWP visual language. It completely re-styles every standard Avalonia control and adds a suite of custom controls, a multi-theme engine with custom theme support, a reactive localization system, and a custom window chrome — all AOT-compatible with no `rd.xml` required.

The project has been in active development since 2021, originally as part of the [Regul](https://github.com/Onebeld/Regul) and [Regul Save Cleaner](https://github.com/Onebeld/RegulSaveCleaner) projects.

## ✨ Features

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

| Control                                 | Description                                                                                                                                                                                   | Demo                                      |
|-----------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------|
| `PleasantWindow`                        | Custom window chrome with a Fluent title bar, subtitle, custom icon/title content, optional blur, content-extends-into-titlebar, and macOS caption override                                   | None                                      |
| `NavigationView` / `NavigationViewItem` | Collapsible side navigation panel, similar to WinUI NavigationView                                                                                                                            | None                                      |
| `PleasantTabView` / `PleasantTabItem`   | Chromium-style tab strip with add/close buttons and scrollable tab bar                                                                                                                        | None                                      |
| `ContentDialog`                         | Modal overlay dialog with bottom button panel and smooth scroll content area                                                                                                                  | None                                      |
| `PleasantSnackbar`                      | Temporary non-intrusive notification bar                                                                                                                                                      | ![Imgur](https://i.imgur.com/tAppDaS.gif) |
| `ProgressRing`                          | Circular progress indicator — both determinate and indeterminate with animated arc                                                                                                            | ![Imgur](https://i.imgur.com/htsqFUE.gif) |
| `OptionsDisplayItem`                    | Settings-style row with header, description, icon, action button slot, navigation chevron, and expandable content                                                                             | ![Imgur](https://i.imgur.com/ro1lGRN.png) |
| `InformationBlock`                      | Compact pill-shaped label combining an icon and a value                                                                                                                                       | ![Imgur](https://i.imgur.com/SX0BZ2k.png) |
| `MarkedTextBox` / `MarkedNumericUpDown` | Input controls with inline label/unit markers                                                                                                                                                 | None                                      |
| `RippleEffect`                          | Material-style ripple click feedback                                                                                                                                                          | ![Imgur](https://i.imgur.com/2WtIPvj.gif) |
| `SmoothScrollViewer`                    | ScrollViewer with inertia gesture support                                                                                                                                                     | None                                      |
| `PleasantMiniWindow`                    | Lightweight floating window                                                                                                                                                                   | None                                      |
| `Timeline`                              | Displays a list of events in chronological order along a vertical axis. Supports four layout modes, custom icons, and five severity types.                                                    | ![Imgur](https://i.imgur.com/DGP3nDR.png) |
| `InstallWizard`                         | A multi-step installation wizard with a sidebar step list, progress bar, and Back / Next / Cancel navigation.                                                                                 | ![Imgur](https://i.imgur.com/bbgKH3L.png) |
| `PleasantMenu`                          | A customizable flyout menu with a title, optional info badges, a grid of large icon buttons, and a footer bar with small utility buttons.                                                     | ![Imgur](https://i.imgur.com/kGYM2fL.png) |
| `PathPicker`                            | Combines a read-only TextBox with a browse button. Supports OpenFile, SaveFile, and OpenFolder modes, optional multi-select, file-type filters, and two-way binding on the selected path text. | ![Imgur](https://i.imgur.com/qYyeQDd.png) |
| `PopConfirm`                            | Wraps any trigger control and shows a small popup with a header, body, and Confirm / Cancel buttons before executing a command.                                                               | ![Imgur](https://i.imgur.com/D8VEQ8J.png) |

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

## Packages

| Package | Description |
|---|---|
| `PleasantUI` | Core theme, all control styles, Pleasant controls, theme engine, localization |
| `PleasantUI.ToolKit` | `MessageBox`, `ThemeEditorWindow`, color picker utilities |
| `PleasantUI.MaterialIcons` | Material Design icon geometry library for use with `PathIcon` |
| `PleasantUI.DataGrid` | Fluent-styled DataGrid extension |

## 📖 Documentation

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
| [Timeline](docs/Timeline.md) | `Timeline`, `TimelineItem` |
| [InstallWizard](docs/InstallWizard.md) | `InstallWizard`, `WizardStep` |
| [PathPicker](docs/PathPicker.md) | `PathPicker` |
| [PopConfirm](docs/PopConfirm.md) | `PopConfirm` |
| [PleasantMenu](docs/PleasantMenu.md) | `PleasantMenu`, `PleasantMenuItem`, `PleasantMenuFooterItem` |
| [PinCode](docs/PinCode.md) | `PinCode` |
| [SelectionList](docs/SelectionList.md) | `SelectionList`, `SelectionListItem` |
| [RippleEffect](docs/RippleEffect.md) | `RippleEffect` |
| [SmoothScrollViewer](docs/SmoothScrollViewer.md) | `SmoothScrollViewer` |
| [MarkedInputs](docs/MarkedInputs.md) | `MarkedTextBox`, `MarkedNumericUpDown` |
| [PleasantBorder](docs/PleasantBorder.md) | `PleasantBorder` |
| [PleasantTrayPopup](docs/PleasantTrayPopup.md) | `PleasantTrayPopup`, `StatusItem` |
| [DataGrid](docs/DataGrid.md) | `PleasantUI.DataGrid` package |
| [Localization](docs/Localization.md) | `Localizer`, `{Localize}` markup extension |
| [Theme Engine](docs/ThemeEngine.md) | `PleasantTheme`, custom themes, color tokens |

## 🚀 Getting Started

### Install

**Package List (Avalonia 12)**

Published on NuGet:

```xml
<PackageReference Include="PleasantUI" Version="5.1.0-alpha4.2" />
<PackageReference Include="PleasantUI.DataGrid" Version="5.1.0-alpha4.2" />
<PackageReference Include="PleasantUI.MaterialIcons" Version="5.1.0-alpha4.2" />
<PackageReference Include="PleasantUI.ToolKit" Version="5.1.0-alpha4.2" />
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

## 🌍 Localization

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

## 🔲 Button variants

```xml
<Button Content="Default" />
<Button Theme="{DynamicResource AccentButtonTheme}" Content="Accent" />
<Button Theme="{DynamicResource DangerButtonTheme}" Content="Danger" />
<Button Theme="{DynamicResource AppBarButtonTheme}" Content="AppBar" />
```

## 📃 OptionsDisplayItem

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

## 🖼️ Screenshots

[Regul Save Cleaner](https://github.com/Onebeld/RegulSaveCleaner)

![image](https://github.com/Onebeld/PleasantUI/assets/44552715/72544683-228f-4d1d-9465-e0401828bd5d)

[OlibKey](https://github.com/Onebeld/OlibKey)

![image](https://github.com/Onebeld/OlibKey/assets/44552715/c6f78465-0e3a-4757-ba03-903e93ec3e04)

## ❤️ Credits

- [Avalonia](https://github.com/AvaloniaUI/Avalonia)
- Some controls inspired by PieroCastillo's [Aura.UI](https://github.com/PieroCastillo/Aura.UI)
- [ProgressRing](https://github.com/ymg2006/FluentAvalonia.ProgressRing) by ymg2006
- Built with [JetBrains Rider](https://www.jetbrains.com/rider/)

<img src="https://i.imgur.com/IvbDwuz.png" width="360" align="right"/>
