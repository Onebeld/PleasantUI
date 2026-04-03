namespace PleasantUI.Controls;

/// <summary>
/// Represents a named file-type filter entry for <see cref="PleasantFileChooserViewModel"/>.
/// </summary>
public sealed class PleasantFileChooserFilter
{
    /// <summary>Gets the display name shown in the filter combo box.</summary>
    public string Name { get; }

    /// <summary>Gets the list of file extensions (e.g. ".txt", ".md"). Empty means all files.</summary>
    public IReadOnlyList<string> Extensions { get; }

    /// <summary>Initializes a new filter entry.</summary>
    public PleasantFileChooserFilter(string name, IReadOnlyList<string> extensions)
    {
        Name       = name;
        Extensions = extensions;
    }

    /// <inheritdoc />
    public override string ToString() => Name;
}
