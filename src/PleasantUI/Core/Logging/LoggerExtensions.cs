using Avalonia;
using Avalonia.Logging;

namespace PleasantUI.Core.Logging;

/// <summary>
/// Extensions for <see cref="AppBuilder"/> to log to Serilog.
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// Logs to Serilog with the given log level and areas.
    /// </summary>
    /// <param name="builder">The <see cref="AppBuilder"/>.</param>
    /// <param name="level">The level of the log messages to be written to Serilog.</param>
    /// <param name="areas">The areas to log.</param>
    /// <returns>The same instance of the <see cref="AppBuilder"/>.</returns>
    public static AppBuilder LogToSerilog(this AppBuilder builder, LogEventLevel level = LogEventLevel.Warning,
        params string[] areas)
    {
        Logger.Sink = new SerilogSink(level, areas);
        return builder;
    }
}