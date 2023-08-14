namespace RH.Apollo.Persistence.Search.Converters;

public class DateTimeSearchValueConverter : ISearchValueConverter
{
    private static readonly Type s_dateTimeOffsetType = typeof(DateTimeOffset);
    private static readonly Type s_dateTimeType = typeof(DateTime);

    public bool UseExtractor => false;

    public bool CanConvert(Type type)
    {
        return type == s_dateTimeType
            || type == s_dateTimeOffsetType;
    }

    public IEnumerable<string> Convert(object value)
    {
        if (value is DateTime dateTime)
            yield return dateTime.ToString();

        if (value is DateTimeOffset dateTimeOffset)
            yield return dateTimeOffset.ToString();
    }
}
