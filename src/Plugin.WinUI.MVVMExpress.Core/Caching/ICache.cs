namespace Plugin.WinUI.MVVMExpress.Caching;

/// <summary>Key/value cache. Production apps should adapt Plugin.Maui.ApiCache.</summary>
public interface ICache
{
    /// <summary>Gets a cached value.</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>Stores <paramref name="value"/>.</summary>
    Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default);

    /// <summary>Removes <paramref name="key"/>.</summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}

/// <summary>Process-memory <see cref="ICache"/>.</summary>
public sealed class MemoryCache : ICache
{
    private readonly Dictionary<string, object?> _map = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrEmpty(key);
        return Task.FromResult(_map.TryGetValue(key, out var value) && value is T typed ? typed : default);
    }

    /// <inheritdoc />
    public Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrEmpty(key);
        _map[key] = value;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrEmpty(key);
        _map.Remove(key);
        return Task.CompletedTask;
    }
}
