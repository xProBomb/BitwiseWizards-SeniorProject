using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TrustTrade.Helpers.JsonConverters
{
    public class FloatFromStringConverter : JsonConverter<float>
    {
        public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string? stringValue = reader.GetString();
                if (float.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out float floatValue))
                {
                    return floatValue;
                }
                // Optional: Handle parsing failure - log error, return default, or throw
                Console.Error.WriteLine($"Warning: Could not parse string '{stringValue}' as float at path. Returning 0.");
                return 0.0f;
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetSingle();
            }
            // Optional: Handle other types like Null if needed
            else if (reader.TokenType == JsonTokenType.Null)
            {
                return 0.0f; // Or handle nullable floats appropriately if your model uses float?
            }

            throw new JsonException($"Unexpected token type {reader.TokenType} when trying to deserialize float at path.");
        }

        public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options)
        {
            // Write as a standard number
            writer.WriteNumberValue(value);
        }
    }
}