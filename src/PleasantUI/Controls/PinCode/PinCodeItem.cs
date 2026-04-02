using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace PleasantUI.Controls;

/// <summary>
/// A single character cell inside a <see cref="PinCode"/> control.
/// </summary>
[PseudoClasses(":filled", ":focused", ":password")]
public class PinCodeItem : TemplatedControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<PinCodeItem, string>(
            nameof(Text),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<char> PasswordCharProperty =
        AvaloniaProperty.Register<PinCodeItem, char>(
            nameof(PasswordChar),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public char PasswordChar
    {
        get => GetValue(PasswordCharProperty);
        set => SetValue(PasswordCharProperty, value);
    }

    static PinCodeItem()
    {
        TextProperty.Changed.AddClassHandler<PinCodeItem>((item, _) =>
            item.UpdateStates());

        PasswordCharProperty.Changed.AddClassHandler<PinCodeItem>((item, _) =>
            item.UpdateStates());

        FocusableProperty.OverrideDefaultValue<PinCodeItem>(true);
    }

    private void UpdateStates()
    {
        bool hasText = !string.IsNullOrEmpty(Text);
        bool isPassword = PasswordChar != '\0' && hasText;

        PseudoClasses.Set(":filled", hasText);
        PseudoClasses.Set(":password", isPassword);
    }

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        PseudoClasses.Set(":focused", true);
    }

    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        PseudoClasses.Set(":focused", false);
    }

    protected override Type StyleKeyOverride => typeof(PinCodeItem);
}
