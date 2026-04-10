using Avalonia;
using Avalonia.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PleasantUI.ToolKit.Controls;

/// <summary>
/// Represents the status of a download chunk.
/// </summary>
public enum ChunkStatus
{
    /// <summary>The chunk is waiting to start downloading.</summary>
    Waiting,
    /// <summary>The chunk is currently downloading.</summary>
    Downloading,
    /// <summary>The chunk download has completed successfully.</summary>
    Completed,
    /// <summary>The chunk download has failed.</summary>
    Error,
    /// <summary>The chunk download is paused.</summary>
    Paused
}

/// <summary>
/// Describes a single download chunk for visualization in a <see cref="DownloadPanel"/>.
/// </summary>
public class ChunkInfo : INotifyPropertyChanged
{
    private int _index;
    private double _start;
    private double _end;
    private double _progress;
    private long _totalSizeBytes;
    private long _downloadedBytes;
    private double _speedBytesPerSecond;
    private double _timeRemainingSeconds;
    private ChunkStatus _status;

    private string _waitingText = "Waiting";
    private string _queuedText = "Queued";
    private string _startingText = "Starting";
    private string _doneText = "Done";
    private string _failedText = "Failed";
    private string _pausedText = "Paused";
    private string _unknownText = "Unknown";

    /// <summary>Gets or sets the 1-based chunk index.</summary>
    public int Index
    {
        get => _index;
        set => SetProperty(ref _index, value);
    }

    /// <summary>Gets or sets the normalized start position [0..1].</summary>
    public double Start
    {
        get => _start;
        set => SetProperty(ref _start, value);
    }

    /// <summary>Gets or sets the normalized end position [0..1].</summary>
    public double End
    {
        get => _end;
        set => SetProperty(ref _end, value);
    }

    /// <summary>Gets or sets the normalized download progress within this chunk [0..1].</summary>
    public double Progress
    {
        get => _progress;
        set
        {
            if (SetProperty(ref _progress, value))
            {
                OnPropertyChanged(nameof(DownloadedSize));
                OnPropertyChanged(nameof(TotalSize));
                OnPropertyChanged(nameof(Info));
            }
        }
    }

    /// <summary>Gets or sets the total size of this chunk in bytes.</summary>
    public long TotalSizeBytes
    {
        get => _totalSizeBytes;
        set
        {
            if (SetProperty(ref _totalSizeBytes, value))
            {
                OnPropertyChanged(nameof(TotalSize));
            }
        }
    }

    /// <summary>Gets or sets the amount downloaded in bytes.</summary>
    public long DownloadedBytes
    {
        get => _downloadedBytes;
        set
        {
            if (SetProperty(ref _downloadedBytes, value))
            {
                OnPropertyChanged(nameof(DownloadedSize));
                OnPropertyChanged(nameof(Info));
            }
        }
    }

    /// <summary>Gets or sets the current download speed in bytes per second.</summary>
    public double SpeedBytesPerSecond
    {
        get => _speedBytesPerSecond;
        set
        {
            if (SetProperty(ref _speedBytesPerSecond, value))
            {
                OnPropertyChanged(nameof(Speed));
            }
        }
    }

    /// <summary>Gets or sets the estimated time remaining in seconds.</summary>
    public double TimeRemainingSeconds
    {
        get => _timeRemainingSeconds;
        set
        {
            if (SetProperty(ref _timeRemainingSeconds, value))
            {
                OnPropertyChanged(nameof(TimeRemaining));
            }
        }
    }

    /// <summary>Gets or sets the status of this chunk.</summary>
    public ChunkStatus Status
    {
        get => _status;
        set
        {
            if (SetProperty(ref _status, value))
            {
                OnPropertyChanged(nameof(Info));
            }
        }
    }

    /// <summary>Gets or sets an optional override brush for this chunk's progress bar.</summary>
    public IBrush? ProgressBrush { get; set; }

    /// <summary>Gets or sets the text for Waiting status.</summary>
    public string WaitingText
    {
        get => _waitingText;
        set { if (SetProperty(ref _waitingText, value)) OnPropertyChanged(nameof(Info)); }
    }

    /// <summary>Gets or sets the text for Queued status.</summary>
    public string QueuedText
    {
        get => _queuedText;
        set { if (SetProperty(ref _queuedText, value)) OnPropertyChanged(nameof(Info)); }
    }

    /// <summary>Gets or sets the text for Starting status.</summary>
    public string StartingText
    {
        get => _startingText;
        set { if (SetProperty(ref _startingText, value)) OnPropertyChanged(nameof(Info)); }
    }

    /// <summary>Gets or sets the text for Done status.</summary>
    public string DoneText
    {
        get => _doneText;
        set { if (SetProperty(ref _doneText, value)) OnPropertyChanged(nameof(Info)); }
    }

    /// <summary>Gets or sets the text for Failed status.</summary>
    public string FailedText
    {
        get => _failedText;
        set { if (SetProperty(ref _failedText, value)) OnPropertyChanged(nameof(Info)); }
    }

    /// <summary>Gets or sets the text for Paused status.</summary>
    public string PausedText
    {
        get => _pausedText;
        set { if (SetProperty(ref _pausedText, value)) OnPropertyChanged(nameof(Info)); }
    }

    /// <summary>Gets or sets the text for Unknown status.</summary>
    public string UnknownText
    {
        get => _unknownText;
        set { if (SetProperty(ref _unknownText, value)) OnPropertyChanged(nameof(Info)); }
    }

    /// <summary>Gets the downloaded size as a formatted string.</summary>
    public string DownloadedSize => FormatBytes(_downloadedBytes);

    /// <summary>Gets the total size as a formatted string.</summary>
    public string TotalSize => FormatBytes(_totalSizeBytes);

    /// <summary>Gets the speed as a formatted string.</summary>
    public string Speed => FormatSpeed(_speedBytesPerSecond);

    /// <summary>Gets the time remaining as a formatted string.</summary>
    public string TimeRemaining => FormatTime(_timeRemainingSeconds);

    /// <summary>Gets a computed info string based on status and progress.</summary>
    public string Info => _status switch
    {
        ChunkStatus.Waiting when _progress == 0 => _waitingText,
        ChunkStatus.Waiting => _queuedText,
        ChunkStatus.Downloading when _progress > 0 => $"{_progress:P0}",
        ChunkStatus.Downloading => _startingText,
        ChunkStatus.Completed => _doneText,
        ChunkStatus.Error => _failedText,
        ChunkStatus.Paused when _progress > 0 => $"{_progress:P0}",
        ChunkStatus.Paused => _pausedText,
        _ => _unknownText
    };

    /// <summary>Formats bytes to a human-readable string.</summary>
    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F1} MB";
        return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
    }

    /// <summary>Formats speed to a human-readable string.</summary>
    private static string FormatSpeed(double bytesPerSecond)
    {
        if (bytesPerSecond < 1024) return $"{bytesPerSecond:F0} B/s";
        if (bytesPerSecond < 1024 * 1024) return $"{bytesPerSecond / 1024.0:F1} KB/s";
        if (bytesPerSecond < 1024 * 1024 * 1024) return $"{bytesPerSecond / (1024.0 * 1024.0):F1} MB/s";
        return $"{bytesPerSecond / (1024.0 * 1024.0 * 1024.0):F2} GB/s";
    }

    /// <summary>Formats time to a human-readable string.</summary>
    private static string FormatTime(double seconds)
    {
        if (seconds <= 0 || double.IsInfinity(seconds)) return "–";
        if (seconds < 60) return $"{seconds:F0}s";
        if (seconds < 3600) return $"{seconds / 60:F0}m";
        return $"{seconds / 3600:F1}h";
    }

    public override string ToString()
    {
        return $"{DownloadedSize} / {TotalSize}";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
