using System.Text.Json;
using System.Text.Json.Serialization;
using PleasantUI.Core.Interfaces;

namespace PleasantUI.Core.Settings;
/// <summary>
/// Provides methods for loading and saving application settings using JSON serialization.
/// </summary>
/// <typeparam name="T">The type of settings object to handle. Must have a parameterless constructor.</typeparam>
public class AppSettingsProvider<T> : ISettingsProvider<T> where T : new()
{
    /// <summary>
    /// Loads settings from a JSON file at the specified path.
    /// </summary>
    /// <param name="path">The file path from which to load the settings.</param>
    /// <param name="jsonSerializerContext">The JSON serializer context used for deserialization.</param>
    /// <returns>The loaded settings object of type <typeparamref name="T"/>.</returns>
    public T Load(string path, JsonSerializerContext jsonSerializerContext)
    {
        T settings;

        if (File.Exists(path))
        {
            try
            {
                using FileStream fileStream = File.OpenRead(path);
                settings = JsonSerializer.Deserialize<T>(fileStream, jsonSerializerContext.Options)!;
                return settings;
            }
            catch (Exception)
            {
                // Exception ignored, returning default settings instance.
            }
        }

        settings = new T();

        return settings;
    }

    /// <summary>
    /// Saves the provided settings object to a JSON file at the specified path.
    /// </summary>
    /// <param name="settings">The settings object to save.</param>
    /// <param name="path">The file path where the settings should be saved.</param>
    /// <param name="jsonSerializerContext">The JSON serializer context used for serialization.</param>
    public void Save(T settings, string path, JsonSerializerContext jsonSerializerContext)
    {
        using FileStream fileStream = File.Create(path);
        JsonSerializer.Serialize(fileStream, settings, jsonSerializerContext.Options);
    }
}
