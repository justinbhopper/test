namespace RH.Apollo.Persistence.Search.Converters;

public interface ISearchValueConverter
{
    bool CanConvert(Type type);

    IEnumerable<string> Convert(object value);
}
