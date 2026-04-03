namespace PleasantUI.Controls;

/// <summary>
/// Defines which character types are accepted by a <see cref="PinCode"/> control.
/// </summary>
[Flags]
public enum PinCodeMode
{
    /// <summary>Only letter characters (A–Z, a–z) are accepted.</summary>
    Letter = 1,

    /// <summary>Only digit characters (0–9) are accepted.</summary>
    Digit = 2,

    /// <summary>Both letters and digits are accepted (default).</summary>
    LetterOrDigit = Letter | Digit
}
