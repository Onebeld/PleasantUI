using System.Globalization;
using Avalonia.Data.Converters;

namespace PleasantUI.Converters;

/// <summary>
/// A multi-value converter that translates a string using string.Format.
/// The first value in the values list is the translation text, and the remaining values are the arguments to be used
/// in the format string.
/// </summary>
public class TranslateConverter : IMultiValueConverter
{
    /// <summary>
    /// Converts a list of values to a translated string.
    /// </summary>
    /// <param name="values">
    /// A list of values. The first value is the translation text, and the remaining values are the
    /// arguments to be used in the format string.
    /// </param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="culture">The culture to use in the conversion.</param>
    /// <returns>The translated string, or the original translation text if an error occurs during formatting.</returns>
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        List<object> list = new(values!);

        string translationText = (list[0] as string)!;

        list.RemoveAt(0);

        try
        {
            return string.Format(translationText, list.ToArray());
        }
        catch
        {
            return translationText;
        }
    }
}