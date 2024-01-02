using System.Text.Json.Serialization;

namespace PleasantUI.Core.GenerationContexts;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(PleasantSettings))]
internal partial class PleasantSettingsGenerationContext : JsonSerializerContext
{
}