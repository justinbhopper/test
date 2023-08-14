using System.Collections;

namespace RH.Apollo.Persistence.Search.Extractors;

public class ExtractingCollectionSearchIndexExtractor : ISearchIndexExtractor
{
    private readonly ISearchIndexExtractor _itemExtractor;

    public ExtractingCollectionSearchIndexExtractor(ISearchIndexExtractor itemExtractor)
    {
        _itemExtractor = itemExtractor;
    }

    public IEnumerable<SearchIndexEntry> Extract(string currentPath, object collection, SearchIndexExtractionContext context)
    {
        foreach (var item in (IEnumerable)collection)
        {
            foreach (var index in _itemExtractor.Extract(currentPath, item, context))
            {
                if (index.IsUnique)
                    yield return new(index.ParamName, index.Value, false);
                else
                    yield return index;
            }
        }
    }
}
