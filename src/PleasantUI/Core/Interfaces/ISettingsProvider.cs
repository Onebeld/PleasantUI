using System.Text.Json.Serialization;

namespace PleasantUI.Core.Interfaces;

/// <summary>
/// Defines methods for loading and saving settings of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of settings object, which must have a parameterless constructor.</typeparam>
public interface ISettingsProvider<T> where T : new()
{
    /// <summary>
    /// Loads settings from the specified file path using the provided JSON serializer context.
    /// </summary>
    /// <param name="path">The file path from which to load the settings.</param>
    /// <param name="jsonSerializerContext">The JSON serializer context to use for deserialization.</param>
    /// <returns>The loaded settings object of type <typeparamref name="T"/>.</returns>
    T Load(string path, JsonSerializerContext jsonSerializerContext);

    /// <summary>
    /// Saves the given settings to the specified file path using the provided JSON serializer context.
    /// </summary>
    /// <param name="settings">The settings object to save.</param>
    /// <param name="path">The file path to save the settings to.</param>
    /// <param name="jsonSerializerContext">The JSON serializer context to use for serialization.</param>
    void Save(T settings, string path, JsonSerializerContext jsonSerializerContext);
}
