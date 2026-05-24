using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UBB_SE_2026_923_2.Shared
{
    public class TupleStringBoolConverter : JsonConverter<Tuple<string, bool>>
    {
        public override Tuple<string, bool> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected StartObject");
            }

            string item1 = string.Empty;
            bool item2 = false;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return new Tuple<string, bool>(item1, item2);
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString();
                    reader.Read();

                    if (string.Equals(propertyName, "Item1", StringComparison.OrdinalIgnoreCase))
                    {
                        item1 = reader.GetString();
                    }
                    else if (string.Equals(propertyName, "Item2", StringComparison.OrdinalIgnoreCase))
                    {
                        item2 = reader.GetBoolean();
                    }
                }
            }

            throw new JsonException("Expected EndObject");
        }

        public override void Write(Utf8JsonWriter writer, Tuple<string, bool> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("Item1", value.Item1);
            writer.WriteBoolean("Item2", value.Item2);
            writer.WriteEndObject();
        }
    }
}
