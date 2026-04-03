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
| `TitleBarType` | `Classic` / `ClassicExtended` | `Classic` | `ClassicExtended` gives a taller title bar with more room for custom content |
| `EnableCustomTitleBar` | `bool` | `true` | Replaces the OS title bar with the Fluent one |
| `ExtendsContentIntoTitleBar` | `bool` | `false` | Lets window content render behind the title bar area |
| `Subtitle` | `string` | — | Secondary text shown next to the title |
| `DisplayIcon` | `object` | — | Custom icon in the title bar (e.g. `DrawingImage`, `PathIcon`) |
| `DisplayTitle` | `object` | — | Replaces the text title with arbitrary content |
| `LeftTitleBarContent` | `object` | — | Content injected to the left of the title |
| `TitleContent` | `object` | — | Content placed in the center of the title bar |
| `CaptionButtons` | enum | `All` | Which caption buttons to show |
| `EnableBlur` | `bool` | from settings | Acrylic/blur window background |
| `SplashScreen` | `IPleasantSplashScreen` | `null` | Splash screen shown on startup |

## Extended title bar with custom content

```xml
<PleasantWindow TitleBarType="ClassicExtended"
                ExtendsContentIntoTitleBar="True"
                DisplayIcon="{StaticResource MyLogo}"
                Subtitle="v2.0">
    <PleasantWindow.TitleContent>
        <TextBox PlaceholderText="Search…" Width="200" />
    </PleasantWindow.TitleContent>
    <!-- content -->
</PleasantWindow>
```

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
