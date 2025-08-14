using System.Text.Json;
using PleasantUI.Core.GenerationContexts;

namespace PleasantUI.Core.Settings.Providers;

internal class PleasantSettingsProvider : Interfaces.ISettingsProvider<PleasantSettings>
{
    public PleasantSettings Load(string path)
    {
        PleasantSettings settings;

        if (File.Exists(path))
        {
            try
            {
                using FileStream fileStream = File.OpenRead(path);
                settings = JsonSerializer.Deserialize<PleasantSettings>(fileStream, PleasantSettingsGenerationContext.Default.PleasantSettings)!;
                return settings;
            }
            catch (Exception)
            {
                // Exception ignored, returning default settings instance.
            }
        }

        settings = new PleasantSettings();

        return settings;
    }

    public void Save(PleasantSettings? settings, string path)
    {
        EnsureFilePathFolderExists(path);
        
        using FileStream fileStream = File.Create(path);
        JsonSerializer.Serialize(fileStream, settings, PleasantSettingsGenerationContext.Default.PleasantSettings);
    }
    
    private void EnsureFilePathFolderExists(string path)
    {
        string? folder = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(folder))
            throw new ArgumentException("Failed to get the directory path", nameof(path));

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
    }
}