using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace PleasantUI.Controls;

public class PleasantIcon : TemplatedControl
{
    public static readonly StyledProperty<IDataTemplate?> IconTemplateProperty =
        AvaloniaProperty.Register<PleasantIcon, IDataTemplate?>(
            nameof(IconTemplate));
    
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<PleasantIcon, object?>(nameof(Icon));

    public IDataTemplate? IconTemplate
    {
        get => GetValue(IconTemplateProperty);
        set => SetValue(IconTemplateProperty, value);
    }

    [Content]
    [DependsOn(nameof(IconTemplate))]
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public PleasantIcon()
    {
    }
}