using Plugin.WinUI.MVVMExpress.Auth;
using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Core.Tests.Support;
using Plugin.WinUI.MVVMExpress.Navigation;
using Plugin.WinUI.MVVMExpress.Testing;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Navigation;

public sealed class NavigatorContractTests
{
    [Fact]
    public async Task ExistingTypedApis_StillWork_OnFakeNavigator()
    {
        INavigator navigator = new FakeNavigator();
        Assert.True((await navigator.NavigateToAsync<ProbeViewModel>()).IsSuccess);
        Assert.True((await navigator.NavigateToAsync<ProbeViewModel, int>(4)).IsSuccess);
        Assert.Equal(4, navigator.History[^1].Args);
        Assert.Equal(typeof(ProbeViewModel), navigator.Current);
        Assert.True((await navigator.GoBackAsync()).IsSuccess);
        Assert.Equal("back", navigator.History[^1].Args);
    }

    [Fact]
    public async Task CancelledToken_Throws_OnStackApis()
    {
        var navigator = new InMemoryNavigator().Map<ProbeViewModel>("probe");
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        await Assert.ThrowsAsync<OperationCanceledException>(() => navigator.NavigateToAsync<ProbeViewModel>(cts.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => navigator.NavigateToAsync<ProbeViewModel, int>(1, cts.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => navigator.NavigateToAsync("probe", cancellationToken: cts.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => navigator.GoBackAsync(cts.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => navigator.PopToRootAsync(cts.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => navigator.ReplaceAsync<ProbeViewModel>(cts.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => navigator.ResetAsync<ProbeViewModel>(cts.Token));
    }

    [Fact]
    public async Task EmptyOrNullRoute_Throws()
    {
        var navigator = new InMemoryNavigator();
        await Assert.ThrowsAsync<ArgumentException>(() => navigator.NavigateToAsync(" "));
        await Assert.ThrowsAsync<ArgumentNullException>(() => navigator.NavigateToAsync((string)null!));
    }

    [Fact]
    public async Task DirtyGuard_BlocksGoBackPopToRootAndReset()
    {
        var navigator = new InMemoryNavigator(_ => false);
        await navigator.NavigateToAsync<ProbeViewModel>();
        navigator.Map<DependentViewModel>("x");
        Assert.Equal("E_GUARD", (await navigator.GoBackAsync()).Error?.Code);
        Assert.Equal("E_GUARD", (await navigator.PopToRootAsync()).Error?.Code);
        Assert.Equal("E_GUARD", (await navigator.ResetAsync<DependentViewModel>()).Error?.Code);
        Assert.Equal([typeof(ProbeViewModel)], navigator.Stack);
    }

    [Fact]
    public async Task Guarded_ReplaceAndReset_RequireAuth()
    {
        var inner = new InMemoryNavigator();
        var auth = new MemoryAuth();
        var guarded = new GuardedNavigator(inner, auth, typeof(ProbeViewModel));
        Assert.Equal("E_AUTH", (await guarded.ReplaceAsync<ProbeViewModel>()).Error?.Code);
        Assert.Equal("E_AUTH", (await guarded.ResetAsync<ProbeViewModel>()).Error?.Code);
        Assert.Empty(inner.History);
        auth.IsAuthenticated = true;
        Assert.True((await guarded.ResetAsync<ProbeViewModel>()).IsSuccess);
        Assert.Equal([typeof(ProbeViewModel)], guarded.Stack);
    }

    [Fact]
    public async Task Guarded_GoBackAndPopToRoot_DoNotRequireAuth()
    {
        var inner = new InMemoryNavigator();
        await inner.NavigateToAsync<ProbeViewModel>();
        await inner.NavigateToAsync<DependentViewModel>();
        var guarded = new GuardedNavigator(inner, new MemoryAuth(), typeof(DependentViewModel));
        Assert.True((await guarded.GoBackAsync()).IsSuccess);
        Assert.Equal(typeof(ProbeViewModel), guarded.Current);
        Assert.True((await guarded.PopToRootAsync()).IsSuccess);
        Assert.Equal([typeof(ProbeViewModel)], guarded.Stack);
    }

    [Fact]
    public void Guarded_ForwardsWindow()
    {
        var inner = new InMemoryNavigator(window: new WindowContext("desk"));
        var guarded = new GuardedNavigator(inner, new MemoryAuth());
        Assert.Equal("desk", guarded.Window.WindowId);
    }

    [Fact]
    public void FakeNavigator_IsPageNavigator()
    {
        IPageNavigator navigator = new FakeNavigator();
        Assert.Equal("default", navigator.Window.WindowId);
        Assert.False(navigator.CanGoBack);
        Assert.Empty(navigator.Stack);
        Assert.Empty(navigator.ModalStack);
    }

    [Fact]
    public void WindowRegistry_TryGet_FalseWhenMissing()
    {
        var registry = new WindowNavigatorRegistry();
        Assert.False(registry.TryGetNavigator(new WindowContext("none"), out var navigator));
        Assert.Null(navigator);
    }

    [Fact]
    public void NavArgsApplier_NullViewModel_DoesNotThrow()
    {
        NavArgsApplier.ApplyTyped(null, 1);
        NavArgsApplier.ApplyQuery(null, new Dictionary<string, object> { ["A"] = "1" });
    }

    private sealed class MemoryAuth : IAuthState
    {
        public bool IsAuthenticated { get; set; }

        public string? UserName { get; set; }

        public Task<Plugin.WinUI.MVVMExpress.Outcome.Outcome> SignInAsync(
            string userName,
            string password,
            CancellationToken cancellationToken = default)
        {
            IsAuthenticated = true;
            UserName = userName;
            return Task.FromResult(Plugin.WinUI.MVVMExpress.Outcome.Outcome.Success());
        }

        public Task SignOutAsync(CancellationToken cancellationToken = default)
        {
            IsAuthenticated = false;
            UserName = null;
            return Task.CompletedTask;
        }
    }
}
