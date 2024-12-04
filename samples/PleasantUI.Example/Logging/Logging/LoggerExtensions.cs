using Avalonia;
using Avalonia.Logging;

namespace PleasantUI.Example.Logging.Logging;

/// <summary>
/// Extensions for <see cref="Avalonia.AppBuilder"/> to log to Serilog.
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// Logs to Serilog with the given log level and areas.
    /// </summary>
    /// <param name="builder">The <see cref="Avalonia.AppBuilder"/>.</param>
    /// <param name="level">The level of the log messages to be written to Serilog.</param>
    /// <param name="areas">The areas to log.</param>
    /// <returns>The same instance of the <see cref="Avalonia.AppBuilder"/>.</returns>
    public static AppBuilder LogToSerilog(this AppBuilder builder, LogEventLevel level = LogEventLevel.Warning,
        params string[] areas)
    {
        Avalonia.Logging.Logger.Sink = new SerilogSink(level, areas);
        return builder;
    }
}