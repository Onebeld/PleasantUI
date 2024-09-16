using System.Globalization;

namespace PleasantUI.Extensions;

/// <summary>
/// Provides extension methods to convert strings to numbers.
/// </summary>
public static class StringToNumberExtensions
{
    /// <summary>
    /// Converts the specified string representation of a number to its double-precision floating-point number equivalent.
    /// </summary>
    /// <param name="value">A string containing a number to convert.</param>
    /// <returns>
    /// A double-precision floating-point number that is equivalent to the numeric value or 1 if the conversion fails.
    /// </returns>
    public static double GetDouble(this string value)
    {
        // Try parsing in the current culture
        if (!double.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out double result) &&
            // Then try in US english
            !double.TryParse(value, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out result) &&
            // Then in neutral language
            !double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            result = 1;

        return result;
    }
}