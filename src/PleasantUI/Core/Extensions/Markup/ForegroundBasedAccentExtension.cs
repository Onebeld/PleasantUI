using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using PleasantUI.Converters;

namespace PleasantUI.Core.Extensions.Markup;

public class ForegroundBasedAccentExtension : MarkupExtension
{
    public object? ResourceKey { get; set; }
    
    public ForegroundBasedAccentExtension()
    {
    }

    public ForegroundBasedAccentExtension(object resourceKey)
    {
        ResourceKey = resourceKey;
    }
    
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        DynamicResourceExtension extension = new(ResourceKey);

        MultiBinding multiBinding = new()
        {
            Bindings = { extension },
            Converter = new ForegroundBasedAccentConverter()
        };

        return multiBinding;
    }
}