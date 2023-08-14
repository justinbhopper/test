using System.Collections.Concurrent;
using RH.Apollo.Persistence.Search.Converters;

namespace RH.Apollo.Persistence.Search;

public class SearchIndexResolverSettings
{
    public static readonly SearchIndexResolverSettings Default = new(
        ignoredTypes: new List<Type>
        {
            typeof(IDictionary<,>),
            typeof(ConcurrentDictionary<,>)
        }.AsReadOnly(),
        valueConverters: new List<ISearchValueConverter>
        {
            new BooleanSearchValueConverter(),
            new CodedEntryValueConverter(),
            new DateRangeSearchValueConverter(),
            new DateTimeSearchValueConverter(),
            new EnumSearchValueConverter(),
            new NumericSearchValueConverter(),
            new StringSearchValueConverter(),
            new TimeSpanSearchValueConverter(),
            new UriSearchValueConverter(),
            new GuidSearchValueConverter(),
            new CareFabricIdSearchValueConverter(),
            new TypeValuePairValueConverter(),
            new IdTypeValuePairValueConverter()
        }.AsReadOnly()
    );

    private SearchIndexResolverSettings(ICollection<Type> ignoredTypes, ICollection<ISearchValueConverter> valueConverters)
    {
        IgnoredTypes = ignoredTypes;
        ValueConverters = valueConverters;
    }

    public ICollection<Type> IgnoredTypes { get; set; }

    public ICollection<ISearchValueConverter> ValueConverters { get; set; }
}
