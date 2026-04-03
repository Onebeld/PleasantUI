namespace PleasantUI.Controls;

/// <summary>
/// Controls which user interaction opens the <see cref="PopConfirm"/> popup.
/// </summary>
[Flags]
public enum PopConfirmTriggerMode
{
    /// <summary>Opens on pointer click / button click.</summary>
    Click = 1,
    /// <summary>Opens when the wrapped element receives keyboard focus.</summary>
    Focus = 2
}
