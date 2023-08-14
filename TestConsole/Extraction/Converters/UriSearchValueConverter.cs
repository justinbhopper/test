namespace RH.Apollo.Persistence.Search.Converters;

public class UriSearchValueConverter : SearchValueConverter<Uri>
{
    protected override IEnumerable<string> Convert(Uri value)
    {
        if (!string.IsNullOrWhiteSpace(value.ToString()))
            yield return value.ToString();
    }
}
