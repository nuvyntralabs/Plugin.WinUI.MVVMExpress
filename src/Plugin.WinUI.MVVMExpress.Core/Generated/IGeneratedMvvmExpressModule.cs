using Microsoft.Extensions.DependencyInjection;
using Plugin.WinUI.MVVMExpress.Auth;

namespace Plugin.WinUI.MVVMExpress.Generated;

/// <summary>AOT-safe generated ViewModel / route / auth registrations.</summary>
public interface IGeneratedMvvmExpressModule
{
    /// <summary>Registers generated ViewModels and views.</summary>
    void AddViewModels(IServiceCollection services);

    /// <summary>Applies <c>[Route]</c> mappings.</summary>
    void ApplyRoutes(Action<Type, string> map);

    /// <summary>Applies <c>[RegisterView]</c> page + ViewModel maps. Default is a no-op so 1.0 modules stay valid.</summary>
    void ApplyPageMaps(Action<Type, Type, string?> map)
    {
    }

    /// <summary>Generated <c>[RequiresAuth]</c> / <c>[RequiresRole]</c> policy.</summary>
    INavigationAuthPolicy AuthPolicy { get; }
}

/// <summary>Collects <see cref="IGeneratedMvvmExpressModule"/> instances emitted by the source generator.</summary>
public static class GeneratedRegistrationHooks
{
    private static readonly List<IGeneratedMvvmExpressModule> Modules = [];

    /// <summary>Adds a generated module. Called from a <c>ModuleInitializer</c>.</summary>
    public static void Add(IGeneratedMvvmExpressModule module)
    {
        ArgumentNullException.ThrowIfNull(module);
        lock (Modules)
        {
            Modules.Add(module);
        }
    }

    /// <summary>Registered modules (copy).</summary>
    public static IReadOnlyList<IGeneratedMvvmExpressModule> Snapshot()
    {
        lock (Modules)
        {
            return [.. Modules];
        }
    }

    /// <summary>Applies every registered module to <paramref name="services"/>.</summary>
    public static void Apply(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        foreach (var module in Snapshot())
        {
            module.AddViewModels(services);
        }
    }

    /// <summary>Applies every generated <c>[Route]</c> mapping.</summary>
    public static void ApplyRoutes(Action<Type, string> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        foreach (var module in Snapshot())
        {
            module.ApplyRoutes(map);
        }
    }

    /// <summary>Applies every generated <c>[RegisterView]</c> page map.</summary>
    public static void ApplyPageMaps(Action<Type, Type, string?> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        foreach (var module in Snapshot())
        {
            module.ApplyPageMaps(map);
        }
    }

    /// <summary>Clears generated modules. Tests only.</summary>
    internal static void ClearForTests()
    {
        lock (Modules)
        {
            Modules.Clear();
        }
    }
}
