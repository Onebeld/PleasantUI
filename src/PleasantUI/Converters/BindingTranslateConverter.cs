using System.Globalization;
using Avalonia.Data.Converters;
using PleasantUI.Core.Localization;

namespace PleasantUI.Converters;

/// <summary>
/// A converter that translates a binding key and additional arguments into a localized string.
/// </summary>
public class BindingTranslateConverter : IMultiValueConverter
{
    private readonly string? _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="BindingTranslateConverter"/> class.
    /// </summary>
    /// <param name="context">The context for the translation, which will be used to construct the localization key.</param>
    public BindingTranslateConverter(string? context)
    {
        _context = context;
    }
    
    /// <inheritdoc />
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is null || values.Count == 0) return string.Empty;

        string key = (values[0] as string) ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(_context))
            key = $"{_context}/{key}";

        // values[1] is the language-trigger (ignored as a value, used only to force re-evaluation)
        // values[2..] are optional format args
        List<object> args = new();
        for (int i = 2; i < values.Count; i++)
            if (values[i] is not null)
                args.Add(values[i]!);

        try
        {
            return Localizer.Tr(key, args: args.ToArray());
        }
        catch
        {
            return Localizer.Tr(key);
        }
    }
}