using Avalonia;
using Avalonia.Win32;

namespace PleasantUI.Example.Desktop;

class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
    {
        AppBuilder appBuilder = AppBuilder.Configure<App>();
        appBuilder.UseSkia();

#if Windows
        appBuilder.UseWin32()
                  .With(new AngleOptions
                  {
                      AllowedPlatformApis = new List<AngleOptions.PlatformApi>
                      {
                          AngleOptions.PlatformApi.DirectX11
                      }
                  });
#elif Linux
        appBuilder.UseX11();
#elif OSX
        appBuilder.UseAvaloniaNative();
#endif

        appBuilder
#if Windows
            .With(new Win32PlatformOptions
            {
                OverlayPopups = true
            });
#endif
#if OSX
            .With(new MacOSPlatformOptions()
            {
                DisableDefaultApplicationMenuItems = true,
                ShowInDock = false,
                DisableNativeMenus = true
            });
#endif
#if Linux
            .With(new X11PlatformOptions()
            {
                OverlayPopups = true
            });
#endif

#if DEBUG
        appBuilder.LogToTrace();
#endif

        return appBuilder;
    }
}