namespace RH.Apollo.Persistence.Search.Converters;

public class StringSearchValueConverter : SearchValueConverter<string>
{
    protected override IEnumerable<string> Convert(string s)
    {
        if (!string.IsNullOrWhiteSpace(s))
            yield return s;
    }
}
