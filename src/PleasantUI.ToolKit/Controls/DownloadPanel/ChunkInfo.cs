using Avalonia;
using Avalonia.Media;

namespace PleasantUI.ToolKit.Controls;

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

    /// <summary>Gets or sets a status/info string for this chunk.</summary>
    public string? Info { get; set; }

    /// <summary>Gets or sets an optional override brush for this chunk's progress bar.</summary>
    public IBrush? ProgressBrush { get; set; }

    public override string ToString()
    {
        return Info ?? DownloadedSize ?? $"Chunk {Index}";
    }
}
