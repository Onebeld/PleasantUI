using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace PleasantUI.Core.Extensions.Markup;

/// <summary>
/// A markup extension that converts a string representation of a key gesture into a <see cref="KeyGesture" />.
/// </summary>
public class KeyGestureExtension : MarkupExtension
{
    private readonly string _keyGesture;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyGestureExtension" /> class.
    /// </summary>
    /// <param name="keyGesture">The string representation of the key gesture.</param>
    public KeyGestureExtension(string keyGesture)
    {
        _keyGesture = keyGesture;
    }

    /// <summary>
    /// Provides the value of the markup extension.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>The string representation of the parsed <see cref="KeyGesture" />.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return KeyGesture.Parse(_keyGesture).ToString();
    }
}