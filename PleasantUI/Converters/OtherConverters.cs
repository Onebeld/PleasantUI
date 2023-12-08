using Avalonia.Controls.Converters;

namespace PleasantUI.Converters;

/// <summary>
/// Provides a set of converters to be used in various scenarios.
/// </summary>
public static class OtherConverters
{
    /// <inheritdoc cref="EnumToBoolConverter"/>
    public static readonly EnumToBoolConverter EnumToBool = new();
}