using System.Windows.Input;

using PleasantUI.Core.Enums;

namespace PleasantUI.Core.Structures;

/// <summary>
/// Represents a button in a <see cref="PleasantUI.ToolKit.PleasantDialog"/>.
/// </summary>
public class PleasantDialogButton
{
    /// <summary>Text displayed on the button.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Value returned when this button is clicked.</summary>
    public object DialogResult { get; set; } = PleasantDialogResult.None;

    /// <summary>Whether this button is the default (receives Enter key and accent styling).</summary>
    public bool IsDefault { get; set; }

    /// <summary>Whether this button is enabled.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Optional ControlTheme resource key (e.g. "DangerButtonTheme").
    /// When null, default or accent theme is applied based on <see cref="IsDefault"/>.
    /// </summary>
    public string? ThemeKey { get; set; }

    /// <summary>Optional command executed when the button is clicked.</summary>
    public ICommand? Command { get; set; }

    /// <summary>Parameter passed to <see cref="Command"/>.</summary>
    public object? CommandParameter { get; set; }

    /// <summary>Raised when the button is clicked.</summary>
    public event EventHandler? Click;

    public void RaiseClick() => Click?.Invoke(this, EventArgs.Empty);

    public static PleasantDialogButton OK     => new() { Text = "OK",     DialogResult = PleasantDialogResult.OK,     IsDefault = true };
    public static PleasantDialogButton Cancel => new() { Text = "Cancel", DialogResult = PleasantDialogResult.Cancel };
    public static PleasantDialogButton Yes    => new() { Text = "Yes",    DialogResult = PleasantDialogResult.Yes,    IsDefault = true };
    public static PleasantDialogButton No     => new() { Text = "No",     DialogResult = PleasantDialogResult.No };
    public static PleasantDialogButton Close  => new() { Text = "Close",  DialogResult = PleasantDialogResult.Close };
    public static PleasantDialogButton Retry  => new() { Text = "Retry",  DialogResult = PleasantDialogResult.Retry };
}
