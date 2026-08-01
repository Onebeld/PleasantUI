using System.Text;
using Avalonia;
using Avalonia.Logging;
using PleasantUI.Example.Logging.Logging;
using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;

namespace PleasantUI.Example.Desktop;

class Program
{
    private static readonly string PathToLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
    private static readonly string FileName = Path.Combine(PathToLog,
        $"{AppDomain.CurrentDomain.FriendlyName}_{DateTime.Now:dd.MM.yyyy}.log");
    
    [STAThread]
    public static void Main(string[] args)
    {
        if (!Directory.Exists(PathToLog))
            Directory.CreateDirectory(PathToLog);
        
        bool createLogFile = !File.Exists(FileName);
        
        var expressionTemplate = new ExpressionTemplate(
            "[{@t:HH:mm:ss} {@l}]" +
            "{#if Area is not null} [{Area}]{#end}" +
            "{#if @p.SourceContext is not null} ({@p.SourceContext}){#end}" +
            " {@m}\n" +
            "{@x}",
            theme: TemplateTheme.Code
        );
        
        using PleasantLogger logger = new(
            new LoggerConfiguration()
                .WriteTo.File(FileName, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level}] | {Message:lj}{NewLine}{Exception}")
                .WriteTo.Console(expressionTemplate)
        );
        
        if (createLogFile)
            WriteDeviceInformation();

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    private static void WriteDeviceInformation()
    {
        StringBuilder stringBuilder = new();

        stringBuilder.AppendLine($"OS: {Environment.OSVersion}");
        stringBuilder.AppendLine($"CPU: {Environment.ProcessorCount} cores");
        
        Log.Information("Device information:\n{StringBuilder}", stringBuilder);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        AppBuilder appBuilder = AppBuilder.Configure<App>();
        appBuilder.UsePlatformDetect();

        appBuilder
            .With(new Win32PlatformOptions
            {
                OverlayPopups = false
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

        appBuilder.LogToSerilog(LogEventLevel.Debug, LogArea.Binding,
            LogArea.Property,
            LogArea.Animations,
            LogArea.Control,
            LogArea.Fonts,
            LogArea.Platform);

        return appBuilder;
    }
}