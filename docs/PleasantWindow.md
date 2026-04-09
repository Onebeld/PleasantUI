# PleasantWindow

A custom `Window` subclass with a Fluent-style title bar, optional blur, subtitle, custom icon/title content, and splash screen support.

## Basic usage

```xml
<PleasantWindow xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                x:Class="YourApp.MainWindow"
                Title="My App"
                Width="1000" Height="650"
                WindowStartupLocation="CenterScreen">
    <!-- your content -->
</PleasantWindow>
```

```csharp
public partial class MainWindow : PleasantWindow
{
    public MainWindow() => InitializeComponent();
}
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `TitleBarType` | `PleasantTitleBar.Type` | `Classic` | Type of title bar: `Classic`, `ClassicExtended`, or `Compact` |
| `EnableCustomTitleBar` | `bool` | `true` | Replaces the OS title bar with the Fluent one |
| `OverrideMacOSCaption` | `bool` | `true` | Overrides native macOS caption buttons |
| `ExtendsContentIntoTitleBar` | `bool` | `false` | Lets window content render behind the title bar area |
| `TitleBarHeight` | `double` | `32` | Height of the title bar (read-only, auto-set) |
| `Subtitle` | `string` | — | Secondary text shown next to the title |
| `DisplayIcon` | `object` | — | Custom icon in the title bar (e.g. `DrawingImage`, `PathIcon`) |
| `DisplayTitle` | `object` | — | Replaces the text title with arbitrary content |
| `LeftTitleBarContent` | `object` | — | Content injected to the left of the title |
| `TitleContent` | `object` | — | Content placed in the center of the title bar |
| `CaptionButtons` | `PleasantCaptionButtons.Type` | `All` | Which caption buttons to show |
| `IsCloseButtonVisible` | `bool` | `true` | Whether the close button is visible |
| `IsMinimizeButtonVisible` | `bool` | `true` | Whether the minimize button is visible |
| `IsRestoreButtonVisible` | `bool` | `true` | Whether the restore/maximize button is visible |
| `IsFullScreenButtonVisible` | `bool` | `false` | Whether the full-screen toggle button is visible |
| `EnableBlur` | `bool` | `false` | Acrylic/blur window background |
| `SplashScreen` | `IPleasantSplashScreen` | `null` | Splash screen shown on startup |

## Extended title bar with custom content

```xml
<PleasantWindow TitleBarType="ClassicExtended"
                ExtendsContentIntoTitleBar="True"
                DisplayIcon="{DynamicResource MyLogo}"
                Subtitle="v2.0">
    <PleasantWindow.TitleContent>
        <TextBox PlaceholderText="Search…" Width="200" />
    </PleasantWindow.TitleContent>
    <!-- content -->
</PleasantWindow>
```

## macOS caption buttons

Control whether to override native macOS caption buttons:

```xml
<PleasantWindow OverrideMacOSCaption="True" />
```

When `true`, the custom caption buttons are used on macOS. When `false`, the system traffic light buttons are shown.

## Button visibility

Control individual caption button visibility:

```xml
<PleasantWindow IsCloseButtonVisible="True"
              IsMinimizeButtonVisible="True"
              IsRestoreButtonVisible="True"
              IsFullScreenButtonVisible="False" />
```

These properties provide fine-grained control over which caption buttons are displayed, independent of the `CaptionButtons` property.

## Splash screen

Implement `IPleasantSplashScreen` and assign it before the window is shown:

```csharp
public class MySplash : IPleasantSplashScreen
{
    public string? AppName => "My App";
    public IImage? AppIcon => null;
    public object? SplashScreenContent => null; // or a custom UserControl
    public IBrush? Background => null;           // null = window background
    public int MinimumShowTime => 1500;          // ms

    public async Task RunTasks(CancellationToken ct)
    {
        await LoadSettingsAsync(ct);
        await PreloadDataAsync(ct);
    }
}

public MainWindow()
{
    InitializeComponent();
    SplashScreen = new MySplash();
}
```

For fully custom splash content, return a `Control` from `SplashScreenContent`:

```csharp
public object? SplashScreenContent => new MySplashView();
```
