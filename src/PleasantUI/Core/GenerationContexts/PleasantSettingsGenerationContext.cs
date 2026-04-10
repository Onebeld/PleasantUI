using System.Text.Json.Serialization;

namespace PleasantUI.Core.GenerationContexts;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(PleasantSettings))]
[JsonSerializable(typeof(PleasantUI.Core.Settings.AppVersionSettings))]
internal partial class PleasantSettingsGenerationContext : JsonSerializerContext;