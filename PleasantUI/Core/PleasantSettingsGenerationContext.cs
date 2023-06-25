using System.Text.Json.Serialization;

namespace PleasantUI.Core;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(PleasantSettings))]
internal partial class PleasantSettingsGenerationContext : JsonSerializerContext
{
}