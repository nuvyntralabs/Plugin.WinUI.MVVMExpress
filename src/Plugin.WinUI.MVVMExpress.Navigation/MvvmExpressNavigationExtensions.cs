using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Plugin.WinUI.MVVMExpress.Diagnostics;
using Plugin.WinUI.MVVMExpress.Generated;
using Plugin.WinUI.MVVMExpress.Hosting;
using Plugin.WinUI.MVVMExpress.Threading;

namespace Plugin.WinUI.MVVMExpress.Navigation;

public static class MvvmExpressNavigationExtensions
{
    public static MvvmExpressOptions UseFrameNavigation(
        this MvvmExpressOptions options,
        Action<WinUIFrameNavigator, IServiceProvider>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.AddRegistration(services =>
        {
            services.RemoveAll<INavigator>();
            services.RemoveAll<IPageNavigator>();
            services.AddSingleton<WinUIFrameNavigator>(sp =>
            {
                var window = sp.GetService<IWindowContext>();
                var navigator = new WinUIFrameNavigator(
                    window,
                    sp,
                    () => WinUIVisualTree.CurrentFrame(window),
                    sp.GetService<IMainThread>(),
                    sp.GetService<IMvvmExpressDiagnostics>(),
                    sp.GetService<MvvmExpressOptions>());
                GeneratedRegistrationHooks.ApplyPageMaps((vm, view, route) => navigator.Map(vm, view, route));
                configure?.Invoke(navigator, sp);
                return navigator;
            });
            services.AddSingleton<IPageNavigator>(sp => sp.GetRequiredService<WinUIFrameNavigator>());
            services.AddSingleton<INavigator>(sp => sp.GetRequiredService<WinUIFrameNavigator>());
        });
    }
}
