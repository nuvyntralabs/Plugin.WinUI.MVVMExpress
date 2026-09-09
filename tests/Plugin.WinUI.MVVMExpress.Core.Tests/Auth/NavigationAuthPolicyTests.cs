using Plugin.WinUI.MVVMExpress.Auth;
using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Navigation;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Auth;

public sealed class NavigationAuthPolicyTests
{
    [Fact]
    public async Task Policy_RequiresAuth_BlocksAnonymous()
    {
        var auth = new TestAuth();
        var inner = new InMemoryNavigator();
        var policy = new NavigationAuthPolicy(authRequired: [typeof(SecureVm)]);
        var navigator = new GuardedNavigator(inner, auth, policy);
        var blocked = await navigator.NavigateToAsync<SecureVm>();
        Assert.False(blocked.IsSuccess);
        Assert.Equal("E_AUTH", blocked.Error?.Code);
        auth.IsAuthenticated = true;
        var ok = await navigator.NavigateToAsync<SecureVm>();
        Assert.True(ok.IsSuccess);
    }

    [Fact]
    public async Task Policy_RequiresRole_BlocksWithoutRole()
    {
        var auth = new TestAuth { IsAuthenticated = true };
        var inner = new InMemoryNavigator();
        var policy = new NavigationAuthPolicy(
            authRequired: [typeof(AdminVm)],
            roles: new Dictionary<Type, string> { [typeof(AdminVm)] = "admin" });
        var navigator = new GuardedNavigator(inner, auth, policy);
        var blocked = await navigator.NavigateToAsync<AdminVm>();
        Assert.Equal("E_ROLE", blocked.Error?.Code);
        auth.Grant("admin");
        var ok = await navigator.NavigateToAsync<AdminVm>();
        Assert.True(ok.IsSuccess);
    }

    private sealed class SecureVm : ViewModel;

    private sealed class AdminVm : ViewModel;

    private sealed class TestAuth : IAuthState, IRoleState
    {
        private readonly HashSet<string> _roles = new(StringComparer.OrdinalIgnoreCase);

        public bool IsAuthenticated { get; set; }

        public string? UserName => IsAuthenticated ? "ada" : null;

        public Task<Plugin.WinUI.MVVMExpress.Outcome.Outcome> SignInAsync(string userName, string password, CancellationToken cancellationToken = default)
            => Task.FromResult(Plugin.WinUI.MVVMExpress.Outcome.Outcome.Success());

        public Task SignOutAsync(CancellationToken cancellationToken = default)
        {
            IsAuthenticated = false;
            _roles.Clear();
            return Task.CompletedTask;
        }

        public bool HasRole(string role) => _roles.Contains(role);

        public void Grant(string role) => _roles.Add(role);
    }
}
