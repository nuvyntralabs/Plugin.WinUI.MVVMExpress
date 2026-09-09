using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
#if DEBUG
using Plugin.WinUI.MVVMExpress.Diagnostics;
#endif
using Plugin.WinUI.MVVMExpress.Generated;
using Plugin.WinUI.MVVMExpress.Navigation;
using Plugin.WinUI.MVVMExpress.Threading;

namespace Plugin.WinUI.MVVMExpress.Hosting;

/// <summary>WinUI 3 host entry point.</summary>
public static class WinUIMvvmExpressServiceCollectionExtensions
{
    public static IServiceCollection AddWinUIMvvmExpress(
        this IServiceCollection services,
        Action<MvvmExpressOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        var options = new MvvmExpressOptions();
        configure?.Invoke(options);
        return UseWinUIMvvmExpress(services, options);
    }

    public static IHostApplicationBuilder UseWinUIMvvmExpress(
        this IHostApplicationBuilder builder,
        Action<MvvmExpressOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.AddWinUIMvvmExpress(configure);
        return builder;
    }

    public static IServiceCollection UseWinUIMvvmExpress(
        this IServiceCollection services,
        Action<MvvmExpressOptions>? configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        var options = new MvvmExpressOptions();
        configure?.Invoke(options);
        return UseWinUIMvvmExpress(services, options);
    }

    private static IServiceCollection UseWinUIMvvmExpress(IServiceCollection services, MvvmExpressOptions options)
    {
        services.AddSingleton(options);
        services.AddMvvmExpress();
        var main = new DispatcherQueueMainThread();
        services.RemoveAll<IMainThread>();
        services.AddSingleton<IMainThread>(main);
        NotificationMarshaller.Current = main;
        NotificationMarshaller.MarshalNotifications = options.MarshalNotifications;
        services.RemoveAll<IWindowContext>();
        services.AddSingleton<IWindowContext>(_ => WinUIWindowContext.Current);
#if DEBUG
        if (options.EnableDiagnostics)
        {
            var diagnostics = new CallbackDiagnostics(static (area, message) =>
                System.Diagnostics.Debug.WriteLine($"[MVVMExpress:{area}] {message}"));
            services.RemoveAll<IMvvmExpressDiagnostics>();
            services.AddSingleton<IMvvmExpressDiagnostics>(_ => diagnostics);
            NotificationMarshaller.Diagnostics = diagnostics;
        }
#endif
        options.ApplyRegistrations(services);
        if (options.ApplyGeneratedRegistrations)
        {
            GeneratedRegistrationHooks.Apply(services);
        }

        if (options.AuthChallengeViewModel is { } challenge)
        {
            services.AddAuth(challenge, options.ForwardNavigationFailures);
        }

        return services;
    }
}
