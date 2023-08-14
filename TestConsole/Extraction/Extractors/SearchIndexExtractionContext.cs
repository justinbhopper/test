namespace RH.Apollo.Persistence.Search.Extractors;

public class SearchIndexExtractionContext
{
    private readonly IDictionary<object, ISet<string>> _seenReferences = new Dictionary<object, ISet<string>>();

    public bool IsCircularReference(object value, string path)
    {
        if (!_seenReferences.ContainsKey(value))
        {
            _seenReferences.Add(value, new HashSet<string> {path});
            return false;
        }

        var paths = _seenReferences[value];
        if (paths.Contains(path))
            return true;

        var nestedPath = paths.Any(p => path.StartsWith(p + "."));

        paths.Add(path);

        // If nested path, assume we're heading down a nested loop
        return nestedPath;
    }
}
