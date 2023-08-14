using System.Collections.Concurrent;
using RH.Apollo.Contracts.Utilities;
using RH.Apollo.Persistence.Search.Converters;
using RH.Apollo.Persistence.Search.Extractors;

namespace RH.Apollo.Persistence.Search;

public class SearchIndexResolver
{
    private readonly ConcurrentDictionary<Type, ISearchValueConverter?> _converterCache = new();
    private readonly ConcurrentDictionary<Type, ISearchIndexExtractor?> _extractorCache = new();
    private readonly ICollection<Type> _ignoredTypes;
    private readonly ICollection<ISearchValueConverter> _valueConverters;

    public SearchIndexResolver()
        : this(SearchIndexResolverSettings.Default) {}

    public SearchIndexResolver(SearchIndexResolverSettings settings)
    {
        _valueConverters = settings.ValueConverters ?? new List<ISearchValueConverter>();
        _ignoredTypes = settings.IgnoredTypes ?? new List<Type>();
    }

    public ISearchIndexExtractor? ResolveExtractor(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        return _extractorCache.GetOrAdd(type, CreateExtractor);
    }

    private ISearchIndexExtractor? CreateExtractor(Type type)
    {
        if (IsTypeIgnored(type))
            return null;

        var collectionItemType = ReflectionHelper.GetCollectionItemType(type);
        if (collectionItemType != null)
        {
            var itemExtractor = ResolveExtractor(collectionItemType);

            // Primative collections like IEnumerable<string>, IEnumerable<Enum>, IEnumerable<int>, etc
            var itemConverter = ResolveValueConverter(collectionItemType);
            if (itemConverter != null)
                return new ConvertingCollectionSearchIndexExtractor(itemConverter);

            var basicExtractor = itemExtractor != null
                ? new ExtractingCollectionSearchIndexExtractor(itemExtractor)
                : null;

            // Complex collections like IEnumerable<Address>, IEnumerable<Practitioner>, etc
            if (basicExtractor != null)
                return basicExtractor;

            // No extractor for this collection type, probably because it is ignored
            return null;
        }
        else
        {
            // If we've defined a value converter for this type, then we use the converter instead
            var itemConverter = ResolveValueConverter(type);
            if (itemConverter != null)
                return new ConvertingSearchIndexExtractor(itemConverter);

            return !type.IsValueType
                ? new ReferenceTypeSearchIndexExtractor(type, this)
                : null;
        }
    }

    private ISearchValueConverter? FindValueConverter(Type type)
    {
        if (IsTypeIgnored(type))
            return null;

        foreach (var converter in _valueConverters)
        {
            if (converter.CanConvert(type))
                return converter;
        }

        return null;
    }

    private bool IsTypeIgnored(Type type)
    {
        if (_ignoredTypes.Contains(type))
            return true;

        if (type.IsGenericType && !type.IsGenericTypeDefinition)
        {
            if (IsTypeIgnored(type.GetGenericTypeDefinition()))
            {
                _ignoredTypes.Add(type);
                return true;
            }
        }

        if (ReflectionHelper.IsTupleType(type) || ReflectionHelper.IsValueTupleType(type))
        {
            _ignoredTypes.Add(type);
            return true;
        }

        return false;
    }

    private ISearchValueConverter? ResolveValueConverter(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        return _converterCache.GetOrAdd(type, FindValueConverter);
    }
}
