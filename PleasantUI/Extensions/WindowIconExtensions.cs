using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace PleasantUI.Extensions;

public static class WindowIconExtensions
{
    /// <summary>
    /// Converts a <see cref="WindowIcon"/> into a <see cref="Bitmap"/>
    /// </summary>
    /// <param name="icon">Window icon</param>
    /// <returns>Bitmap of the window icon</returns>
    public static IBitmap ToBitmap(this WindowIcon icon)
    {
        using MemoryStream stream = new();
        icon.Save(stream);
        stream.Position = 0;

        return new Bitmap(stream);
    }
}