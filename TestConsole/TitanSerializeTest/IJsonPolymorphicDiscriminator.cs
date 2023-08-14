using Newtonsoft.Json.Linq;

namespace TitanSerializationTest;

public interface IJsonPolymorphicDiscriminator
{
    Type Discriminate(JObject jObject);
}
