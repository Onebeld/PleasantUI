using PleasantUI.Core.Enums;

namespace PleasantUI.Core.Structures;

/// <summary>
/// Represents a command item (radio button, checkbox, or command link) in a
/// <see cref="PleasantUI.ToolKit.PleasantDialog"/>.
/// </summary>
public abstract class PleasantDialogCommand
{
    /// <summary>Label text.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Value returned when this command closes the dialog.</summary>
    public object DialogResult { get; set; } = PleasantDialogResult.None;

    /// <summary>Whether the command is enabled.</summary>
    public bool IsEnabled { get; set; } = true;
}

/// <summary>A radio button command in a <see cref="PleasantUI.ToolKit.PleasantDialog"/>.</summary>
public class PleasantDialogRadioButton : PleasantDialogCommand
{
    public bool IsChecked { get; set; }
}

/// <summary>A checkbox command in a <see cref="PleasantUI.ToolKit.PleasantDialog"/>.</summary>
public class PleasantDialogCheckBox : PleasantDialogCommand
{
    public bool IsChecked { get; set; }
}

/// <summary>
/// A command-link button (large button with description) in a
/// <see cref="PleasantUI.ToolKit.PleasantDialog"/>.
/// </summary>
public class PleasantDialogCommandLink : PleasantDialogCommand
{
    /// <summary>Secondary description text shown below the label.</summary>
    public string? Description { get; set; }

    /// <summary>Whether clicking this command closes the dialog.</summary>
    public bool ClosesOnInvoked { get; set; } = true;

    /// <summary>Raised when the command link is clicked.</summary>
    public event EventHandler? Click;

    public void RaiseClick() => Click?.Invoke(this, EventArgs.Empty);
}
