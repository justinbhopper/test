namespace System.ComponentModel.DataAnnotations;

public static class CollectionExtensions
{
    public static int FindIndex<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        var index = 0;

        foreach (var item in collection)
        {
            if (predicate(item))
                return index;

            index++;
        }

        return -1;
    }

    public static bool RemoveWhere<T>(this ICollection<T> collection, Func<T, bool> predicate)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        var matches = collection.Where(predicate).ToList();
        var anyMatches = false;

        foreach (var match in matches)
        {
            while (collection.Remove(match))
            {
                anyMatches = true;
            }
        }

        return anyMatches;
    }

    public static bool IsNullOrEmpty<T>(this ICollection<T>? collection) => collection is null || collection.Count == 0;

    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> range)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        foreach (var item in range)
        {
            collection.Add(item);
        }
    }

    public static T? TryGetSingle<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        using (var e = source.GetEnumerator())
        {
            while (e.MoveNext())
            {
                var result = e.Current;
                if (!predicate(result))
                    continue;

                while (e.MoveNext())
                {
                    // More than one result, return default
                    if (predicate(e.Current))
                        return default;
                }

                return result;
            }
        }

        // No result found, return default
        return default;
    }

    public static T GetOrAdd<T>(this ICollection<T> collection, Func<T, bool> selector, T addable)
    {
        var found = collection.FirstOrDefault(selector);
        if (found is not null)
            return found;

        collection.Add(addable);
        return addable;
    }

    public static T GetOrAdd<T>(this ICollection<T> collection, Func<T, bool> selector, Func<T> factory)
    {
        var found = collection.FirstOrDefault(selector);
        if (found is not null)
            return found;

        var created = factory();
        collection.Add(created);
        return created;
    }

    public static (ICollection<T> pass, ICollection<T> fail) Split<T>(this IEnumerable<T> source, Predicate<T> predicate)
    {
        var pass = new List<T>();
        var fail = new List<T>();

        foreach (var item in source)
        {
            if (predicate(item))
                pass.Add(item);
            else
                fail.Add(item);
        }

        return (pass, fail);
    }

    public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, T item)
    {
        return source.Concat(new[] { item });
    }

    public static K MaxOrDefault<T, K>(this IEnumerable<T> source, Func<T, K?> selector, K defaultValue)
    {
        if (!source.Any())
            return defaultValue;

        return source.Max(selector) ?? defaultValue;
    }

    public static K MinOrDefault<T, K>(this IEnumerable<T> source, Func<T, K?> selector, K defaultValue)
    {
        if (!source.Any())
            return defaultValue;

        return source.Min(selector) ?? defaultValue;
    }

    [SuppressMessage("Design", "CA1068:CancellationToken parameters must come last", Justification = "Consistency with Parallel.ForEachAsync")]
    public static Task ForEachAsync<TIn>(this IEnumerable<TIn> source, int maxDegreeOfParallelism, CancellationToken cancellationToken, Func<TIn, CancellationToken, ValueTask> body)
    {
        var thunk = new object();
        return source.ForEachAsync(maxDegreeOfParallelism, cancellationToken, async (i, ct) =>
        {
            await body(i, ct);
            return thunk;
        });
    }

    [SuppressMessage("Design", "CA1068:CancellationToken parameters must come last", Justification = "Consistency with Parallel.ForEachAsync")]
    public static async Task<TOut[]> ForEachAsync<TIn, TOut>(this IEnumerable<TIn> source, int maxDegreeOfParallelism, CancellationToken cancellationToken, Func<TIn, CancellationToken, ValueTask<TOut>> body)
    {
        using var throttler = new SemaphoreSlim(maxDegreeOfParallelism, maxDegreeOfParallelism);

        return await Task.WhenAll(source.Select(async input =>
        {
            await throttler.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return await body(input, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                throttler.Release();
            }
        }));
    }
}
