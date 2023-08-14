namespace TestConsole.Locking;

public class LocalLock
{
    private static readonly Dictionary<object, RefCounted<SemaphoreSlim>> s_semaphores = new();
    private readonly string _name;

    public LocalLock(string name)
    {
        _name = name;
    }

    public async Task<IDisposable> AcquireAsync(TimeSpan waitTimeout, CancellationToken cancellationToken)
    {
        if (!await GetOrCreate(_name).WaitAsync(waitTimeout, cancellationToken))
            throw new Exception("Failed to acquire a lock within the alloted time.");

        return new Releaser(_name);
    }

    private static SemaphoreSlim GetOrCreate(string name)
    {
        lock (s_semaphores)
        {
            if (s_semaphores.TryGetValue(name, out var item))
            {
                ++item.RefCount;
                return item.Value;
            }
            else
            {
                item = new RefCounted<SemaphoreSlim>(new SemaphoreSlim(1, 1));
                s_semaphores[name] = item;
                return item.Value;
            }
        }
    }

    private class Releaser : IDisposable
    {
        private readonly string _name;

        public Releaser(string name)
        {
            _name = name;
        }

        public void Dispose()
        {
            RefCounted<SemaphoreSlim> item;
            lock (s_semaphores)
            {
                item = s_semaphores[_name];
                --item.RefCount;
                if (item.RefCount == 0)
                    s_semaphores.Remove(_name);
            }
            item.Value.Release();
        }
    }

    private sealed class RefCounted<T>
    {
        public RefCounted(T value)
        {
            RefCount = 1;
            Value = value;
        }

        public int RefCount { get; set; }

        public T Value { get; private set; }
    }
}
