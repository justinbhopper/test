using Netsmart.Bedrock.CareFabric.Cdm.Entities;

namespace RH.Apollo.Persistence.Search.Converters;

public class DateRangeSearchValueConverter : SearchValueConverter<DateRange>
{
    protected override IEnumerable<string> Convert(DateRange value)
    {
        if (!string.IsNullOrEmpty(value.FromDate))
            yield return value.FromDate;

        if (!string.IsNullOrEmpty(value.ToDate))
            yield return value.ToDate;
    }
}
