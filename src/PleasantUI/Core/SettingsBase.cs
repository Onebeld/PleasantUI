using System.Text.Json.Serialization;
using PleasantUI.Core.Interfaces;

namespace PleasantUI.Core;

/// <summary>
/// Represents a base class for managing application settings with serialization and deserialization support.
/// </summary>
/// <typeparam name="T">The type of settings to be managed, which must have a parameterless constructor.</typeparam>
public abstract class SettingsBase<T> : ViewModelBase where T : new()
{
    private static ISettingsProvider<T>? _settingsProvider;

    /// <summary>
    /// Gets or sets the JSON serializer context for handling serialization.
    /// </summary>
    protected static JsonSerializerContext? JsonSerializerContext;

    /// <summary>
    /// Initializes the settings provider and JSON serializer context.
    /// </summary>
    /// <param name="settingsProvider">The settings provider instance.</param>
    /// <param name="jsonSerializerContext">The JSON serializer context.</param>
    public static void Initialize(ISettingsProvider<T> settingsProvider, JsonSerializerContext jsonSerializerContext)
    {
        _settingsProvider = settingsProvider;
        JsonSerializerContext = jsonSerializerContext;
    }

    /// <summary>
    /// Loads settings from the specified file path.
    /// </summary>
    /// <param name="path">The file path from which to load the settings.</param>
    /// <returns>The loaded settings instance.</returns>
    /// <exception cref="NullReferenceException">Thrown if the settings provider is not initialized.</exception>
    public static T Load(string path)
    {
        if (_settingsProvider is null)
            throw new NullReferenceException("SettingsProvider is not initialized.");

        if (JsonSerializerContext is null)
            throw new NullReferenceException("JsonSerializerContext is not initialized.");

        return _settingsProvider.Load(path, JsonSerializerContext);
    }

    /// <summary>
    /// Saves the specified settings to the given file path.
    /// </summary>
    /// <param name="settings">The settings instance to save.</param>
    /// <param name="path">The file path where the settings will be saved.</param>
    /// <exception cref="NullReferenceException">
    /// Thrown if the settings instance is null or the settings provider is not initialized.
    /// </exception>
    public static void Save(T? settings, string path)
    {
        if (settings is null)
            throw new NullReferenceException("Settings is null");

        if (_settingsProvider is null)
            throw new NullReferenceException("SettingsProvider is not initialized.");

        if (JsonSerializerContext is null)
            throw new NullReferenceException("JsonSerializerContext is not initialized.");

        _settingsProvider.Save(settings, path, JsonSerializerContext);
    }
}
