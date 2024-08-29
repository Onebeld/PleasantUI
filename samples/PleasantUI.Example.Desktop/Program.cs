using System.Runtime.ExceptionServices;
using Avalonia;
using Serilog;

namespace PleasantUI.Example.Desktop;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        InitializeLogger();
        
        Log.Information("Starting application");
        
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
        
        Log.Information("The program has completed its work");
        
        Log.CloseAndFlush();
    }

    private static void InitializeLogger()
    {
        string pathToLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        
        if (!Directory.Exists(pathToLog))
            Directory.CreateDirectory(pathToLog);

        string fileName = Path.Combine(pathToLog,
            $"{AppDomain.CurrentDomain.FriendlyName}_{DateTime.Now:dd.MM.yyyy}.log");

        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(fileName, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level}] | {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
        
        AppDomain.CurrentDomain.FirstChanceException += CurrentDomainOnFirstChanceException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
    }

    private static void CurrentDomainOnFirstChanceException(object? sender, FirstChanceExceptionEventArgs e)
    {
        Log.Error(e.Exception, "A handled exception occurred");
    }

    private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex) 
            Log.Fatal(ex, "An unhandled exception occurred");
        
        Log.CloseAndFlush();
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        AppBuilder appBuilder = AppBuilder.Configure<App>();
        appBuilder.UsePlatformDetect();

        appBuilder
            .With(new Win32PlatformOptions
            {
                OverlayPopups = true
            })
            .With(new MacOSPlatformOptions
            {
                DisableDefaultApplicationMenuItems = true,
                ShowInDock = false,
                DisableNativeMenus = true
            })
            .With(new X11PlatformOptions
            {
                OverlayPopups = true
            });

        appBuilder.LogToSerilog();

        return appBuilder;
    }
}