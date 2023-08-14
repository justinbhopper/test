using System.Text.Json;
using System.Text.Json.Serialization;

namespace TestConsole.Netsmart;

internal class ForgivingStringConverter : JsonConverter<string>
{
    private readonly JsonConverter<string>? _decorated;

    public ForgivingStringConverter(JsonConverter<string>? decorated)
    {
        _decorated = decorated;
    }

    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => _decorated != null
                ? _decorated.Read(ref reader, typeToConvert, options)
                : reader.GetString(),
            JsonTokenType.Number => reader.GetInt32().ToString(),
            JsonTokenType.True
            or JsonTokenType.False => reader.GetBoolean().ToString(),
            _ => throw new JsonException($"Unexpected json token type '{reader.TokenType}'."),
        };
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
