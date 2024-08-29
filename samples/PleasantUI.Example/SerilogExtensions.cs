using Avalonia;
using Avalonia.Logging;

namespace PleasantUI.Example;

public static class SerilogExtensions
{
	public static AppBuilder LogToSerilog(this AppBuilder builder, LogEventLevel level = LogEventLevel.Warning,
		params string[] areas)
	{
		Logger.Sink = new SerilogSink(level, areas);
		return builder;
	}
}