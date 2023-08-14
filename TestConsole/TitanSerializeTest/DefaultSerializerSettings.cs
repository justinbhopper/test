using Newtonsoft.Json;

namespace TitanSerializationTest;

public class DefaultSerializerSettings2 : JsonSerializerSettings
{
    public DefaultSerializerSettings2()
    {
        this.ConfigureDefault();
    }
}
