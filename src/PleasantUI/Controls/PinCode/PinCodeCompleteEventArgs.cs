using Avalonia.Interactivity;

namespace PleasantUI.Controls;

/// <summary>
/// Event args raised when all cells of a <see cref="PinCode"/> have been filled.
/// </summary>
public class PinCodeCompleteEventArgs : RoutedEventArgs
{
    /// <summary>The entered values — one string per cell.</summary>
    public IList<string> Digits { get; }

    /// <summary>The full PIN as a single concatenated string.</summary>
    public string Value => string.Concat(Digits);

    internal PinCodeCompleteEventArgs(IList<string> digits, RoutedEvent routedEvent)
        : base(routedEvent)
    {
        Digits = digits;
    }
}
