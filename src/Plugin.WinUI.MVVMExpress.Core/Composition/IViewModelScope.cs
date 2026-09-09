using Plugin.WinUI.MVVMExpress.ComponentModel;

namespace Plugin.WinUI.MVVMExpress.Composition;

/// <summary>DI scope for a page or child ViewModel. Dispose pops the scope.</summary>
public interface IViewModelScope : IAsyncDisposable, IDisposable
{
    /// <summary>Resolves a ViewModel from this scope.</summary>
    T GetViewModel<T>()
        where T : class, IViewModel;
}

/// <summary>Creates page and child scopes from <see cref="IServiceProvider"/>.</summary>
public interface IViewModelScopeFactory
{
    /// <summary>Creates a new scope (typically one page).</summary>
    IViewModelScope CreatePageScope();

    /// <summary>Creates a child scope under <paramref name="parent"/>.</summary>
    IViewModelScope CreateChildScope(IViewModelScope parent);
}
