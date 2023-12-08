using Avalonia;
using Avalonia.Controls;

namespace PleasantUI.Controls;

/// <summary>
/// Represents a numeric up-down control with an associated symbol.
/// </summary>
public class NumericUpDownWithSymbol : NumericUpDown
{
    public static readonly StyledProperty<object?> SymbolProperty =
        AvaloniaProperty.Register<TextBoxWithSymbol, object?>(nameof(Symbol));

    /// <summary>
    /// It determines the character, indicating the type of input expected from the user.
    /// </summary>
    public object? Symbol
    {
        get => GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }
}