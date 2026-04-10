using Avalonia;
using Avalonia.Media;

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
public class ChunkInfo
{
    /// <summary>Gets or sets the 1-based chunk index.</summary>
    public int Index { get; set; }

    /// <summary>Gets or sets the normalized start position [0..1].</summary>
    public double Start { get; set; }

    /// <summary>Gets or sets the normalized end position [0..1].</summary>
    public double End { get; set; }

    /// <summary>Gets or sets the normalized download progress within this chunk [0..1].</summary>
    public double Progress { get; set; }

    /// <summary>Gets or sets the amount downloaded as a display string.</summary>
    public string? DownloadedSize { get; set; }

    /// <summary>Gets or sets the status of this chunk.</summary>
    public ChunkStatus Status { get; set; } = ChunkStatus.Waiting;

    /// <summary>Gets or sets an optional override brush for this chunk's progress bar.</summary>
    public IBrush? ProgressBrush { get; set; }

    /// <summary>Gets a computed info string based on status and progress.</summary>
    public string Info => Status switch
    {
        ChunkStatus.Waiting when Progress == 0 => "Waiting",
        ChunkStatus.Waiting => "Queued",
        ChunkStatus.Downloading => $"Downloading {Progress:P0}",
        ChunkStatus.Completed => "Completed",
        ChunkStatus.Error => "Failed",
        ChunkStatus.Paused when Progress > 0 => $"Paused {Progress:P0}",
        ChunkStatus.Paused => "Paused",
        _ => "Unknown"
    };

    public override string ToString()
    {
        return DownloadedSize ?? Info;
    }
}
