namespace PleasantUI.Core.Structures;

/// <summary>
/// Result returned by <see cref="PleasantUI.ToolKit.MessageBox.Show{T}"/>.
/// Carries both the button result string and any value extracted from the extra content.
/// </summary>
/// <typeparam name="T">Type of the value produced by the extra content.</typeparam>
public sealed class MessageBoxResult<T>
{
    /// <summary>The <see cref="MessageBoxButton.Result"/> of the button that was clicked.</summary>
    public string Button { get; init; } = "OK";

    /// <summary>Value captured from the extra content control.</summary>
    public T? Value { get; init; }
}
