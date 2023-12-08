using System.Globalization;

namespace PleasantUI.Core;

public static class Globalization
{
    /// <summary>
    /// Represents the number formatting information used for formatting numeric values.
    /// </summary>
    public static readonly NumberFormatInfo NumberFormatInfo = new()
    {
        NumberDecimalDigits = 0
    };
}