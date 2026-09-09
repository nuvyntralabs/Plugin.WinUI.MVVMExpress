using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Composition;

namespace Plugin.WinUI.MVVMExpress.Testing;

/// <summary>
/// In-memory page host: each push creates an <see cref="IViewModelScope"/>, and pop disposes the ViewModel then the scope.
/// Use this to prove popped pages are collectable without a MAUI window.
/// </summary>
public sealed class ScopedNavigator : IDisposable
{
    private readonly IViewModelScopeFactory _factory;
    private readonly Stack<(IViewModelScope Scope, IViewModel ViewModel)> _stack = new();
    private bool _disposed;

    /// <summary>Creates a host over <paramref name="factory"/>.</summary>
    /// <param name="factory">Page-scope factory from <c>AddMvvmExpress</c>.</param>
    public ScopedNavigator(IViewModelScopeFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory;
    }

    /// <summary>ViewModel at the top of the stack.</summary>
    public IViewModel? Current => _stack.Count == 0 ? null : _stack.Peek().ViewModel;

    /// <summary>How many page scopes are alive.</summary>
    public int Count => _stack.Count;

    /// <summary>Gets a value indicating whether <see cref="Pop"/> has a page to remove.</summary>
    public bool CanGoBack => _stack.Count > 0;

    /// <summary>Resolves <typeparamref name="TViewModel"/> in a new page scope and pushes it.</summary>
    /// <typeparam name="TViewModel">ViewModel type registered in DI.</typeparam>
    /// <param name="configure">Optional accept / seed callback before the caller initializes.</param>
    public TViewModel Push<TViewModel>(Action<TViewModel>? configure = null)
        where TViewModel : class, IViewModel
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var scope = _factory.CreatePageScope();
        var viewModel = scope.GetViewModel<TViewModel>();
        configure?.Invoke(viewModel);
        _stack.Push((scope, viewModel));
        return viewModel;
    }

    /// <summary>Disposes the current ViewModel and its page scope.</summary>
    public void Pop()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_stack.Count == 0)
        {
            throw new InvalidOperationException("Navigation stack is empty.");
        }

        var (scope, viewModel) = _stack.Pop();
        viewModel.Dispose();
        scope.Dispose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        while (_stack.Count > 0)
        {
            var (scope, viewModel) = _stack.Pop();
            viewModel.Dispose();
            scope.Dispose();
        }

        _disposed = true;
    }
}
