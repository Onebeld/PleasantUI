using Avalonia.Data;
using Avalonia.Data.Converters;
using System.Globalization;

namespace PleasantUI.Converters;

/// <summary>
/// Calculates the progress bar indicator width/height based on the parent container size and progress value.
/// </summary>
public class ProgressBarIndicatorConverter : IMultiValueConverter
{
    public static readonly ProgressBarIndicatorConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 3)
            return 0.0;

        if (values[0] is not double containerSize || values[1] is not double value || values[2] is not double maximum)
            return 0.0;

        if (maximum == 0)
            return 0.0;

        double ratio = value / maximum;
        return containerSize * ratio;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
