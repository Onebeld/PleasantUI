using Avalonia;
using Avalonia.Headless;

namespace PleasantUI.Tests;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<Application>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions
            {
                UseHeadlessDrawing = true
            })
            .AfterSetup(builder =>
            {
                builder.Instance?.Styles.Add(new PleasantTheme());
            })
            .LogToTrace();
}