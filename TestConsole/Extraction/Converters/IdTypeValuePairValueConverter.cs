using Netsmart.Bedrock.CareFabric.Cdm.Entities;

namespace RH.Apollo.Persistence.Search.Converters;

public class IdTypeValuePairValueConverter : SearchValueConverter<IdTypeValuePair>
{
    protected override IEnumerable<string> Convert(IdTypeValuePair value)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(value.Type))
            parts.Add($"Type={value.Type}");

        if (!string.IsNullOrWhiteSpace(value.Value))
            parts.Add($"Value={value.Value}");

        if (!string.IsNullOrWhiteSpace(value.ID))
            parts.Add($"ID={value.ID}");

        if (parts.Count > 0)
            yield return string.Join(", ", parts);
    }
}
