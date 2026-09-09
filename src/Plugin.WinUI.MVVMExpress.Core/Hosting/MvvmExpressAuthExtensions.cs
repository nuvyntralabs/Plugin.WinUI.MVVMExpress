using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Plugin.WinUI.MVVMExpress.Auth;
using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Dialogs;
using Plugin.WinUI.MVVMExpress.Errors;
using Plugin.WinUI.MVVMExpress.Generated;
using Plugin.WinUI.MVVMExpress.Navigation;

namespace Plugin.WinUI.MVVMExpress.Hosting;

/// <summary>Registers <see cref="GuardedNavigator"/> around the current <see cref="INavigator"/>.</summary>
public static class MvvmExpressAuthExtensions
{
    /// <summary>
    /// Wraps the registered <see cref="INavigator"/> with <see cref="GuardedNavigator"/>.
    /// MAUI apps should call <c>UseAuth&lt;TChallenge&gt;()</c> on <c>UseMvvmExpress</c> instead of reconstructing the guard.
    /// </summary>
    /// <typeparam name="TChallenge">Login ViewModel opened when a <c>[RequiresAuth]</c> route is blocked.</typeparam>
    /// <param name="services">Service collection.</param>
    /// <param name="forwardFailures">When <see langword="true"/>, failed outcomes go to <see cref="IErrorSink"/> / <see cref="IDialogs"/>.</param>
    /// <returns>The same collection.</returns>
    public static IServiceCollection AddAuth<TChallenge>(this IServiceCollection services, bool forwardFailures = true)
        where TChallenge : class, IViewModel
        => AddAuth(services, typeof(TChallenge), forwardFailures);

    /// <summary>
    /// Wraps the registered <see cref="INavigator"/> with <see cref="GuardedNavigator"/>.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="challengeViewModel">Login ViewModel type opened when a guarded route is blocked.</param>
    /// <param name="forwardFailures">When <see langword="true"/>, failed outcomes go to <see cref="IErrorSink"/> / <see cref="IDialogs"/>.</param>
    /// <returns>The same collection.</returns>
    public static IServiceCollection AddAuth(this IServiceCollection services, Type challengeViewModel, bool forwardFailures = true)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(challengeViewModel);
        if (!typeof(IViewModel).IsAssignableFrom(challengeViewModel))
        {
            throw new ArgumentException($"Challenge type '{challengeViewModel.Name}' must implement {nameof(IViewModel)}.", nameof(challengeViewModel));
        }

        var navigatorDescriptor = FindLast(services, typeof(INavigator));
        services.RemoveAll<INavigator>();
        services.RemoveAll<IPageNavigator>();
        services.AddSingleton<INavigator>(sp =>
        {
            var inner = ResolveInner(sp, navigatorDescriptor)
                ?? throw new InvalidOperationException(
                    "UseAuth requires an INavigator. Call UseFrameNavigation() first, or AddMvvmExpress().");
            var auth = sp.GetService<IAuthState>()
                ?? throw new InvalidOperationException(
                    "UseAuth requires IAuthState. Register an adapter (samples: InMemoryAuthState).");
            return new GuardedNavigator(
                inner,
                auth,
                ResolvePolicy(sp),
                new GuardedNavigatorOptions
                {
                    ChallengeViewModel = challengeViewModel,
                    Errors = sp.GetService<IErrorSink>(),
                    Dialogs = sp.GetService<IDialogs>(),
                    ForwardFailures = forwardFailures
                });
        });
        services.AddSingleton<IPageNavigator>(sp => (IPageNavigator)sp.GetRequiredService<INavigator>());
        return services;
    }

    internal static INavigationAuthPolicy? ResolvePolicy(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);
        List<INavigationAuthPolicy> policies = [];
        var registered = services.GetService<INavigationAuthPolicy>();
        if (registered is not null)
        {
            policies.Add(registered);
        }

        foreach (var module in GeneratedRegistrationHooks.Snapshot())
        {
            if (module.AuthPolicy is { } generated
                && !ReferenceEquals(generated, registered))
            {
                policies.Add(generated);
            }
        }

        return policies.Count switch
        {
            0 => null,
            1 => policies[0],
            _ => new CompositeNavigationAuthPolicy(policies)
        };
    }

    private static ServiceDescriptor? FindLast(IServiceCollection services, Type serviceType)
    {
        for (var i = services.Count - 1; i >= 0; i--)
        {
            if (services[i].ServiceType == serviceType)
            {
                return services[i];
            }
        }

        return null;
    }

    private static INavigator? ResolveInner(IServiceProvider services, ServiceDescriptor? descriptor)
    {
        if (descriptor is null)
        {
            return services.GetService<InMemoryNavigator>();
        }

        if (descriptor.ImplementationInstance is INavigator instance)
        {
            return instance;
        }

        if (descriptor.ImplementationFactory is { } factory)
        {
            return (INavigator)factory(services);
        }

        if (descriptor.ImplementationType is { } type)
        {
            return (INavigator)ActivatorUtilities.GetServiceOrCreateInstance(services, type);
        }

        return services.GetService<InMemoryNavigator>();
    }
}
