using RH.Apollo.Contracts.Resources;

namespace RH.Apollo.Persistence.Search;

public class SearchIndexer
{
    private readonly SearchIndexResolver _resolver;

    public SearchIndexer(SearchIndexResolver resolver)
    {
        _resolver = resolver;
    }

    public IReadOnlyCollection<SearchIndexEntry> Extract<T>(T item)
        where T : notnull
    {
        var extractor = _resolver.ResolveExtractor(typeof(T));

        if (extractor is null)
            throw new InvalidOperationException($"Failed to resolve search index extractor for {typeof(T)}.");

        return new HashSet<SearchIndexEntry>(extractor.Extract(string.Empty, item, new()));
    }
}
