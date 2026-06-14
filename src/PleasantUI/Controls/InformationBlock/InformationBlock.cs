using Avalonia;
using Avalonia.Controls;

namespace PleasantUI.Controls;

/// <summary>
/// Represents a control that displays information with an associated icon.
/// </summary>
public class InformationBlock : ContentControl
{
    /// <summary>
    /// Defines the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<InformationBlock, object?>(nameof(Icon));

    /// <summary>
    /// Represents the icon associated with the count of items, indicating the number of items associated with that icon.
    /// </summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}