using Netsmart.Bedrock.CareFabric.Cdm.Entities;

namespace RH.Apollo.Persistence.Search.Converters;

public class TypeValuePairValueConverter : SearchValueConverter<TypeValuePair>
{
    protected override IEnumerable<string> Convert(TypeValuePair value)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(value.Type))
            parts.Add($"Type={value.Type}");

        if (!string.IsNullOrWhiteSpace(value.Value))
            parts.Add($"Value={value.Value}");

        if (parts.Count > 0)
            yield return string.Join(", ", parts);
    }
}
