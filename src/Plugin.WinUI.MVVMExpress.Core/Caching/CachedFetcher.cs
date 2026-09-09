using Plugin.WinUI.MVVMExpress.Connectivity;

namespace Plugin.WinUI.MVVMExpress.Caching;

/// <summary>In-process <see cref="ICachedFetcher"/>. Production apps should adapt Plugin.Maui.ApiCache.</summary>
public sealed class CachedFetcher : ICachedFetcher
{
    private readonly ICache _cache;
    private readonly IConnectivityProbe? _connectivity;

    /// <summary>Creates a fetcher.</summary>
    /// <param name="cache">Backing cache.</param>
    /// <param name="connectivity">When offline, network policies fall back to cache.</param>
    public CachedFetcher(ICache cache, IConnectivityProbe? connectivity = null)
    {
        ArgumentNullException.ThrowIfNull(cache);
        _cache = cache;
        _connectivity = connectivity;
    }

    /// <summary>Last background refresh started by <see cref="FetchPolicy.StaleWhileRevalidate"/>.</summary>
    public Task? LastRefresh { get; private set; }

    /// <summary>Exception from the last background refresh, if it failed.</summary>
    public Exception? LastRefreshError { get; private set; }

    /// <inheritdoc />
    public async Task<CachedFetchResult<T>> FetchAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> fetch,
        FetchPolicy policy,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(fetch);
        cancellationToken.ThrowIfCancellationRequested();

        var cached = await _cache.GetAsync<T>(key, cancellationToken).ConfigureAwait(false);
        var online = _connectivity?.IsOnline != false;

        switch (policy)
        {
            case FetchPolicy.CacheOnly:
                return new CachedFetchResult<T>(cached, FromCache: true);

            case FetchPolicy.CacheFirst:
                if (cached is not null)
                {
                    return new CachedFetchResult<T>(cached, FromCache: true);
                }

                return await NetworkAndStoreAsync(key, fetch, cancellationToken).ConfigureAwait(false);

            case FetchPolicy.NetworkOnly:
                return await NetworkAndStoreAsync(key, fetch, cancellationToken).ConfigureAwait(false);

            case FetchPolicy.NetworkFirst:
                if (!online && cached is not null)
                {
                    return new CachedFetchResult<T>(cached, FromCache: true);
                }

                try
                {
                    return await NetworkAndStoreAsync(key, fetch, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception) when (cached is not null)
                {
                    return new CachedFetchResult<T>(cached, FromCache: true);
                }

            case FetchPolicy.StaleWhileRevalidate:
                if (cached is not null)
                {
                    LastRefresh = RefreshQuietlyAsync(key, fetch, cancellationToken);
                    return new CachedFetchResult<T>(cached, FromCache: true);
                }

                return await NetworkAndStoreAsync(key, fetch, cancellationToken).ConfigureAwait(false);

            default:
                throw new ArgumentOutOfRangeException(nameof(policy), policy, "Unknown fetch policy.");
        }
    }

    private async Task<CachedFetchResult<T>> NetworkAndStoreAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> fetch,
        CancellationToken cancellationToken)
    {
        var fresh = await fetch(cancellationToken).ConfigureAwait(false);
        await _cache.SetAsync(key, fresh, cancellationToken).ConfigureAwait(false);
        return new CachedFetchResult<T>(fresh, FromCache: false);
    }

    private async Task RefreshQuietlyAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> fetch,
        CancellationToken cancellationToken)
    {
        try
        {
            LastRefreshError = null;
            await NetworkAndStoreAsync(key, fetch, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LastRefreshError = ex;
        }
    }
}