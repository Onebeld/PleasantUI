using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using PleasantUI.Converters;

namespace PleasantUI.Core.Extensions.Markup;

/// <summary>
/// A markup extension that provides a transparent color based on a specified resource key.
/// </summary>
public class ColorToTransparentExtension : MarkupExtension
{
    /// <summary>
    /// Gets or sets the resource key associated with the color to be converted to transparent.
    /// </summary>
    public object? ResourceKey { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorToTransparentExtension"/> class.
    /// </summary>
    public ColorToTransparentExtension()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorToTransparentExtension"/> class with a specified resource key.
    /// </summary>
    /// <param name="resourceKey">The resource key associated with the color.</param>
    public ColorToTransparentExtension(object resourceKey)
    {
        ResourceKey = resourceKey;
    }

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        // The animation does not work correctly if a normal color is converted to Transparent.
        // Since DynamicResource does not yet support converters,
        // we will use a workaround in the form of a custom MarkupExtension

        if (ResourceKey == null)
            throw new InvalidOperationException("ResourceKey cannot be null when creating DynamicResourceExtension.");

        DynamicResourceExtension extension = new(ResourceKey);


        MultiBinding multiBinding = new()
        {
            Bindings = { extension },
            Converter = new ColorToTransparentConverter()
        };

        return multiBinding;
    }
}