using Avalonia;
using Avalonia.Controls;

namespace PleasantUI.Controls.Docking;

public class NamedColumnDefinition : ColumnDefinition
{
    public static readonly StyledProperty<string?> NameProperty =
        AvaloniaProperty.Register<NamedColumnDefinition, string?>(nameof(Name));

    public string? Name
    {
        get => GetValue(NameProperty);
        set => SetValue(NameProperty, value);
    }
}
