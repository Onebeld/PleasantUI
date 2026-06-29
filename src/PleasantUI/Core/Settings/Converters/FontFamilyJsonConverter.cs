using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Media;

namespace PleasantUI.Core.Settings.Converters;

internal class FontFamilyJsonConverter : JsonConverter<FontFamily>
{
    public override FontFamily Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();

        return value is null ? FontFamily.Default : FontFamily.Parse(value);
    }

    public override void Write(Utf8JsonWriter writer, FontFamily value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}