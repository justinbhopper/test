namespace RH.Apollo.Persistence.Search.Converters;

public class EnumSearchValueConverter : ISearchValueConverter
{
    public bool CanConvert(Type type)
    {
        return type.IsEnum;
    }

    public IEnumerable<string> Convert(object value)
    {
        if (value is Enum enumValue)
        {
            yield return enumValue.ToString("F");
        }
    }
}
