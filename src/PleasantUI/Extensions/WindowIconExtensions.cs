using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace PleasantUI.Extensions;

/// <summary>
/// Provides extension methods for <see cref="WindowIcon"/>.
/// </summary>
public static class WindowIconExtensions
{
    /// <summary>
    /// Converts a <see cref="WindowIcon" /> into a <see cref="Bitmap" />
    /// </summary>
    /// <param name="icon">Window icon</param>
    /// <returns>Bitmap of the window icon</returns>
    public static Bitmap ToBitmap(this WindowIcon icon)
    {
        using MemoryStream stream = new();
        icon.Save(stream);
        stream.Position = 0;

        return new Bitmap(stream);
    }
}