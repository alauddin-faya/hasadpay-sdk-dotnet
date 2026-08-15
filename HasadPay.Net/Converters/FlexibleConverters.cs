using System.Text.Json;
using System.Text.Json.Serialization;

namespace HasadPay.Net.Converters;

/// <summary>
/// A resilient JSON converter for strings that accepts strings, numbers, booleans, and nulls without throwing exceptions.
/// </summary>
public class FlexibleStringConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => reader.TryGetInt64(out long l)
                ? l.ToString()
                : (reader.TryGetDouble(out double d)
                    ? d.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    : reader.GetDecimal().ToString(System.Globalization.CultureInfo.InvariantCulture)),
            JsonTokenType.True => "true",
            JsonTokenType.False => "false",
            JsonTokenType.Null => null,
            _ => JsonDocument.ParseValue(ref reader).RootElement.GetRawText()
        };
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}

/// <summary>
/// A resilient JSON converter for integers that accepts integers or numeric strings.
/// </summary>
public class FlexibleIntConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetInt32();
        }
        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            if (int.TryParse(str, out int val)) return val;
        }
        return 0;
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
