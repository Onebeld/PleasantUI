using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace PleasantUI.Controls;

/// <summary>
/// A PIN / OTP entry control that renders a row of individual character cells.
/// Supports digit-only, letter-only, or mixed input, optional password masking,
/// clipboard paste, and keyboard navigation.
/// </summary>
[TemplatePart("PART_ItemsControl", typeof(PinCodeItemsControl))]
public class PinCode : TemplatedControl
{
    private PinCodeItemsControl? _itemsControl;
    private int _currentIndex;

    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>Number of character cells. Default is 4.</summary>
    public static readonly StyledProperty<int> CountProperty =
        AvaloniaProperty.Register<PinCode, int>(nameof(Count), 4);

    /// <summary>Which character types are accepted.</summary>
    public static readonly StyledProperty<PinCodeMode> ModeProperty =
        AvaloniaProperty.Register<PinCode, PinCodeMode>(nameof(Mode), PinCodeMode.LetterOrDigit);

    /// <summary>When non-zero, each filled cell displays this character instead of the actual input.</summary>
    public static readonly StyledProperty<char> PasswordCharProperty =
        AvaloniaProperty.Register<PinCode, char>(nameof(PasswordChar));

    /// <summary>The current list of entered characters (one per cell, empty string = unfilled).</summary>
    public static readonly DirectProperty<PinCode, IList<string>> DigitsProperty =
        AvaloniaProperty.RegisterDirect<PinCode, IList<string>>(nameof(Digits), o => o.Digits);

    /// <summary>Command invoked when all cells are filled.</summary>
    public static readonly StyledProperty<ICommand?> CompleteCommandProperty =
        AvaloniaProperty.Register<PinCode, ICommand?>(nameof(CompleteCommand));

    /// <summary>Spacing between cells in pixels. Default is 8.</summary>
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<PinCode, double>(nameof(Spacing), 8);

    private IList<string> _digits = new List<string>(Enumerable.Repeat(string.Empty, 4));

    public int         Count           { get => GetValue(CountProperty);           set => SetValue(CountProperty, value); }
    public PinCodeMode Mode            { get => GetValue(ModeProperty);            set => SetValue(ModeProperty, value); }
    public char        PasswordChar    { get => GetValue(PasswordCharProperty);    set => SetValue(PasswordCharProperty, value); }
    public ICommand?   CompleteCommand { get => GetValue(CompleteCommandProperty); set => SetValue(CompleteCommandProperty, value); }
    public double      Spacing         { get => GetValue(SpacingProperty);         set => SetValue(SpacingProperty, value); }

    /// <summary>Current entered values — one entry per cell.</summary>
    public IList<string> Digits
    {
        get => _digits;
        private set
        {
            SetAndRaise(DigitsProperty, ref _digits, value);
            // Keep ItemsControl in sync whenever the list is replaced
            if (_itemsControl is not null)
                _itemsControl.ItemsSource = _digits;
        }
    }

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Raised when all cells have been filled.</summary>
    public static readonly RoutedEvent<PinCodeCompleteEventArgs> CompleteEvent =
        RoutedEvent.Register<PinCode, PinCodeCompleteEventArgs>(nameof(Complete), RoutingStrategies.Bubble);

    /// <summary>Raised when all cells have been filled.</summary>
    public event EventHandler<PinCodeCompleteEventArgs> Complete
    {
        add    => AddHandler(CompleteEvent, value);
        remove => RemoveHandler(CompleteEvent, value);
    }

    // ── Static init ───────────────────────────────────────────────────────────

    static PinCode()
    {
        FocusableProperty.OverrideDefaultValue<PinCode>(true);

        CountProperty.Changed.AddClassHandler<PinCode, int>((pin, args) =>
        {
            int n = args.NewValue.Value;
            if (n > 0)
            {
                pin.Digits = new List<string>(Enumerable.Repeat(string.Empty, n));
                // If template already applied, refresh ItemsSource
                if (pin._itemsControl is not null)
                    pin._itemsControl.ItemsSource = pin.Digits;
            }
        });

        KeyDownEvent.AddClassHandler<PinCode>((o, e) => o.OnPreviewKeyDown(e), RoutingStrategies.Tunnel);
    }

    public PinCode()
    {
        // Initialize with default Count so Digits is non-empty before template applies
        Digits = new List<string>(Enumerable.Repeat(string.Empty, Count));
    }

    // ── Template ──────────────────────────────────────────────────────────────

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _itemsControl = e.NameScope.Get<PinCodeItemsControl>("PART_ItemsControl");

        // Ensure ItemsSource is set — Digits may have been initialized before template applied
        _itemsControl.ItemsSource = Digits;

        AddHandler(PointerPressedEvent, OnControlPressed, RoutingStrategies.Tunnel, handledEventsToo: false);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(PinCode);

    // ── Pointer ───────────────────────────────────────────────────────────────

    private void OnControlPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is Control t)
        {
            var item = t.FindLogicalAncestorOfType<PinCodeItem>();
            if (item is not null)
            {
                item.Focus();
                _currentIndex = _itemsControl?.IndexFromContainer(item) ?? 0;
            }
            else
            {
                _currentIndex = Clamp(_currentIndex, 0, Count - 1);
                _itemsControl?.ContainerFromIndex(_currentIndex)?.Focus();
            }
        }
        e.Handled = true;
    }

    // ── Text input ────────────────────────────────────────────────────────────

    /// <inheritdoc />
    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);

        if (e.Text?.Length != 1 || _currentIndex >= Count) return;

        char c = e.Text[0];
        if (!IsValid(c)) return;

        var cell = _itemsControl?.ContainerFromIndex(_currentIndex) as PinCodeItem;
        if (cell is null) return;

        cell.Text        = e.Text;
        cell.PasswordChar = PasswordChar;
        Digits[_currentIndex] = e.Text;
        _currentIndex++;

        _itemsControl?.ContainerFromIndex(_currentIndex)?.Focus();

        if (_currentIndex == Count)
        {
            RaiseComplete();
            _currentIndex--;
        }
    }

    // ── Keyboard ──────────────────────────────────────────────────────────────

    private async void OnPreviewKeyDown(KeyEventArgs e)
    {
        // Paste
        var pasteKeys = Application.Current?.PlatformSettings?.HotkeyConfiguration.Paste;
        if (pasteKeys?.Any(k => k.Matches(e)) == true)
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard is null) return;

            // IClipboard implements IAsyncDataTransfer in Avalonia 12
            string? text = null;
            if (clipboard is IAsyncDataTransfer transfer)
                text = await transfer.TryGetTextAsync();

            if (text is not null)
            {
                var chars = text.Where(IsValid).Take(Count).ToArray();
                for (int i = 0; i < chars.Length; i++)
                {
                    Digits[i] = chars[i].ToString();
                    if (_itemsControl?.ContainerFromIndex(i) is PinCodeItem cell)
                    {
                        cell.Text         = chars[i].ToString();
                        cell.PasswordChar  = PasswordChar;
                    }
                }
                _currentIndex = Math.Min(chars.Length, Count - 1);
                _itemsControl?.ContainerFromIndex(_currentIndex)?.Focus();

                if (chars.Length == Count)
                    RaiseComplete();
            }
            e.Handled = true;
            return;
        }

        switch (e.Key)
        {
            case Key.Back:
            {
                _currentIndex = Clamp(_currentIndex, 0, Count - 1);
                var cell = _itemsControl?.ContainerFromIndex(_currentIndex) as PinCodeItem;
                if (cell is null) break;

                if (!string.IsNullOrEmpty(Digits[_currentIndex]))
                {
                    // Clear current cell first
                    cell.Text = string.Empty;
                    Digits[_currentIndex] = string.Empty;
                }
                else if (_currentIndex > 0)
                {
                    // Move back and clear previous
                    _currentIndex--;
                    var prev = _itemsControl?.ContainerFromIndex(_currentIndex) as PinCodeItem;
                    if (prev is not null) { prev.Text = string.Empty; Digits[_currentIndex] = string.Empty; }
                    _itemsControl?.ContainerFromIndex(_currentIndex)?.Focus();
                }
                e.Handled = true;
                break;
            }
            case Key.Delete:
            {
                _currentIndex = Clamp(_currentIndex, 0, Count - 1);
                var cell = _itemsControl?.ContainerFromIndex(_currentIndex) as PinCodeItem;
                if (cell is not null) { cell.Text = string.Empty; Digits[_currentIndex] = string.Empty; }
                if (_currentIndex < Count - 1)
                {
                    _currentIndex++;
                    _itemsControl?.ContainerFromIndex(_currentIndex)?.Focus();
                }
                e.Handled = true;
                break;
            }
            case Key.Left:
            case Key.FnLeftArrow:
                _currentIndex = Clamp(_currentIndex - 1, 0, Count - 1);
                _itemsControl?.ContainerFromIndex(_currentIndex)?.Focus();
                e.Handled = true;
                break;

            case Key.Right:
            case Key.FnRightArrow:
                _currentIndex = Clamp(_currentIndex + 1, 0, Count - 1);
                _itemsControl?.ContainerFromIndex(_currentIndex)?.Focus();
                e.Handled = true;
                break;

            case Key.Tab:
                _currentIndex = e.KeyModifiers == KeyModifiers.Shift
                    ? Clamp(_currentIndex - 1, 0, Count - 1)
                    : Clamp(_currentIndex + 1, 0, Count - 1);
                _itemsControl?.ContainerFromIndex(_currentIndex)?.Focus();
                e.Handled = true;
                break;

            case Key.Enter:
                RaiseComplete();
                e.Handled = true;
                break;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private bool IsValid(char c) => Mode switch
    {
        PinCodeMode.Digit        => char.IsDigit(c),
        PinCodeMode.Letter       => char.IsLetter(c),
        PinCodeMode.LetterOrDigit => char.IsLetterOrDigit(c),
        _                        => true
    };

    private void RaiseComplete()
    {
        CompleteCommand?.Execute(Digits);
        RaiseEvent(new PinCodeCompleteEventArgs(Digits, CompleteEvent));
    }

    private static int Clamp(int value, int min, int max) =>
        value < min ? min : value > max ? max : value;
}
