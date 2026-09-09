using Microsoft.Extensions.DependencyInjection;
using Plugin.WinUI.MVVMExpress.Caching;
using Plugin.WinUI.MVVMExpress.Composition;
using Plugin.WinUI.MVVMExpress.Connectivity;
using Plugin.WinUI.MVVMExpress.Dialogs;
using Plugin.WinUI.MVVMExpress.Files;
using Plugin.WinUI.MVVMExpress.Flags;
using Plugin.WinUI.MVVMExpress.Hosting;
using Plugin.WinUI.MVVMExpress.Media;
using Plugin.WinUI.MVVMExpress.Messaging;
using Plugin.WinUI.MVVMExpress.Navigation;
using Plugin.WinUI.MVVMExpress.Operations;
using Plugin.WinUI.MVVMExpress.Permissions;
using Plugin.WinUI.MVVMExpress.Threading;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Hosting;

public sealed class AddMvvmExpressTests
{
    [Fact]
    public void AddMvvmExpress_RegistersCoreSingletons()
    {
        using var provider = new ServiceCollection().AddMvvmExpress().BuildServiceProvider();
        Assert.IsType<MessageHub>(provider.GetRequiredService<IMessageHub>());
        Assert.IsType<MemoryCache>(provider.GetRequiredService<ICache>());
        Assert.IsType<InMemoryConnectivityProbe>(provider.GetRequiredService<IConnectivityProbe>());
        var navigator = Assert.IsType<InMemoryNavigator>(provider.GetRequiredService<INavigator>());
        Assert.Same(navigator, provider.GetRequiredService<IPageNavigator>());
        Assert.Same(ImmediateMainThread.Instance, provider.GetRequiredService<IMainThread>());
        Assert.IsType<NullDialogs>(provider.GetRequiredService<IDialogs>());
        Assert.Same(NullDialogs.Instance, provider.GetRequiredService<INotifier>());
        Assert.Equal("default", provider.GetRequiredService<IWindowContext>().WindowId);
        Assert.IsType<WindowNavigatorRegistry>(provider.GetRequiredService<IWindowNavigatorRegistry>());
        Assert.IsType<CachedFetcher>(provider.GetRequiredService<ICachedFetcher>());
        Assert.IsType<OperationExecutor>(provider.GetRequiredService<IOperationExecutor>());
        Assert.IsType<ServiceViewModelScopeFactory>(provider.GetRequiredService<IViewModelScopeFactory>());
        Assert.IsType<MemoryFeatureSwitch>(provider.GetRequiredService<IFeatureSwitch>());
        Assert.Same(AllowAllPermissionGate.Instance, provider.GetRequiredService<IPermissionGate>());
        Assert.IsType<MemoryFileStore>(provider.GetRequiredService<IFileStore>());
        Assert.Same(NullMediaPicker.Instance, provider.GetRequiredService<IMediaPicker>());
    }
}
