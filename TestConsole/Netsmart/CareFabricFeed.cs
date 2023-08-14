using Netsmart.Bedrock.CareFabric.Cdm.Entities;

namespace TestConsole.Netsmart;

internal sealed class CareFabricFeed<T> : IFeed<T>
    where T : PayloadBase
{
    private readonly ICareFabricClient _client;
    private readonly IPayload _payload;
    private int _count;
    private int _totalCount = -1;

    public CareFabricFeed(ICareFabricClient client, IPayload payload)
    {
        _client = client;
        _payload = payload;
    }

    public bool HasMoreResults { get; private set; } = true;

    public int CurrentCount => _count;

    public int? TotalResults => _totalCount >= 0 ? _totalCount : null;

    public IPaginatedFeed<TResult> Paginate<TResult>(Func<T, IEnumerable<TResult>> selector)
        where TResult : SdkBase
    {
        return Paginate(50, selector);
    }

    public IPaginatedFeed<TResult> Paginate<TResult>(int limit, Func<T, IEnumerable<TResult>> selector)
        where TResult : SdkBase
    {
        return new PaginatedFeed<T, TResult>(this, limit, selector);
    }

    public async Task<IList<TResult>> ReadAllAsync<TResult>(Func<T, IEnumerable<TResult>> selector, CancellationToken cancellationToken)
        where TResult : SdkBase
    {
        // Force high page size
        _payload.Value.PageSize = 50;

        var results = new List<TResult>();

        while (HasMoreResults)
        {
            results.AddRange(await ReadNextAsync(selector, cancellationToken));
        }

        return results;
    }

    public async Task<TResult> ReadNextAsync<TResult>(Func<T, TResult> selector, CancellationToken cancellationToken)
        where TResult : SdkBase
    {
        if (!HasMoreResults)
            throw new InvalidOperationException("No more results.  Check HasMoreResults before reading for more.");

        _payload.Value.PageIndex = 0;
        _payload.Value.PageSize = 1;

        var response = await _client.RequestAsync(_payload, cancellationToken);
        var result = await response.ReadAsAsync<T>(cancellationToken);

        HasMoreResults = false;

        Interlocked.Exchange(ref _count, 1);

        return selector(result);
    }

    public Task<IList<TResult>> ReadNextAsync<TResult>(Func<T, IEnumerable<TResult>> selector, CancellationToken cancellationToken)
        where TResult : SdkBase
    {
        return ReadNextAsync(50, selector, cancellationToken);
    }

    public async Task<IList<TResult>> ReadNextAsync<TResult>(int limit, Func<T, IEnumerable<TResult>> selector, CancellationToken cancellationToken)
        where TResult : SdkBase
    {
        if (!HasMoreResults)
            throw new InvalidOperationException("No more results.  Check HasMoreResults before reading for more.");

        _payload.Value.PageIndex ??= 0;
        _payload.Value.PageSize = limit;

        var response = await _client.RequestAsync(_payload, cancellationToken);
        var result = await response.ReadAsAsync<T>(cancellationToken);

        var items = selector(result).ToList();

        // Prepare next page request
        _payload.Value.PageIndex += 1;

        HasMoreResults = result.HasMoreResults(_payload.Value.PageSize, _count, items.Count);

        Interlocked.Add(ref _count, items.Count);

        if (result.TotalRecordCount.HasValue)
            Interlocked.Exchange(ref _totalCount, result.TotalRecordCount.Value);

        return items;
    }

    private class PaginatedFeed<TPayload, TResult> : IPaginatedFeed<TResult>
        where TResult : SdkBase
    {
        private readonly IFeed<TPayload> _feed;
        private readonly int _limit;
        private readonly Func<TPayload, IEnumerable<TResult>> _selector;

        public PaginatedFeed(IFeed<TPayload> feed, int limit, Func<TPayload, IEnumerable<TResult>> selector)
        {
            _feed = feed;
            _limit = limit;
            _selector = selector;
        }

        public int CurrentCount => _feed.CurrentCount;

        public bool HasMoreResults => _feed.HasMoreResults;

        public int? TotalResults => _feed.TotalResults;

        public Task<IList<TResult>> ReadNextAsync(CancellationToken cancellationToken)
        {
            return _feed.ReadNextAsync(_limit, _selector, cancellationToken);
        }
    }
}

