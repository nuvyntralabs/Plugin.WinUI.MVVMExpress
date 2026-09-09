namespace Plugin.WinUI.MVVMExpress.Caching;

/// <summary>How <see cref="ICachedFetcher"/> combines cache and network.</summary>
public enum FetchPolicy
{
    /// <summary>Return a cache hit; otherwise fetch and store.</summary>
    CacheFirst = 0,

    /// <summary>Fetch and store; on failure return cache when present.</summary>
    NetworkFirst = 1,

    /// <summary>Return cache immediately when present, then refresh in the background.</summary>
    StaleWhileRevalidate = 2,

    /// <summary>Always fetch; still write through to the cache.</summary>
    NetworkOnly = 3,

    /// <summary>Cache only. Miss is <see langword="default"/>.</summary>
    CacheOnly = 4
}
