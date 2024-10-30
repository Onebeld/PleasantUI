using System.Diagnostics;
using System.Runtime.ExceptionServices;
using Serilog;

namespace PleasantUI.Core.Logging;

/// <summary>
/// A logger that handles unhandled exceptions and first chance exceptions.
/// </summary>
public class PleasantLogger : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PleasantLogger"/> class.
    /// </summary>
    /// <param name="loggerConfiguration">The logger configuration.</param>
    public PleasantLogger(LoggerConfiguration loggerConfiguration)
    {
        Log.Logger = loggerConfiguration.CreateLogger();
        
        AppDomain.CurrentDomain.FirstChanceException += CurrentDomainOnFirstChanceException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
        
        Log.Information("The logger has been initialized");
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Log.Information("The logger has been disposed");
        
        AppDomain.CurrentDomain.FirstChanceException -= CurrentDomainOnFirstChanceException;
        AppDomain.CurrentDomain.UnhandledException -= CurrentDomainOnUnhandledException;
        
        Log.CloseAndFlush();
    }
    
    private static void CurrentDomainOnFirstChanceException(object? sender, FirstChanceExceptionEventArgs e)
    {
        StackTrace stackTrace = new(1, true);
        Log.Error($"An handled exception occurred\n{e.Exception.GetType()}: {e.Exception.Message}\n{stackTrace}");
    }

    private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex) 
            Log.Fatal(ex, "An unhandled exception occurred");
        
        Log.CloseAndFlush();
    }
}