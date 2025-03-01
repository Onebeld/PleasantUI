using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using PleasantUI.Converters;

namespace PleasantUI.Core.Extensions.Markup;

/// <summary>
/// A markup extension that provides a foreground-based accent color using a resource key.
/// </summary>
public class ForegroundBasedAccentExtension : MarkupExtension
{
    /// <summary>
    /// Gets or sets the resource key used to retrieve the accent color.
    /// </summary>
    public object? ResourceKey { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForegroundBasedAccentExtension"/> class.
    /// </summary>
    public ForegroundBasedAccentExtension()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForegroundBasedAccentExtension"/> class with a specified resource key.
    /// </summary>
    /// <param name="resourceKey">The resource key used to retrieve the accent color.</param>
    public ForegroundBasedAccentExtension(object resourceKey)
    {
        ResourceKey = resourceKey ?? throw new ArgumentNullException(nameof(resourceKey), "ResourceKey cannot be null.");
    }

    /// <summary>
    /// Provides the value of the foreground-based accent color for the binding.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>A <see cref="MultiBinding"/> that applies the <see cref="ForegroundBasedAccentConverter"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="ResourceKey"/> is null.</exception>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (ResourceKey is null)
            throw new InvalidOperationException("ResourceKey cannot be null when creating DynamicResourceExtension.");

        DynamicResourceExtension extension = new(ResourceKey);

        MultiBinding multiBinding = new()
        {
            Bindings = { extension },
            Converter = new ForegroundBasedAccentConverter()
        };

        return multiBinding;
    }
}
