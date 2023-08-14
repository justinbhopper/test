namespace TestConsole.Netsmart;

public interface IPaginatedFeed<T>
{
    int CurrentCount { get; }

    bool HasMoreResults { get; }

    int? TotalResults { get; }

    Task<IList<T>> ReadNextAsync(CancellationToken cancellationToken);
}
