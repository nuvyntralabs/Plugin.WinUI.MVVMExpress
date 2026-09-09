using Plugin.WinUI.MVVMExpress.Testing;
using Xunit;

namespace App1.Tests;

public sealed class AppViewModelTests
{
    [Fact]
    public async Task Home_Increment_UpdatesCount()
    {
        var dialogs = new FakeDialogs();
        var vm = new HomeViewModel(new FakeNavigator(), dialogs, dialogs, new MemoryItemStore());

        await vm.IncrementCommand.ExecuteAsync();

        Assert.Equal(1, vm.Count);
    }

    [Fact]
    public async Task Home_OpenEdit_Navigates()
    {
        var navigator = new FakeNavigator().Map<EditViewModel>("edit");
        var dialogs = new FakeDialogs();
        var vm = new HomeViewModel(navigator, dialogs, dialogs, new MemoryItemStore());

        await vm.OpenEditCommand.ExecuteAsync();

        Assert.Equal(typeof(EditViewModel), navigator.Current);
    }

    [Fact]
    public async Task SignIn_WithDemoCredentials_ReplacesRootWithHome()
    {
        var auth = new DemoAuthState();
        var navigator = new FakeNavigator()
            .Map<LoginViewModel>("login")
            .Map<HomeViewModel>("home");
        var vm = new LoginViewModel(navigator, new FakeDialogs(), auth);

        await vm.SignInCommand.ExecuteAsync();

        Assert.True(auth.IsAuthenticated);
        Assert.Equal(typeof(HomeViewModel), navigator.Current);
    }

    [Fact]
    public async Task SignIn_WithBadPassword_ShowsError_AndStaysOnLogin()
    {
        var auth = new DemoAuthState();
        var navigator = new FakeNavigator().Map<LoginViewModel>("login");
        var dialogs = new FakeDialogs();
        var vm = new LoginViewModel(navigator, dialogs, auth) { Password = "wrong" };

        await vm.SignInCommand.ExecuteAsync();

        Assert.False(auth.IsAuthenticated);
        Assert.Contains(dialogs.Alerts, item => item.Contains("Invalid", StringComparison.Ordinal));
        Assert.NotEqual(typeof(HomeViewModel), navigator.Current);
    }
}
