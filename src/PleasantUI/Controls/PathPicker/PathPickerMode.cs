namespace PleasantUI.Controls;

/// <summary>
/// Specifies what kind of path the <see cref="PathPicker"/> should pick.
/// </summary>
public enum PathPickerMode
{
    /// <summary>Open one or more existing files.</summary>
    OpenFile,
    /// <summary>Choose a save location for a file.</summary>
    SaveFile,
    /// <summary>Open one or more existing folders.</summary>
    OpenFolder
}
