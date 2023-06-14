using Avalonia;
using Avalonia.Controls;

namespace PleasantUI.Controls;

public class TextBoxWithSymbol : TextBox
{
    public static readonly StyledProperty<object?> SymbolProperty =
        AvaloniaProperty.Register<TextBoxWithSymbol, object?>(nameof(Symbol));

    public object? Symbol
    {
        get => GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }
}