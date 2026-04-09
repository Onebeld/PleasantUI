# PleasantSplashScreen

The overlay control rendered on top of `PleasantWindow` content during startup. Populated automatically from `IPleasantSplashScreen` — do not instantiate directly.

## Usage

The splash screen is automatically created and displayed by the `PleasantWindow` when an `IPleasantSplashScreen` is provided:

```csharp
public class MySplashScreen : IPleasantSplashScreen
{
    public IBrush? Background { get; } = new SolidColorBrush(Color.Parse("#2D2D2D"));
    public IImage? AppIcon { get; } = new Bitmap("Assets/icon.png");
    public string? AppName { get; } = "My Application";
    public object? SplashScreenContent { get; } = null;
}

// In your window
public class MyWindow : PleasantWindow
{
    public MyWindow()
    {
        SplashScreen = new MySplashScreen();
    }
}
```

## Properties

| Property | Type | Description |
|---|---|---|
| `SplashScreen` | `IPleasantSplashScreen?` | The `IPleasantSplashScreen` driving this control |
| `AppName` | `string?` | Application name displayed when no icon or custom content is set |
| `AppIcon` | `IImage?` | Image displayed in the splash screen |
| `SplashContent` | `object?` | Fully custom content — overrides icon and name |

## IPleasantSplashScreen interface

Implement this interface to provide splash screen data:

```csharp
public interface IPleasantSplashScreen
{
    IBrush? Background { get; }
    IImage? AppIcon { get; }
    string? AppName { get; }
    object? SplashScreenContent { get; }
}
```

## Custom content

Provide fully custom splash screen content:

```csharp
public class CustomSplashScreen : IPleasantSplashScreen
{
    public IBrush? Background { get; } = new SolidColorBrush(Colors.Blue);
    public IImage? AppIcon { get; } = null;
    public string? AppName { get; } = null;
    public object? SplashScreenContent { get; } = new StackPanel
    {
        Children =
        {
            new TextBlock { Text = "Loading...", FontSize = 24 },
            new ProgressBar { IsIndeterminate = true }
        }
    };
}
```

## Priority

The splash screen displays content in the following priority order:
1. `SplashScreenContent` (if not null)
2. `AppIcon` (if not null)
3. `AppName` (if not null)

## Background

The background is automatically set from the `IPleasantSplashScreen.Background` property:

```csharp
public class MySplashScreen : IPleasantSplashScreen
{
    public IBrush? Background { get; } = new SolidColorBrush(Color.Parse("#1A1A2E"));
}
```

## Notes

- This control is typically created and managed by `PleasantWindow`, not instantiated directly
- The control automatically populates its properties from the `IPleasantSplashScreen` when it changes
- Use `IPleasantSplashScreen` to define your splash screen appearance
