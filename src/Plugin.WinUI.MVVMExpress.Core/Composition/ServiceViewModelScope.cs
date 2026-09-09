using Microsoft.Extensions.DependencyInjection;
using Plugin.WinUI.MVVMExpress.ComponentModel;

namespace Plugin.WinUI.MVVMExpress.Composition;

/// <summary><see cref="IViewModelScope"/> over an <see cref="IServiceScope"/>.</summary>
public sealed class ServiceViewModelScope : IViewModelScope
{
    private readonly IServiceScope _scope;
    private bool _disposed;

    /// <summary>Creates a scope wrapper.</summary>
    public ServiceViewModelScope(IServiceScope scope)
    {
        ArgumentNullException.ThrowIfNull(scope);
        _scope = scope;
    }

    /// <inheritdoc />
    public T GetViewModel<T>()
        where T : class, IViewModel
        => _scope.ServiceProvider.GetRequiredService<T>();

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _scope.Dispose();
        _disposed = true;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}

/// <summary>Creates <see cref="ServiceViewModelScope"/> instances from the root provider.</summary>
public sealed class ServiceViewModelScopeFactory : IViewModelScopeFactory
{
    private readonly IServiceScopeFactory _factory;

    /// <summary>Creates a factory.</summary>
    public ServiceViewModelScopeFactory(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _factory = services.GetRequiredService<IServiceScopeFactory>();
    }

    /// <inheritdoc />
    public IViewModelScope CreatePageScope()
        => new ServiceViewModelScope(_factory.CreateScope());

    /// <inheritdoc />
    public IViewModelScope CreateChildScope(IViewModelScope parent)
    {
        ArgumentNullException.ThrowIfNull(parent);
        return CreatePageScope();
    }
}
