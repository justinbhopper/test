namespace RH.Apollo.Persistence.Search;

public class ValueExtractor
{
    private readonly Dictionary<string, ValueSet> _memory = new();
    private readonly SearchIndexer _indexer;

    public ValueExtractor(SearchIndexer indexer)
    {
        _indexer = indexer;
    }

    public IEnumerable<KeyValuePair<string, string?>> ExtractList<T>(IEnumerable<T> items)
        where T : notnull
    {
        var newValues = new List<KeyValuePair<string, string?>>();

        foreach (var item in items)
        {
            var indexValues = _indexer.Extract(item);

            foreach (var indexValue in indexValues)
            {
                ValueSet bucket;
                if (_memory.ContainsKey(indexValue.ParamName))
                {
                    bucket = _memory[indexValue.ParamName];
                }
                else
                {
                    bucket = new ValueSet();
                    _memory[indexValue.ParamName] = bucket;
                }

                // Special consideration to make sure NULL is always present in the bucket
                if (indexValue.Value is null)
                {
                    if (!bucket.Contains(null))
                    {
                        bucket.HasSeenNull = true;

                        // Report null if we've seen it in combination with a non-null value
                        if (bucket.Count > 0)
                            newValues.Add(new(indexValue.ParamName, indexValue.Value));
                    }
                }
                // Otherwise, limit how many values we care to record
                else if (bucket.Count < 5)
                {
                    bucket.Add(indexValue.Value);
                    newValues.Add(new(indexValue.ParamName, indexValue.Value));
                }
            }
        }

        return newValues;
    }

    public IEnumerable<KeyValuePair<string, string?>> ExtractObject<T>(T item)
        where T : notnull
    {
        return ExtractList(new T[] { item });
    }

    private class ValueSet : HashSet<string?>
    {
        public bool HasSeenNull { get; set; }
    }
}
