using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Plugin.WinUI.MVVMExpress.Busy;
using Plugin.WinUI.MVVMExpress.Caching;
using Plugin.WinUI.MVVMExpress.Composition;
using Plugin.WinUI.MVVMExpress.Connectivity;
using Plugin.WinUI.MVVMExpress.Dialogs;
using Plugin.WinUI.MVVMExpress.Errors;
using Plugin.WinUI.MVVMExpress.Files;
using Plugin.WinUI.MVVMExpress.Flags;
using Plugin.WinUI.MVVMExpress.Media;
using Plugin.WinUI.MVVMExpress.Messaging;
using Plugin.WinUI.MVVMExpress.Navigation;
using Plugin.WinUI.MVVMExpress.Operations;
using Plugin.WinUI.MVVMExpress.Permissions;
using Plugin.WinUI.MVVMExpress.Diagnostics;
using Plugin.WinUI.MVVMExpress.State;
using Plugin.WinUI.MVVMExpress.Threading;

namespace Plugin.WinUI.MVVMExpress.Hosting;

/// <summary>Registers Core services for tests, samples, and <c>UseMvvmExpress</c>.</summary>
public static class MVVMExpressServiceCollectionExtensions
{
    /// <summary>Adds Core singletons used by ViewModels.</summary>
    /// <param name="services">Service collection.</param>
    public static IServiceCollection AddMvvmExpress(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<IMessageHub, MessageHub>();
        services.TryAddSingleton<IBusyGate, BusyGate>();
        services.TryAddSingleton<IErrorSink, NullErrorSink>();
        services.TryAddSingleton<ICache, MemoryCache>();
        services.TryAddSingleton<IConnectivityProbe, InMemoryConnectivityProbe>();
        services.TryAddSingleton<IWindowContext>(_ => WindowContext.Default);
        services.TryAddSingleton<IWindowNavigatorRegistry, WindowNavigatorRegistry>();
        services.TryAddSingleton<InMemoryNavigator>();
        services.TryAddSingleton<INavigator>(sp => sp.GetRequiredService<InMemoryNavigator>());
        services.TryAddSingleton<IPageNavigator>(sp => sp.GetRequiredService<InMemoryNavigator>());
        services.TryAddSingleton<IMainThread>(_ => ImmediateMainThread.Instance);
        services.TryAddSingleton<IDialogs, NullDialogs>();
        services.TryAddSingleton<INotifier>(_ => NullDialogs.Instance);
        services.TryAddSingleton<ICachedFetcher>(sp =>
            new CachedFetcher(sp.GetRequiredService<ICache>(), sp.GetService<IConnectivityProbe>()));
        services.TryAddSingleton<IOperationExecutor>(sp =>
            new OperationExecutor(sp.GetService<IBusyGate>(), sp.GetService<IErrorSink>()));
        services.TryAddSingleton<IViewModelScopeFactory>(sp => new ServiceViewModelScopeFactory(sp));
        services.TryAddSingleton<IFeatureSwitch, MemoryFeatureSwitch>();
        services.TryAddSingleton<IPermissionGate>(_ => AllowAllPermissionGate.Instance);
        services.TryAddSingleton<IFileStore, MemoryFileStore>();
        services.TryAddSingleton<IMediaPicker>(_ => NullMediaPicker.Instance);
        services.TryAddSingleton<IStateStore, MemoryStateStore>();
        services.TryAddSingleton<IMvvmExpressDiagnostics>(_ => NullDiagnostics.Instance);
        return services;
    }
}
