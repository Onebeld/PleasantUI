using Avalonia;

namespace PleasantUI.Core.Extensions;

/// <summary>
/// Extension methods for accessing application resources.
/// </summary>
public static class ResourceExtensions
{
    /// <summary>
    /// Retrieves a resource of the specified type from the current application's resources.
    /// </summary>
    /// <typeparam name="T">The type of the resource to retrieve.</typeparam>
    /// <param name="key">The key of the resource to retrieve.</param>
    /// <returns>The resource if found; otherwise, the default value for <typeparamref name="T" />.</returns>
    public static T? GetResource<T>(string key)
    {
        if (Application.Current != null && Application.Current.TryGetResource(key, null, out object? value) &&
            value is T resource)
            return resource;

        return default;
    }
}