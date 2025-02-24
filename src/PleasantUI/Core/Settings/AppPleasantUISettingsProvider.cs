using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using PleasantUI.Core.Interfaces;

namespace PleasantUI.Core.Settings;

public class AppSettingsProvider<T> : ISettingsProvider<T> where T : new()
{
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
                // ignored
            }
        }

        settings = new T();
        
        return settings;
    }

    public void Save(T settings, string path, JsonSerializerContext jsonSerializerContext)
    {
        using FileStream fileStream = File.Create(path);
        JsonSerializer.Serialize(fileStream, settings, jsonSerializerContext.Options);
    }
}