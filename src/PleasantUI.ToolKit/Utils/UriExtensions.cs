namespace PleasantUI.ToolKit.Utils;

public static class UriExtensions
{
    public static string ToDecodedLocalPath(this Uri uri)
    {
        return Uri.UnescapeDataString(uri.LocalPath);
    }
}