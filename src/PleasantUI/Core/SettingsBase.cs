using System.Text.Json.Serialization;
using PleasantUI.Core.Interfaces;

namespace PleasantUI.Core;

public abstract class SettingsBase<T> : ViewModelBase where T : new()
{
    private static ISettingsProvider<T>? _settingsProvider;
    
    protected static JsonSerializerContext JsonSerializerContext;

    public static void Initialize(ISettingsProvider<T> settingsProvider, JsonSerializerContext jsonSerializerContext)
    {
        _settingsProvider = settingsProvider;
        JsonSerializerContext = jsonSerializerContext;
    }

    public static T Load(string path)
    {
        if (_settingsProvider is null)
            throw new NullReferenceException("SettingsProvider is not initialized.");
        
        return _settingsProvider.Load(path, JsonSerializerContext);
    }

    public static void Save(T? settings, string path)
    {
        if (settings is null)
            throw new NullReferenceException("Settings is null");
        
        if (_settingsProvider is null)
            throw new NullReferenceException("SettingsProvider is not initialized.");
        
        _settingsProvider.Save(settings, path, JsonSerializerContext);
    }
}