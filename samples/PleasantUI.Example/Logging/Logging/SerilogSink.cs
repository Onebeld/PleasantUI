using System.Text;
using Avalonia;
using Avalonia.Logging;
using Avalonia.Utilities;

namespace PleasantUI.Example.Logging.Logging;

/// <summary>
/// Represents a Serilog sink that logs messages based on a minimum log level and specific areas.
/// </summary>
/// <param name="minimumLevel">The minimum level of log messages to be logged.</param>
/// <param name="areas">The areas to log, or null to log all areas.</param>
public class SerilogSink(LogEventLevel minimumLevel, IList<string>? areas = null) : ILogSink
{
	private readonly IList<string>? _areas = areas?.Count > 0 ? areas : null;

	/// <summary>
	/// Determines if the sink is enabled for the specified log level and area.
	/// </summary>
	/// <param name="level">The level of the log message.</param>
	/// <param name="area">The area of the log message.</param>
	/// <returns>
	/// <see langword="true"/> if the sink is enabled for the specified log level and area; otherwise,
	/// <see langword="false"/>.
	/// </returns>
	public bool IsEnabled(LogEventLevel level, string area)
	{
		return level >= minimumLevel && (_areas?.Contains(area) ?? true);
	}

	/// <summary>
	/// Logs a message with the specified level and area.
	/// </summary>
	/// <param name="level">The level of the log message.</param>
	/// <param name="area">The area of the log message.</param>
	/// <param name="source">The source of the log message.</param>
	/// <param name="messageTemplate">The message template to log.</param>
	public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
	{
		if (IsEnabled(level, area))
		{
			switch (level)
			{
				case LogEventLevel.Verbose:
					Serilog.Log.Verbose(Format(area, messageTemplate, source, null));
					break;
				case LogEventLevel.Debug:
					Serilog.Log.Debug(Format(area, messageTemplate, source, null));
					break;
				case LogEventLevel.Information:
					Serilog.Log.Information(Format(area, messageTemplate, source, null));
					break;
				case LogEventLevel.Warning:
					Serilog.Log.Warning(Format(area, messageTemplate, source, null));
					break;
				case LogEventLevel.Error:
					Serilog.Log.Error(Format(area, messageTemplate, source, null));
					break;
				case LogEventLevel.Fatal:
					Serilog.Log.Fatal(Format(area, messageTemplate, source, null));
					break;
				
				default:
					throw new ArgumentOutOfRangeException(nameof(level), level, null);
			}
		}
	}

	/// <summary>
	/// Logs a message to Serilog with the specified log level, area, source, and message template.
	/// </summary>
	/// <param name="level">The severity level of the log message.</param>
	/// <param name="area">The area or category of the log message.</param>
	/// <param name="source">The source object related to the log message.</param>
	/// <param name="messageTemplate">The message template to be logged.</param>
	/// <param name="propertyValues">Optional property values to be used in the message template.</param>
	/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the log level is out of the defined range.</exception>
	public void Log(LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
	{
		if (IsEnabled(level, area))
		{
			switch (level)
			{
				case LogEventLevel.Verbose:
					Serilog.Log.Verbose(Format(area, messageTemplate, source, propertyValues));
					break;
				case LogEventLevel.Debug:
					Serilog.Log.Debug(Format(area, messageTemplate, source, propertyValues));
					break;
				case LogEventLevel.Information:
					Serilog.Log.Information(Format(area, messageTemplate, source, propertyValues));
					break;
				case LogEventLevel.Warning:
					Serilog.Log.Warning(Format(area, messageTemplate, source, propertyValues));
					break;
				case LogEventLevel.Error:
					Serilog.Log.Error(Format(area, messageTemplate, source, propertyValues));
					break;
				case LogEventLevel.Fatal:
					Serilog.Log.Fatal(Format(area, messageTemplate, source, propertyValues));
					break;
				
				default:
					throw new ArgumentOutOfRangeException(nameof(level), level, null);
			}
		}
	}
	
	private static string Format(
		string area,
		string template,
		object? source,
		object?[]? values)
	{
		StringBuilder result = new();
		CharacterReader r = new(template.AsSpan());
		int i = 0;

		result.Append('[');
		result.Append(area);
		result.Append("] ");

		while (!r.End)
		{
			char c = r.Take();

			if (c != '{')
			{
				result.Append(c);
			}
			else
			{
				if (r.Peek != '{')
				{
					result.Append('\'');
					result.Append(values?[i++]);
					result.Append('\'');
					r.TakeUntil('}');
					r.Take();
				}
				else
				{
					result.Append('{');
					r.Take();
				}
			}
		}

		FormatSource(source, result);
		return result.ToString();
	}
	
	private static void FormatSource(object? source, StringBuilder result)
	{
		if (source is null)
			return;

		result.Append(" (");
		result.Append(source.GetType().Name);
		result.Append(" #");

		if (source is StyledElement { Name: not null } se)
			result.Append(se.Name);
		else
			result.Append(source.GetHashCode());

		result.Append(')');
	}
}