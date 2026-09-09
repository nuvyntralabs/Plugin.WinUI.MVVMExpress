using Plugin.WinUI.MVVMExpress.Caching;
using Plugin.WinUI.MVVMExpress.Connectivity;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Caching;

public sealed class CachedFetcherTests
{
    [Fact]
    public async Task CacheFirst_ReturnsCache_WithoutNetwork()
    {
        var cache = new MemoryCache();
        await cache.SetAsync("k", "cached");
        var calls = 0;
        var fetcher = new CachedFetcher(cache);
        var result = await fetcher.FetchAsync("k", _ =>
        {
            calls++;
            return Task.FromResult("fresh");
        }, FetchPolicy.CacheFirst);
        Assert.Equal("cached", result.Value);
        Assert.True(result.FromCache);
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task NetworkFirst_FallsBackToCache()
    {
        var cache = new MemoryCache();
        await cache.SetAsync("k", "cached");
        var fetcher = new CachedFetcher(cache);
        var result = await fetcher.FetchAsync<string>("k", _ => throw new InvalidOperationException("down"), FetchPolicy.NetworkFirst);
        Assert.Equal("cached", result.Value);
        Assert.True(result.FromCache);
    }

    [Fact]
    public async Task NetworkFirst_Offline_UsesCache()
    {
        var cache = new MemoryCache();
        await cache.SetAsync("k", "cached");
        var fetcher = new CachedFetcher(cache, new InMemoryConnectivityProbe { IsOnline = false });
        var result = await fetcher.FetchAsync("k", _ => Task.FromResult("fresh"), FetchPolicy.NetworkFirst);
        Assert.Equal("cached", result.Value);
        Assert.True(result.FromCache);
    }

    [Fact]
    public async Task StaleWhileRevalidate_ReturnsCache_ThenRefreshes()
    {
        var cache = new MemoryCache();
        await cache.SetAsync("k", "stale");
        var fetcher = new CachedFetcher(cache);
        var result = await fetcher.FetchAsync("k", _ => Task.FromResult("fresh"), FetchPolicy.StaleWhileRevalidate);
        Assert.Equal("stale", result.Value);
        Assert.True(result.FromCache);
        Assert.NotNull(fetcher.LastRefresh);
        await fetcher.LastRefresh;
        Assert.Equal("fresh", await cache.GetAsync<string>("k"));
    }
}
