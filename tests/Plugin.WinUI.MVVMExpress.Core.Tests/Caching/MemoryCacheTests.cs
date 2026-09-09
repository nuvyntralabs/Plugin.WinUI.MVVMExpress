using Plugin.WinUI.MVVMExpress.Caching;
using Plugin.WinUI.MVVMExpress.Connectivity;
using Plugin.WinUI.MVVMExpress.Dialogs;
using Plugin.WinUI.MVVMExpress.Testing;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Caching;

public sealed class MemoryCacheTests
{
    [Fact]
    public async Task SetGetRemove_RoundTrips()
    {
        var cache = new MemoryCache();
        await cache.SetAsync("k", 7);
        Assert.Equal(7, await cache.GetAsync<int>("k"));
        await cache.RemoveAsync("k");
        Assert.Equal(0, await cache.GetAsync<int>("k"));
    }

    [Fact]
    public void ConnectivityProbe_IsMutable()
    {
        var probe = new InMemoryConnectivityProbe { IsOnline = false };
        Assert.False(probe.IsOnline);
        probe.IsOnline = true;
        Assert.True(probe.IsOnline);
    }

    [Fact]
    public async Task NullDialogs_Completes()
    {
        await NullDialogs.Instance.AlertAsync("t", "m");
        Assert.True(await NullDialogs.Instance.ConfirmAsync("t", "m"));
    }

    [Fact]
    public async Task FakeDialogs_RecordsAlerts()
    {
        var dialogs = new FakeDialogs { ConfirmResult = false };
        await dialogs.AlertAsync("Hi", "there");
        Assert.False(await dialogs.ConfirmAsync("Q", "sure?"));
        Assert.Equal(["Hi:there", "confirm:Q"], dialogs.Alerts);
    }
}
