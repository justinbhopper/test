namespace RH.Apollo.Persistence.Search;

public class FileValueExtractorFactory
{
    private readonly SearchIndexer _indexer;

    public FileValueExtractorFactory(SearchIndexer indexer)
    {
        _indexer = indexer;
    }

    public FileValueExtractor<T> Create<T>(string filePath)
        where T : notnull
    {
        var file = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        var extractor = new ValueExtractor(_indexer);
        return new FileValueExtractor<T>(extractor, file);
    }
}
