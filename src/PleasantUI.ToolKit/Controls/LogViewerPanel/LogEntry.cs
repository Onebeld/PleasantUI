namespace PleasantUI.ToolKit.Controls;

/// <summary>
/// Severity level of a <see cref="LogEntry"/>.
/// </summary>
public enum LogLevel
{
    /// <summary>Verbose diagnostic information.</summary>
    Debug,
    /// <summary>General informational message.</summary>
    Information,
    /// <summary>Non-critical issue that may require attention.</summary>
    Warning,
    /// <summary>Error that caused an operation to fail.</summary>
    Error
}

/// <summary>
/// Represents a single log entry displayed in a <see cref="LogViewerPanel"/>.
/// </summary>
public class LogEntry
{
    /// <summary>Gets or sets when the entry was recorded.</summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>Gets or sets the severity level.</summary>
    public LogLevel Level { get; set; } = LogLevel.Information;

    /// <summary>Gets or sets the source/category of the log entry.</summary>
    public string? Source { get; set; }

    /// <summary>Gets or sets the primary message text.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets optional detail text (e.g. stack trace).</summary>
    public string? Details { get; set; }
}
