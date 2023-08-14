namespace RH.Apollo.Persistence.Search.Converters;

public class BooleanSearchValueConverter : SearchValueConverter<bool>
{
    protected override IEnumerable<string> Convert(bool boolean)
    {
        yield return boolean.ToString();
    }
}
