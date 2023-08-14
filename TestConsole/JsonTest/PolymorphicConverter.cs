using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TestConsole.JsonTest;

public abstract class PolymorphicConverter<T> : JsonConverter
    where T : class
{
    private readonly IJsonPolymorphicDiscriminator _discriminator;

    public PolymorphicConverter(IJsonPolymorphicDiscriminator discriminator)
    {
        _discriminator = discriminator;
    }

    public sealed override bool CanWrite => false;

    public sealed override bool CanRead => true;

    public sealed override bool CanConvert(Type objectType)
    {
        // Only convert the generic type, NOT the discriminated type
        // to avoid a recursive loop
        return objectType == typeof(T);
    }

    public sealed override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public sealed override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        return reader.TokenType switch
        {
            JsonToken.Null => null,
            JsonToken.StartObject => Discriminate(JObject.Load(reader), serializer),
            _ => throw new JsonSerializationException($"Unexpected token type {reader.TokenType} encountered while attempting to deserialize {objectType.Name}."),
        };
    }

    protected virtual T? Deserialize(JObject jObject, Type targetType, JsonSerializer serializer)
    {
        return (T?)serializer.Deserialize(jObject.CreateReader(), targetType);
    }

    private T? Discriminate(JObject jObject, JsonSerializer serializer)
    {
        var targetType = _discriminator.Discriminate(jObject);
        return Deserialize(jObject, targetType, serializer);
    }
}
