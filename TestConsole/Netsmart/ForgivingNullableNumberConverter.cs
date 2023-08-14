using System.Text.Json;
using System.Text.Json.Serialization;

namespace TestConsole.Netsmart;

internal class ForgivingNullableNumberConverter<T> : JsonConverter<T?>
    where T : struct, IConvertible
{
    private readonly JsonConverter<T?>? _decorated;
    private readonly IJsonNumberConverter<T> _converter;

    public ForgivingNullableNumberConverter(JsonConverter<T?>? decorated, IJsonNumberConverter<T> converter)
    {
        _decorated = decorated;
        _converter = converter;
    }

    public sealed override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                return _decorated != null
                    ? _decorated.Read(ref reader, typeToConvert, options)
                    : _converter.Read(ref reader);

            case JsonTokenType.String:
                var value = reader.GetString();
                if (value is null)
                    return null;
                return _converter.Parse(value);

            case JsonTokenType.Null:
                return null;

            default:
                throw new JsonException($"Unexpected json token type '{reader.TokenType}'.");
        };
    }

    public sealed override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            _converter.Write(writer, value.Value);
        else
            writer.WriteNullValue();
    }
}
