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
    public BindingTranslateConverter(string? context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is null || values.Count == 0)
            return string.Empty;

        // values[0] = key (from the bound property, e.g. TitleKey)
        // values[1] = language trigger (ignored as value, forces re-evaluation on lang change)
        // values[2..] = optional format args

        // Skip if Avalonia hasn't provided a real value yet (UnsetValue sentinel)
        object? raw = values[0];
        if (raw is null || raw.GetType().Name == "UnsetValueType")
            return string.Empty;

        string key = (raw as string) ?? raw.ToString() ?? string.Empty;
        if (string.IsNullOrEmpty(key))
            return string.Empty;

        if (!string.IsNullOrWhiteSpace(_context))
            key = $"{_context}/{key}";

        // Collect format args from values[2..], skipping nulls and UnsetValue
        var args = new List<object>();
        for (int i = 2; i < values.Count; i++)
        {
            object? arg = values[i];
            if (arg is null || arg.GetType().Name == "UnsetValueType") continue;
            args.Add(arg);
        }

        try
        {
            string result = args.Count > 0
                ? Localizer.Tr(key, args: [.. args])
                : Localizer.Tr(key);

            System.Diagnostics.Debug.WriteLine($"[BindingTranslateConverter] key=\"{key}\" → \"{result}\" lang={Localizer.Instance.CurrentLanguage}");
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[BindingTranslateConverter] ERROR key=\"{key}\": {ex.Message}");
            return Localizer.Tr(key);
        }
    }
}
