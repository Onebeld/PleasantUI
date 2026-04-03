using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace PleasantUI.Controls;

/// <summary>
/// A single character cell inside a <see cref="PinCode"/> control.
/// </summary>
[PseudoClasses(":filled", ":focused")]
public class PinCodeItem : TemplatedControl
{
    /// <summary>The raw entered character.</summary>
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<PinCodeItem, string>(nameof(Text), string.Empty);

    /// <summary>When non-zero, each filled cell shows this character instead of <see cref="Text"/>.</summary>
    public static readonly StyledProperty<char> PasswordCharProperty =
        AvaloniaProperty.Register<PinCodeItem, char>(nameof(PasswordChar));

    /// <summary>The text actually displayed — either <see cref="Text"/> or <see cref="PasswordChar"/>.</summary>
    public static readonly DirectProperty<PinCodeItem, string> DisplayTextProperty =
        AvaloniaProperty.RegisterDirect<PinCodeItem, string>(nameof(DisplayText), o => o.DisplayText);

    private string _displayText = string.Empty;

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

    public string DisplayText
    {
        get => _displayText;
        private set => SetAndRaise(DisplayTextProperty, ref _displayText, value);
    }

    static PinCodeItem()
    {
        FocusableProperty.OverrideDefaultValue<PinCodeItem>(true);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TextProperty || change.Property == PasswordCharProperty)
        {
            bool filled = !string.IsNullOrEmpty(Text);
            PseudoClasses.Set(":filled", filled);
            DisplayText = filled && PasswordChar != '\0'
                ? PasswordChar.ToString()
                : Text;
        }
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
