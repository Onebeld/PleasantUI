using Avalonia;
using Avalonia.Controls;

namespace PleasantUI.Controls;

/// <summary>
/// Represents a text box control with an associated symbol that indicates the type of input expected from the user.
/// </summary>
public class MarkedTextBox : TextBox
{
    /// <summary>
    /// Defines the <see cref="Mark" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> MarkProperty =
        AvaloniaProperty.Register<MarkedTextBox, object?>(nameof(Mark));

    /// <summary>
    /// It determines the mark, indicating the type of input expected from the user.
    /// </summary>
    public object? Mark
    {
        get => GetValue(MarkProperty);
        set => SetValue(MarkProperty, value);
    }
}