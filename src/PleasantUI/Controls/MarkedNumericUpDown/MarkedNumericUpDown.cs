using Avalonia;
using Avalonia.Controls;

namespace PleasantUI.Controls;

/// <summary>
/// Represents a numeric up-down control with an associated symbol.
/// </summary>
public class MarkedNumericUpDown : NumericUpDown
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