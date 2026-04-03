using System.Globalization;
using Avalonia.Data.Converters;

namespace PleasantUI.Converters;

/// <summary>
/// A multi-value converter that converts a text string to a password character
/// when a password char is provided and the text is not empty.
/// </summary>
public class StringToPasswordCharConverter : IMultiValueConverter
{
    /// <summary>
    /// Converts the text and password char to the display string.
    /// </summary>
    /// <param name="values">Array containing [text, passwordChar]</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="culture">The culture to use in the conversion.</param>
    /// <returns>The password char if set and text is not empty, otherwise the original text.</returns>
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2)
            return string.Empty;

        string? text = values[0] as string;
        char passwordChar = values[1] is char c ? c : '\0';

        // If text is empty or null, return empty string
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        // If password char is set, return it; otherwise return the text
        return passwordChar != '\0' ? passwordChar.ToString() : text;
    }
}
