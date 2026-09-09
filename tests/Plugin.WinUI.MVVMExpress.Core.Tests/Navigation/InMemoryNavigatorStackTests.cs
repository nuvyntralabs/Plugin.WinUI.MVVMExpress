using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Core.Tests.Support;
using Plugin.WinUI.MVVMExpress.Navigation;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Navigation;

public sealed class InMemoryNavigatorStackTests
{
    [Fact]
    public async Task Push_Pop_PopToRoot_Replace_Reset()
    {
        var navigator = new InMemoryNavigator()
            .Map<HomeVm>("home")
            .Map<DetailsVm>("details");

        Assert.False(navigator.CanGoBack);
        Assert.True((await navigator.NavigateToAsync<HomeVm>()).IsSuccess);
        Assert.True((await navigator.NavigateToAsync<DetailsVm>()).IsSuccess);
        Assert.Equal([typeof(HomeVm), typeof(DetailsVm)], navigator.Stack);
        Assert.True(navigator.CanGoBack);
        Assert.Equal(typeof(DetailsVm), navigator.Current);

        Assert.True((await navigator.GoBackAsync()).IsSuccess);
        Assert.Equal(typeof(HomeVm), navigator.Current);
        Assert.False(navigator.CanGoBack);

        Assert.True((await navigator.NavigateToAsync<DetailsVm>()).IsSuccess);
        Assert.True((await navigator.ReplaceAsync<HomeVm>()).IsSuccess);
        Assert.Equal([typeof(HomeVm), typeof(HomeVm)], navigator.Stack);

        Assert.True((await navigator.ResetAsync<DetailsVm>()).IsSuccess);
        Assert.Equal([typeof(DetailsVm)], navigator.Stack);
        Assert.False(navigator.CanGoBack);

        Assert.True((await navigator.NavigateToAsync<HomeVm>()).IsSuccess);
        Assert.True((await navigator.PopToRootAsync()).IsSuccess);
        Assert.Equal([typeof(DetailsVm)], navigator.Stack);
    }

    [Fact]
    public async Task Modal_PopsBeforePageStack()
    {
        var navigator = new InMemoryNavigator();
        await navigator.NavigateToAsync<HomeVm>();
        var modal = await navigator.NavigateToAsync(
            "missing",
            null,
            new NavOptions { Modal = true });
        Assert.Equal("E_ROUTE", modal.Error?.Code);

        navigator.Map<DetailsVm>("details");
        Assert.True((await navigator.NavigateToAsync("details", null, new NavOptions { Modal = true })).IsSuccess);
        Assert.Equal([typeof(DetailsVm)], navigator.ModalStack);
        Assert.True(navigator.CanGoBack);
        await navigator.GoBackAsync();
        Assert.Empty(navigator.ModalStack);
        Assert.Equal(typeof(HomeVm), navigator.Current);
    }

    [Fact]
    public async Task RouteAndQuery_AreRecorded()
    {
        var navigator = new InMemoryNavigator().Map<DetailsVm>("details");
        var query = new Dictionary<string, object> { ["ProductId"] = 7 };
        var result = await navigator.NavigateToAsync("details?Source=uri", query);
        Assert.True(result.IsSuccess);
        Assert.Equal(typeof(DetailsVm), navigator.Current);
        Assert.Equal("details", navigator.History[0].Route);
        Assert.Equal("7", navigator.History[0].Query?["ProductId"]?.ToString());
        Assert.Equal("uri", navigator.History[0].Query?["Source"]?.ToString());
    }

    [Fact]
    public async Task UnknownRoute_ReturnsE_ROUTE()
    {
        var result = await new InMemoryNavigator().NavigateToAsync("nope");
        Assert.Equal("E_ROUTE", result.Error?.Code);
    }

    [Fact]
    public async Task DirtyGuard_BlocksStackMutation()
    {
        var navigator = new InMemoryNavigator(_ => false) { Current = typeof(ProbeViewModel) };
        var blocked = await navigator.NavigateToAsync("x");
        Assert.Equal("E_ROUTE", blocked.Error?.Code);
        navigator.Map<DependentViewModel>("x");
        var guard = await navigator.NavigateToAsync("x");
        Assert.Equal("E_GUARD", guard.Error?.Code);
        Assert.Empty(navigator.Stack);
    }

    [Fact]
    public void Window_DefaultsAndOverrides()
    {
        Assert.Equal("default", new InMemoryNavigator().Window.WindowId);
        Assert.Equal("second", new InMemoryNavigator(window: new WindowContext("second")).Window.WindowId);
    }

    private sealed class HomeVm : ViewModel;

    private sealed class DetailsVm : ViewModel;
}
