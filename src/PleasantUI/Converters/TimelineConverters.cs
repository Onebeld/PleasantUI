using System.Globalization;
using Avalonia.Data.Converters;

namespace PleasantUI.Converters;

/// <summary>
/// Converters used by the <see cref="PleasantUI.Controls.Timeline"/> control theme.
/// </summary>
public static class TimelineConverters
{
    /// <summary>
    /// Formats a <see cref="DateTime"/> using a format string.
    /// Expects two bindings: [0] DateTime, [1] string format.
    /// Returns an empty string when inputs are invalid.
    /// </summary>
    public static readonly IMultiValueConverter DateTimeFormat =
        new FuncMultiValueConverter<object?, string>(values =>
        {
            if (values.Count >= 2 && values[0] is DateTime date && values[1] is string fmt)
                return date.ToString(fmt, CultureInfo.CurrentCulture);
            return string.Empty;
        });
}
