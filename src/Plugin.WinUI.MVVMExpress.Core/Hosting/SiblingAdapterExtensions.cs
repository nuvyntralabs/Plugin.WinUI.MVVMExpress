using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Plugin.WinUI.MVVMExpress.Auth;
using Plugin.WinUI.MVVMExpress.Navigation;

namespace Plugin.WinUI.MVVMExpress.Hosting;

/// <summary>
/// Optional sibling-plugin adapters. Core never takes a PackageReference to DeepLinks or SecureSession.
/// A missing adapter throws — it does not no-op.
/// </summary>
public static class SiblingAdapterExtensions
{
    /// <summary>
    /// Registers <paramref name="bridge"/>. Call the parameterless overload to fail closed when
    /// no IDeepLinkBridge is wired.
    /// </summary>
    public static IServiceCollection AddDeepLinks(this IServiceCollection services, IDeepLinkBridge bridge)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(bridge);
        services.TryAddSingleton(bridge);
        return services;
    }

    /// <summary>Fail-closed DeepLinks registration when no adapter is supplied.</summary>
    public static IServiceCollection AddDeepLinks(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        throw new InvalidOperationException(
            "UseDeepLinks requires an IDeepLinkBridge. Register it with AddDeepLinks(bridge).");
    }

    /// <summary>Registers a production <see cref="IAuthState"/> factory (SecureSession or SecureStorage adapter).</summary>
    public static IServiceCollection AddSecureSessionAuth(
        this IServiceCollection services,
        Func<IServiceProvider, IAuthState> factory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(factory);
        services.RemoveAll<IAuthState>();
        services.AddSingleton(factory);
        return services;
    }

    /// <summary>Fail-closed SecureSession registration when no adapter is supplied.</summary>
    public static IServiceCollection AddSecureSessionAuth(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        throw new InvalidOperationException(
            "UseSecureSessionAuth requires IAuthState. Register it with AddSecureSessionAuth(factory).");
    }
}
