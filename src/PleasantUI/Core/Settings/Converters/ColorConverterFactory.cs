using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Collections;
using Avalonia.Media;

namespace PleasantUI.Core.Settings.Converters;

internal class ColorConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(AvaloniaList<Color>) || typeToConvert == typeof(Color);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert == typeof(Color))
        {
            return new ColorJsonConverter();
        }
        
        if (typeToConvert == typeof(AvaloniaList<Color>))
        {
            return new AvaloniaColorListInternalConverter();
        }

        throw new ArgumentException($"Тип {typeToConvert.Name} не поддерживается этой фабрикой.");
    }
    
    private class AvaloniaColorListInternalConverter : JsonConverter<AvaloniaList<Color>>
    {
        public override AvaloniaList<Color> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            AvaloniaList<Color> list = [];
            
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    uint argb = reader.GetUInt32();
                    list.Add(Color.FromUInt32(argb));
                }
            }
            else
            {
                uint argb = reader.GetUInt32();
                list.Add(Color.FromUInt32(argb));
            }

            return list;
        }

        public override void Write(Utf8JsonWriter writer, AvaloniaList<Color> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (Color color in value)
            {
                uint argb = color.ToUInt32();
                writer.WriteNumberValue(argb);
            }

            writer.WriteEndArray();
        }
    }
    
    private class ColorJsonConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            uint color = reader.GetUInt32();

            return Color.FromUInt32(color);
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.ToUInt32());
        }
    }
}