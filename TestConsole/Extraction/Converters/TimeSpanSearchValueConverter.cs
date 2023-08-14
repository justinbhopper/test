namespace RH.Apollo.Persistence.Search.Converters;

public class TimeSpanSearchValueConverter : SearchValueConverter<TimeSpan>
{
    protected override IEnumerable<string> Convert(TimeSpan timeSpan)
    {
        yield return timeSpan.ToString();
    }
}
