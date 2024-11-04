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
        List<object> list = new(values!);

        string key = (list[0] as string)!;
        
        if (!string.IsNullOrWhiteSpace(_context))
            key = $"{_context}/{key}";

        list.RemoveAt(0);

        try
        {
            return Localizer.Tr(key, args: list.ToArray());
        }
        catch
        {
            return Localizer.Tr(key);
        }
    }
}