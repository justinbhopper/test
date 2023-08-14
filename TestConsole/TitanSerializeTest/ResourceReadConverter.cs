using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RH.Titan.Contracts;
using RH.Titan.Contracts.Models;
using RH.Titan.Contracts.Resources;

namespace TitanSerializationTest;

public class ResourceReadConverter : PolymorphicConverter<Resource>
{
    private static readonly DefaultSerializerSettings2 s_serializerSettings = new();

    public ResourceReadConverter()
        : base(new ResourceDiscriminator()) {}

    protected override Resource? Deserialize(JObject jObject, Type resourceType, JsonSerializer serializer)
    {
        var resource = base.Deserialize(jObject, resourceType, serializer);

        if (resource != null)
        {
            // Clone object, but don't use existing instance of serializer in case there are $id refs
            var original = (Resource?)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(resource, s_serializerSettings), resourceType, s_serializerSettings);

            // Append original source
            ((IOriginalSource)resource).Original = original;
        }

        return resource;
    }

    private class ResourceDiscriminator : IJsonPolymorphicDiscriminator
    {
        public Type Discriminate(JObject jObject)
        {
            var resourceTypeToken = jObject["ResourceType"] ?? jObject["resourceType"];
            if (resourceTypeToken is null || resourceTypeToken.Type != JTokenType.String)
                throw new JsonSerializationException($"Property '{nameof(Resource.ResourceType)}' must be present to deserialize type {nameof(Resource)}.");

            var resourceTypeName = resourceTypeToken.Value<string>();

            if (string.IsNullOrEmpty(resourceTypeName))
                throw new JsonSerializationException($"Property '{nameof(Resource.ResourceType)}' cannot be null or empty to deserialize type {nameof(Resource)}.");

            return ResourceRegistry.GetResourceClass(resourceTypeName);
        }
    }
}
