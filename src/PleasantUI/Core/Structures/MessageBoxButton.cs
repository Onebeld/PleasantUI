namespace PleasantUI.Core.Structures;

/// <summary>
/// Defines the structure of the button for the message box
/// </summary>
public struct MessageBoxButton
{
    /// <summary>
    /// Result as a string when you click on the button
    /// </summary>
    public string Result;

    /// <summary>
    /// The text that will be displayed on the button
    /// </summary>
    public string Text;

    /// <summary>
    /// Determines whether the button will be repainted in the accent color
    /// </summary>
    public bool Default;

    /// <summary>
    /// Defines pressing Enter on the keyboard as pressing this button
    /// </summary>
    public bool IsKeyDown;
}