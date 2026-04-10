using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PleasantUI.ToolKit.Converters;

/// <summary>
/// Converts a ChunkStatus value to an appropriate background brush.
/// </summary>
public static class ChunkStatusToBrushConverter
{
    public static readonly IValueConverter Instance = new FuncValueConverter<ChunkStatus, IBrush>(status => status switch
    {
        ChunkStatus.Waiting => new SolidColorBrush(Color.Parse("#6B7280")),
        ChunkStatus.Downloading => new SolidColorBrush(Color.Parse("#0078D4")),
        ChunkStatus.Completed => new SolidColorBrush(Color.Parse("#107C10")),
        ChunkStatus.Error => new SolidColorBrush(Color.Parse("#E81123")),
        ChunkStatus.Paused => new SolidColorBrush(Color.Parse("#FF9C00")),
        _ => new SolidColorBrush(Color.Parse("#6B7280"))
    });
}

/// <summary>
/// Converts a ChunkStatus value to a speed display string.
/// </summary>
public static class ChunkStatusToSpeedConverter
{
    public static readonly IValueConverter Instance = new FuncValueConverter<ChunkStatus, string>(status => status switch
    {
        ChunkStatus.Waiting => "–",
        ChunkStatus.Downloading => "Active",
        ChunkStatus.Completed => "Done",
        ChunkStatus.Error => "Failed",
        ChunkStatus.Paused => "Paused",
        _ => "–"
    });
}
