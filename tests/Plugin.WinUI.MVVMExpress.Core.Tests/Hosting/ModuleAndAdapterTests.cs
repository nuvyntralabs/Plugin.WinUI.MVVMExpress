using Microsoft.Extensions.DependencyInjection;
using Plugin.WinUI.MVVMExpress.Auth;
using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Hosting;
using Plugin.WinUI.MVVMExpress.Navigation;
using Result = Plugin.WinUI.MVVMExpress.Outcome.Outcome;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Hosting;

public sealed class ModuleAndAdapterTests
{
    [Fact]
    public void AddModule_RegistersFeatureServices()
    {
        var services = new ServiceCollection().AddModule<CatalogModule>();
        using var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetRequiredService<CatalogViewModel>());
    }

    [Fact]
    public void AddDeepLinks_WithoutBridge_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddDeepLinks());
        Assert.Contains("IDeepLinkBridge", ex.Message);
    }

    [Fact]
    public async Task AddDeepLinks_Bridge_Navigates()
    {
        var navigator = new InMemoryNavigator().Map<CatalogViewModel>("catalog");
        var services = new ServiceCollection().AddDeepLinks(new StaticDeepLink("catalog"));
        using var provider = services.BuildServiceProvider();
        var result = await provider.GetRequiredService<IDeepLinkBridge>().NavigateAsync("app://catalog", navigator);
        Assert.True(result.IsSuccess);
        Assert.Equal(typeof(CatalogViewModel), navigator.Current);
    }

    [Fact]
    public void AddSecureSessionAuth_WithoutFactory_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddSecureSessionAuth());
        Assert.Contains("IAuthState", ex.Message);
    }

    [Fact]
    public void AddSecureSessionAuth_Factory_ReplacesAuthState()
    {
        var services = new ServiceCollection().AddSingleton<IAuthState, MemoryAuth>();
        services.AddSecureSessionAuth(_ => new MemoryAuth { SignedIn = true });
        using var provider = services.BuildServiceProvider();
        Assert.True(provider.GetRequiredService<IAuthState>().IsAuthenticated);
    }

    private sealed class CatalogModule : IModule
    {
        public void Configure(IServiceCollection services) => services.AddTransient<CatalogViewModel>();
    }

    private sealed class CatalogViewModel : ViewModel;

    private sealed class StaticDeepLink(string route) : IDeepLinkBridge
    {
        public Task<Result> NavigateAsync(string uri, INavigator navigator, CancellationToken cancellationToken = default)
            => navigator.NavigateToAsync(route, cancellationToken: cancellationToken);
    }

    private sealed class MemoryAuth : IAuthState
    {
        public bool SignedIn { get; set; }
        public bool IsAuthenticated => SignedIn;
        public string? UserName => SignedIn ? "demo" : null;
        public Task<Plugin.WinUI.MVVMExpress.Outcome.Outcome> SignInAsync(string userName, string password, CancellationToken cancellationToken = default)
        {
            SignedIn = true;
            return Task.FromResult(Plugin.WinUI.MVVMExpress.Outcome.Outcome.Success());
        }

        public Task SignOutAsync(CancellationToken cancellationToken = default)
        {
            SignedIn = false;
            return Task.CompletedTask;
        }
    }
}
