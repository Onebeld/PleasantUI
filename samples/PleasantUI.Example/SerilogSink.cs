using System.Text;
using Avalonia;
using Avalonia.Logging;
using Avalonia.Utilities;

namespace PleasantUI.Example;

public class SerilogSink(LogEventLevel minimumLevel, IList<string>? areas = null) : ILogSink
{
	private readonly IList<string>? _areas = areas?.Count > 0 ? areas : null;

	public bool IsEnabled(LogEventLevel level, string area)
	{
		return level >= minimumLevel && (_areas?.Contains(area) ?? true);
	}

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