using RH.Apollo.Persistence.Search.Converters;

namespace RH.Apollo.Persistence.Search.Extractors;

public class ConvertingSearchIndexExtractor : ISearchIndexExtractor
{
    private readonly ISearchValueConverter _converter;

    public ConvertingSearchIndexExtractor(ISearchValueConverter converter)
    {
        _converter = converter;
    }

    public IEnumerable<SearchIndexEntry> Extract(string currentPath, object value, SearchIndexExtractionContext context)
    {
        foreach (var searchValue in _converter.Convert(value))
        {
            yield return new(currentPath, searchValue, true);
        }
    }
}
