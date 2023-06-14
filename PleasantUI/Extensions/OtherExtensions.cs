namespace PleasantUI.Extensions;

public static class OtherExtensions
{
    public static T? GetValueOrDefault<T>(this object? value) where T : class => value as T;
}