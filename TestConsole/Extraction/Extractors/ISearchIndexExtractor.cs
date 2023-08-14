namespace RH.Apollo.Persistence.Search.Extractors;

public interface ISearchIndexExtractor
{
    IEnumerable<SearchIndexEntry> Extract(string currentPath, object value, SearchIndexExtractionContext context);
}
