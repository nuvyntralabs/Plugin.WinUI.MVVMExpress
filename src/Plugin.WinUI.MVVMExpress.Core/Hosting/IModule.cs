using Microsoft.Extensions.DependencyInjection;

namespace Plugin.WinUI.MVVMExpress.Hosting;

/// <summary>
/// Feature-team composition boundary. Registers routes, ViewModels, and services for one assembly.
/// Not a Prism region catalog.
/// </summary>
public interface IModule
{
    /// <summary>Adds this module's services and ViewModels to <paramref name="services"/>.</summary>
    void Configure(IServiceCollection services);
}

/// <summary>Registers thin <see cref="IModule"/> implementations.</summary>
public static class ModuleServiceCollectionExtensions
{
    /// <summary>Constructs <typeparamref name="TModule"/> and runs <see cref="IModule.Configure"/>.</summary>
    public static IServiceCollection AddModule<TModule>(this IServiceCollection services)
        where TModule : class, IModule, new()
    {
        ArgumentNullException.ThrowIfNull(services);
        new TModule().Configure(services);
        return services;
    }
}
