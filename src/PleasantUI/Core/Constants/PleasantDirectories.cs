namespace PleasantUI.Core.Constants;

/// <summary>
/// Provides access to common directory paths.
/// </summary>
public static class PleasantDirectories
{
    /// <summary>
    /// The path of the settings directory.
    /// </summary>
    public static readonly string Settings = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings");
    
    /// <summary>
    /// The path of the themes directory.
    /// </summary>
    public static readonly string Themes = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes");
}