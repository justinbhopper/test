using Netsmart.Bedrock.CareFabric.Cdm.Entities;

namespace RH.Apollo.Persistence.Search.Converters;

public class CodedEntryValueConverter : SearchValueConverter<CodedEntry>
{
    protected override IEnumerable<string> Convert(CodedEntry value)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(value.CodeSystemName))
            parts.Add($"CodeSystemName={value.CodeSystemName}");

        if (!string.IsNullOrWhiteSpace(value.CodeSystem))
            parts.Add($"CodeSystem={value.CodeSystem}");

        if (!string.IsNullOrWhiteSpace(value.Code))
            parts.Add($"Code={value.Code}");

        if (!string.IsNullOrWhiteSpace(value.DisplayName))
            parts.Add($"DisplayName={value.DisplayName}");

        if (!string.IsNullOrWhiteSpace(value.EhrCode))
            parts.Add($"EhrCode={value.EhrCode}");

        if (!string.IsNullOrWhiteSpace(value.EhrDisplayName))
            parts.Add($"EhrDisplayName={value.EhrDisplayName}");

        if (parts.Count > 0)
            yield return string.Join(", ", parts);
    }
}
