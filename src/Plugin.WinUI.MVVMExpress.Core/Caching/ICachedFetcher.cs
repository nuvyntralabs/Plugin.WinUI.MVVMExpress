namespace Plugin.WinUI.MVVMExpress.Caching;

/// <summary>Applies a <see cref="FetchPolicy"/> on top of <see cref="ICache"/>.</summary>
public interface ICachedFetcher
{
    /// <summary>Gets or fetches a value using <paramref name="policy"/>.</summary>
    /// <param name="key">Cache key.</param>
    /// <param name="fetch">Network / source loader.</param>
    /// <param name="policy">Cache / network policy.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    Task<CachedFetchResult<T>> FetchAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> fetch,
        FetchPolicy policy,
        CancellationToken cancellationToken = default);
}

/// <summary>Result of a cached fetch.</summary>
/// <param name="Value">Payload.</param>
/// <param name="FromCache">Whether <paramref name="Value"/> came from the cache.</param>
public readonly record struct CachedFetchResult<T>(T? Value, bool FromCache);
