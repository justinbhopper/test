namespace RH.Apollo.Persistence.Search.Converters;

public abstract class SearchValueConverter<T> : ISearchValueConverter
{
    private static readonly Type s_genericType = typeof(T);

    public bool CanConvert(Type type)
    {
        return s_genericType.IsAssignableFrom(type);
    }

    protected abstract IEnumerable<string> Convert(T value);

    IEnumerable<string> ISearchValueConverter.Convert(object value)
    {
        if (value is T tValue && tValue != null)
            return Convert(tValue);
        return Enumerable.Empty<string>();
    }
}
