using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using PleasantUI.Converters;

namespace PleasantUI.Extensions.Markup;

public class ColorToTransparentExtension : MarkupExtension
{
    public object? ResourceKey { get; set; }
    
    public ColorToTransparentExtension()
    {
    }

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
        
        DynamicResourceExtension extension = new(ResourceKey);

        MultiBinding multiBinding = new()
        {
            Bindings = { extension },
            Converter = new ColorToTransparentConverter()
        };

        return multiBinding;
    }
}