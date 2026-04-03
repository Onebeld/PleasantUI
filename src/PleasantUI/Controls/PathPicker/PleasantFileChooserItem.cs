namespace PleasantUI.Controls;

/// <summary>
/// Represents a single entry (file or directory) in the <see cref="PleasantFileChooserViewModel"/>.
/// </summary>
public sealed class PleasantFileChooserItem
{
    /// <summary>Gets the full path of this entry.</summary>
    public string FullPath { get; }

    /// <summary>Gets the display name (file/folder name only).</summary>
    public string Name { get; }

    /// <summary>Gets whether this entry is a directory.</summary>
    public bool IsDirectory { get; }

    /// <summary>Gets the last-write time, or null if unavailable.</summary>
    public DateTime? Modified { get; }

    /// <summary>Gets the file size in bytes, or null for directories.</summary>
    public long? SizeBytes { get; }

    /// <summary>Gets a human-readable file size string.</summary>
    public string SizeText => SizeBytes is null ? string.Empty : FormatSize(SizeBytes.Value);

    /// <summary>Gets the file extension (e.g. ".txt"), or empty for directories.</summary>
    public string Extension { get; }

    /// <summary>Gets the icon key: "folder", "file", or "drive".</summary>
    public string IconKey { get; }

    public PleasantFileChooserItem(string fullPath, bool isDirectory, DateTime? modified = null, long? sizeBytes = null)
    {
        FullPath    = fullPath;
        Name        = Path.GetFileName(fullPath);
        if (string.IsNullOrEmpty(Name)) Name = fullPath; // drive roots like "C:\"
        IsDirectory = isDirectory;
        Modified    = modified;
        SizeBytes   = isDirectory ? null : sizeBytes;
        Extension   = isDirectory ? string.Empty : Path.GetExtension(fullPath).ToLowerInvariant();
        IconKey     = isDirectory ? (string.IsNullOrEmpty(Path.GetFileName(fullPath)) ? "drive" : "folder") : "file";
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024)        return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024L * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    }
}
