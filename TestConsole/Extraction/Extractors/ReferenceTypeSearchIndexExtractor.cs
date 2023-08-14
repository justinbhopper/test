using System.Reflection;
using Newtonsoft.Json;
using RH.Apollo.Contracts.Annotations;

namespace RH.Apollo.Persistence.Search.Extractors;

public class ReferenceTypeSearchIndexExtractor : ISearchIndexExtractor
{
    private readonly Lazy<Contract> _contract;
    private readonly SearchIndexResolver _resolver;
    private readonly Type _type;

    public ReferenceTypeSearchIndexExtractor(Type type, SearchIndexResolver resolver)
    {
        _type = type;
        _resolver = resolver;
        _contract = new(DiscoverContract, true);
    }

    public IEnumerable<SearchIndexEntry> Extract(string currentPath, object value, SearchIndexExtractionContext context)
    {
        if (value is null)
            yield break;

        var contract = _contract.Value;

        foreach (var property in contract.ExtractableProperties)
        {
            var propertyValue = property.GetMethod.Invoke(value, null);

            var propertyPath = string.IsNullOrEmpty(currentPath)
                ? property.PropertyName
                : $"{currentPath}.{property.PropertyName}";

            // Record null values
            if (propertyValue is null)
            {
                yield return new SearchIndexEntry(propertyPath, null, true);
                continue;
            }

            // Don't extract if we've already crossed this object before
            if (context.IsCircularReference(propertyValue, propertyPath))
                continue;

            var propertyValueType = propertyValue.GetType();

            foreach (var extractor in property.Extractors)
            {
                // Skip AllowedTypes that aren't a match to the actual value type
                if (!propertyValueType.IsAssignableTo(extractor.Type))
                    continue;

                foreach (var searchValue in extractor.Extractor.Extract(propertyPath, propertyValue, context))
                {
                    yield return searchValue;
                }
            }
        }
    }

    private Contract DiscoverContract()
    {
        var properties = _type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var extractableProperties = new List<ExtractableProperty>();

        foreach (var property in properties)
        {
            if (!property.CanRead)
                continue;

            if (property.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                continue;

            if (property.GetCustomAttribute<NonIndexedAttribute>() != null)
                continue;

            var getMethod = property.GetGetMethod();
            if (getMethod is null)
                continue;

            var propertyType = property.PropertyType;

            var extractors = new List<ReferenceExtractor>();

            var extractor = _resolver.ResolveExtractor(propertyType);

            if (extractor != null)
                extractors.Add(new(propertyType, extractor));

            var allowedTypeAttr = property.GetCustomAttribute<AllowedTypesAttribute>(true);
            if (allowedTypeAttr != null)
            {
                foreach (var allowedType in allowedTypeAttr.AllowedTypes)
                {
                    var allowedTypeExtractor = _resolver.ResolveExtractor(allowedType);
                    if (allowedTypeExtractor != null)
                        extractors.Add(new(allowedType, allowedTypeExtractor));
                }
            }

            if (extractors.Count > 0)
                extractableProperties.Add(new(property.Name, getMethod, extractors));
        }

        return new(extractableProperties);
    }

    private class Contract
    {
        public Contract(IEnumerable<ExtractableProperty> extractableProperties)
        {
            ExtractableProperties = extractableProperties;
        }

        public IEnumerable<ExtractableProperty> ExtractableProperties { get; }
    }

    private class ExtractableProperty
    {
        public ExtractableProperty(string propertyName, MethodInfo getMethod, ICollection<ReferenceExtractor> extractors)
        {
            PropertyName = propertyName;
            GetMethod = getMethod;
            Extractors = extractors;
        }

        public ICollection<ReferenceExtractor> Extractors { get; }

        public MethodInfo GetMethod { get; }

        public string PropertyName { get; }
    }

    private class ReferenceExtractor
    {
        public ReferenceExtractor(Type type, ISearchIndexExtractor extractor)
        {
            Type = type;
            Extractor = extractor;
        }

        public ISearchIndexExtractor Extractor { get; }

        public Type Type { get; }
    }
}
