using System.Globalization;

namespace PleasantUI.Core;

/// <summary>
/// Presents settings related to globalization.
/// </summary>
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