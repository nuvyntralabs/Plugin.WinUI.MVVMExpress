using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Navigation;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Navigation;

public sealed class WindowNavigatorRegistryTests
{
    [Fact]
    public async Task Register_GetNavigator_IsPerWindow()
    {
        var registry = new WindowNavigatorRegistry();
        var first = new InMemoryNavigator(window: new WindowContext("one"));
        var second = new InMemoryNavigator(window: new WindowContext("two"));
        registry.Register(first.Window, first);
        registry.Register(second.Window, second);
        registry.CurrentWindow = first.Window;

        await first.NavigateToAsync<EmptyViewModel>();
        Assert.Same(first, registry.GetCurrent());
        Assert.Same(second, registry.GetNavigator(second.Window));
        Assert.Equal(typeof(EmptyViewModel), first.Current);
        Assert.Null(second.Current);
    }

    [Fact]
    public void GetNavigator_Unknown_Throws()
    {
        var registry = new WindowNavigatorRegistry();
        Assert.Throws<KeyNotFoundException>(() => registry.GetNavigator(new WindowContext("missing")));
    }

    private sealed class EmptyViewModel : ViewModel;
}
