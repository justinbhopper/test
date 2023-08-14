using Netsmart.Bedrock.CareFabric.Cdm.Entities;

namespace TestConsole.Netsmart;

public interface IFeed<T>
{
    int CurrentCount { get; }

    bool HasMoreResults { get; }

    int? TotalResults { get; }

    IPaginatedFeed<TResult> Paginate<TResult>(Func<T, IEnumerable<TResult>> selector)
        where TResult : SdkBase;

    IPaginatedFeed<TResult> Paginate<TResult>(int limit, Func<T, IEnumerable<TResult>> selector)
        where TResult : SdkBase;

    Task<IList<TResult>> ReadAllAsync<TResult>(Func<T, IEnumerable<TResult>> selector, CancellationToken cancellationToken)
        where TResult : SdkBase;

    Task<TResult> ReadNextAsync<TResult>(Func<T, TResult> selector, CancellationToken cancellationToken)
        where TResult : SdkBase;

    Task<IList<TResult>> ReadNextAsync<TResult>(Func<T, IEnumerable<TResult>> selector, CancellationToken cancellationToken)
        where TResult : SdkBase;

    Task<IList<TResult>> ReadNextAsync<TResult>(int limit, Func<T, IEnumerable<TResult>> selector, CancellationToken cancellationToken)
        where TResult : SdkBase;
}
