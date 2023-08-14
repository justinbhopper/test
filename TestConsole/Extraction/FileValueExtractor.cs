using System.Text;

namespace RH.Apollo.Persistence.Search;

public sealed class FileValueExtractor<T> : IAsyncDisposable
    where T : notnull
{
    private readonly ValueExtractor _extractor;
    private readonly StreamWriter _file;

    public FileValueExtractor(ValueExtractor extractor, FileStream file)
    {
        _extractor = extractor;
        _file = new StreamWriter(file, Encoding.UTF8, leaveOpen: false);
    }

    public async Task ExtractListAsync(IEnumerable<T> items)
    {
        var values = _extractor.ExtractList(items);

        foreach (var value in values)
        {
            await _file.WriteLineAsync($"{value.Key}\t{value.Value?.Replace("\t", "\\t") ?? "(null)"}");
        }

        await _file.FlushAsync();
    }

    public async Task ExtractObjectAsync(T item)
    {
        await ExtractListAsync(new T[] { item });
    }

    public async ValueTask DisposeAsync()
    {
        await _file.DisposeAsync();
    }
}
