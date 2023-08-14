using Netsmart.Bedrock.CareFabric.Cdm.Entities;

namespace RH.Apollo.Persistence.Search.Converters;

public class CareFabricIdSearchValueConverter : SearchValueConverter<CareFabricID>
{
    protected override IEnumerable<string> Convert(CareFabricID value)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(value.ScopeID))
            parts.Add($"ScopeID={value.ScopeID}");

        if (!string.IsNullOrWhiteSpace(value.ID))
            parts.Add($"ID={value.ID}");

        if (!string.IsNullOrWhiteSpace(value.HumanReadableValue))
            parts.Add($"HumanReadableValue={value.HumanReadableValue}");

        if (parts.Count > 0)
            yield return string.Join(", ", parts);
    }
}
