using Avalonia.Data.Converters;
using Avalonia.Media;
using PleasantUI.Controls;

namespace PleasantUI.Converters;

/// <summary>
/// Converts a LogLevel value to the appropriate background brush.
/// </summary>
public static class LogLevelToBrushConverter
{
    /// <summary>
    /// Converts a LogLevel to a brush.
    /// </summary>
    public static readonly IValueConverter LevelToBrush =
        new FuncValueConverter<LogLevel, IBrush>(level => level switch
        {
            LogLevel.Debug => new SolidColorBrush(Color.Parse("#323232")),
            LogLevel.Information => new SolidColorBrush(Color.Parse("#0078D4")),
            LogLevel.Warning => new SolidColorBrush(Color.Parse("#FF9C00")),
            LogLevel.Error => new SolidColorBrush(Color.Parse("#E81123")),
            _ => new SolidColorBrush(Color.Parse("#323232"))
        });
}
