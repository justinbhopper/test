using System.Collections;
using RH.Apollo.Persistence.Search.Converters;

namespace RH.Apollo.Persistence.Search.Extractors;

public class ConvertingCollectionSearchIndexExtractor : ISearchIndexExtractor
{
    private readonly ISearchValueConverter _itemConverter;

    public ConvertingCollectionSearchIndexExtractor(ISearchValueConverter itemConverter)
    {
        _itemConverter = itemConverter;
    }

    public IEnumerable<SearchIndexEntry> Extract(string currentPath, object collection, SearchIndexExtractionContext context)
    {
        foreach (var item in (IEnumerable)collection)
        {
            foreach (var searchValue in _itemConverter.Convert(item))
            {
                yield return new(currentPath, searchValue, false);
            }
        }
    }
}
