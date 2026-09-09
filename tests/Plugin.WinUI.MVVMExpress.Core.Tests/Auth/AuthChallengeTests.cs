using Plugin.WinUI.MVVMExpress.Auth;
using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Navigation;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Auth;

public sealed class AuthChallengeTests
{
    [Fact]
    public async Task GuardedNavigator_OpensChallenge_ThenResumes()
    {
        var auth = new ChallengeAuth();
        var inner = new InMemoryNavigator()
            .Map<LoginVm>("login")
            .Map<HomeVm>("home");
        var navigator = new GuardedNavigator(
            inner,
            auth,
            policy: new NavigationAuthPolicy(authRequired: [typeof(HomeVm)]),
            options: new GuardedNavigatorOptions { ChallengeViewModel = typeof(LoginVm) });

        var blocked = await navigator.NavigateToAsync<HomeVm>();
        Assert.True(blocked.IsSuccess);
        Assert.Equal(typeof(LoginVm), inner.Current);

        await auth.SignInAsync("ada", "secret");
        await Task.Delay(20);
        Assert.Equal(typeof(HomeVm), inner.Current);
    }

    [Fact]
    public async Task ResetAsync_ReplaceRoot_IsAllowedWhenAuthenticated()
    {
        var auth = new ChallengeAuth { IsAuthenticated = true };
        var inner = new InMemoryNavigator().Map<HomeVm>("//home");
        var navigator = new GuardedNavigator(inner, auth, typeof(HomeVm));
        var result = await navigator.ResetAsync<HomeVm>();
        Assert.True(result.IsSuccess);
        Assert.Equal(typeof(HomeVm), inner.Current);
        Assert.False(inner.CanGoBack);
    }

    private sealed class LoginVm : ViewModel;

    private sealed class HomeVm : ViewModel;

    private sealed class ChallengeAuth : IAuthState
    {
        public bool IsAuthenticated { get; set; }

        public string? UserName => IsAuthenticated ? "ada" : null;

        public string? Email => IsAuthenticated ? "ada@example.com" : null;

        public event EventHandler? Changed;

        public Task<Plugin.WinUI.MVVMExpress.Outcome.Outcome> SignInAsync(string userName, string password, CancellationToken cancellationToken = default)
        {
            IsAuthenticated = true;
            Changed?.Invoke(this, EventArgs.Empty);
            return Task.FromResult(Plugin.WinUI.MVVMExpress.Outcome.Outcome.Success());
        }

        public Task SignOutAsync(CancellationToken cancellationToken = default)
        {
            IsAuthenticated = false;
            Changed?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }
    }
}
