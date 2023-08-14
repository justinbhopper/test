namespace RH.Apollo.Persistence.Search.Converters;

public class GuidSearchValueConverter : SearchValueConverter<Guid>
{
    protected override IEnumerable<string> Convert(Guid value)
    {
        yield return value.ToString();
    }
}
