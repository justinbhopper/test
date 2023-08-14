using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RH.Polaris.NewtonsoftJson;

namespace TitanSerializationTest;

public static class JsonSerializerSettingsExtensions
{
    private static readonly JsonConverter s_resourceReadConverter = new ResourceReadConverter();

    public static void ApplyTitanDefaults(this JsonSerializerSettings settings)
    {
        settings.Converters.Add(s_resourceReadConverter);
    }

    public static JsonSerializer Clone(this JsonSerializer serializer)
    {
        return JsonSerializer.Create(new()
        {
            CheckAdditionalContent = serializer.CheckAdditionalContent,
            ContractResolver = serializer.ContractResolver,
            Converters = new List<JsonConverter>(serializer.Converters),
            Culture = serializer.Culture,
            DateFormatHandling = serializer.DateFormatHandling,
            DateFormatString = serializer.DateFormatString,
            DateParseHandling = serializer.DateParseHandling,
            DateTimeZoneHandling = serializer.DateTimeZoneHandling,
            DefaultValueHandling = serializer.DefaultValueHandling,
            EqualityComparer = serializer.EqualityComparer,
            FloatFormatHandling = serializer.FloatFormatHandling,
            FloatParseHandling = serializer.FloatParseHandling,
            Formatting = serializer.Formatting,
            MaxDepth = serializer.MaxDepth,
            MetadataPropertyHandling = serializer.MetadataPropertyHandling,
            MissingMemberHandling = serializer.MissingMemberHandling,
            NullValueHandling = serializer.NullValueHandling,
            ObjectCreationHandling = serializer.ObjectCreationHandling,
            ReferenceLoopHandling = serializer.ReferenceLoopHandling,
            SerializationBinder = serializer.SerializationBinder,
            StringEscapeHandling = serializer.StringEscapeHandling,
            TypeNameAssemblyFormatHandling = serializer.TypeNameAssemblyFormatHandling,
            TypeNameHandling = serializer.TypeNameHandling,

            // Serializers track reference IDs internally, so we cannot
            // safely preserve references in a cloned serializer
            PreserveReferencesHandling = PreserveReferencesHandling.None
        });
    }

    public static void ConfigureDefault(this JsonSerializerSettings settings)
    {
        settings.ApplyPolarisDefaults();
        settings.ApplyTitanDefaults();
    }

    /// <summary>
    ///     Performs roughly the same operations that JsonSerializer performs for object types.
    ///     This is used to avoid a circular reference issue when attempting to create a JObject
    ///     with an existing JsonSerializer.
    /// </summary>
    [return: NotNullIfNotNull("value")]
    public static JObject? FromObject(this JsonSerializer serializer, object? value)
    {
        if (value is null)
            return null;

        var jObject = new JObject();

        var contract = (JsonObjectContract)serializer.ContractResolver.ResolveContract(value.GetType());

        foreach (var property in contract.Properties)
        {
            if (property.PropertyName is null || property.ValueProvider is null)
                continue;

            object? propertyValue;

            try
            {
                propertyValue = property.ValueProvider.GetValue(value);
            }
            catch (JsonSerializationException)
            {
                // Failure to read property may occur, possibly due to NullReferenceException in the getter
                propertyValue = null;
            }

            if (property.Ignored)
                continue;

            if (propertyValue is null)
            {
                if (serializer.NullValueHandling == NullValueHandling.Include)
                    jObject.Add(property.PropertyName, JValue.CreateNull());
            }
            else
            {
                jObject.Add(property.PropertyName, JToken.FromObject(propertyValue, serializer));
            }
        }

        return jObject;
    }

    public static string ResolvePropertyName(this JsonSerializer serializer, string propertyName)
    {
        return (serializer.ContractResolver as DefaultContractResolver)?.GetResolvedPropertyName(propertyName) ?? propertyName;
    }
}
