using System.Text.Json.Serialization;

namespace PleasantUI.Core.Settings.GenerationContexts;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(PleasantSettings))]
internal partial class PleasantSettingsGenerationContext : JsonSerializerContext;