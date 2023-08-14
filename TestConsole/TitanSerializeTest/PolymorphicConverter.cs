using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TitanSerializationTest;

public abstract class PolymorphicConverter<T> : JsonConverter
    where T : class
{
    private readonly IJsonPolymorphicDiscriminator _discriminator;

    public PolymorphicConverter(IJsonPolymorphicDiscriminator discriminator)
    {
        _discriminator = discriminator;
    }

    public sealed override bool CanRead => true;

    public sealed override bool CanWrite => false;

    public sealed override bool CanConvert(Type objectType)
    {
        // Only convert the generic type, NOT the discriminated type
        // to avoid a recursive loop
        return objectType == typeof(T);
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

    public sealed override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    protected virtual T? Deserialize(JObject jObject, Type targetType, JsonSerializer serializer)
    {
        return (T?)serializer.Deserialize(jObject.CreateReader(), targetType);
    }

    private T? Discriminate(JObject jObject, JsonSerializer serializer)
    {
        // Handle $ref objects
        var reference = jObject["$ref"]?.Value<string>();
        if (reference is not null && serializer.ReferenceResolver != null)
            return (T?)serializer.ReferenceResolver.ResolveReference(serializer, reference);

        var targetType = _discriminator.Discriminate(jObject);
        return Deserialize(jObject, targetType, serializer);
    }
}
