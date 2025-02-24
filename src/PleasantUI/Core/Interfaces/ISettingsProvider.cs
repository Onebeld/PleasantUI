using System.Text.Json.Serialization;

namespace PleasantUI.Core.Interfaces;

public interface ISettingsProvider<T> where T : new()
{
    T Load(string path, JsonSerializerContext jsonSerializerContext);

    void Save(T settings, string path, JsonSerializerContext jsonSerializerContext);
}