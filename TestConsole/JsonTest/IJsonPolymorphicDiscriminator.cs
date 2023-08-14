using Newtonsoft.Json.Linq;

namespace TestConsole.JsonTest;

public interface IJsonPolymorphicDiscriminator
{
    Type Discriminate(JObject jObject);
}
