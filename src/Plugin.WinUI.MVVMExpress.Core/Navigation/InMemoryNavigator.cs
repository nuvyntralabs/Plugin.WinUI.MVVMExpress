using Plugin.WinUI.MVVMExpress.ComponentModel;
using Result = Plugin.WinUI.MVVMExpress.Outcome.Outcome;

namespace Plugin.WinUI.MVVMExpress.Navigation;

/// <summary>
/// Records a URI / page stack for tests and samples that do not attach a MAUI host.
/// </summary>
public class InMemoryNavigator : IPageNavigator, IRouteResolver
{
    private readonly NavigationStack _stack = new();
    private readonly NavigationRouteTable _routes = new();
    private readonly Func<Type, bool>? _canLeave;

    /// <summary>Creates a recorder. Optional <paramref name="canLeave"/> is a dirty-page guard.</summary>
    /// <param name="canLeave">When the current type is set, return false to block.</param>
    /// <param name="window">Window this stack belongs to.</param>
    public InMemoryNavigator(Func<Type, bool>? canLeave = null, IWindowContext? window = null)
    {
        _canLeave = canLeave;
        Window = window ?? WindowContext.Default;
    }

    /// <inheritdoc />
    public IWindowContext Window { get; }

    /// <inheritdoc />
    public Type? Current
    {
        get => _stack.Current;
        set => _stack.Current = value;
    }

    /// <inheritdoc />
    public IReadOnlyList<Type> Stack => _stack.Stack;

    /// <inheritdoc />
    public IReadOnlyList<Type> ModalStack => _stack.ModalStack;

    /// <inheritdoc />
    public bool CanGoBack => _stack.CanGoBack;

    /// <inheritdoc />
    public IReadOnlyList<NavigationRequest> History => _stack.History;

    /// <summary>Maps <typeparamref name="TViewModel"/> to a URI route.</summary>
    public InMemoryNavigator Map<TViewModel>(string route)
        where TViewModel : class, IViewModel
    {
        _routes.Map<TViewModel>(route);
        return this;
    }

    /// <summary>Maps <paramref name="viewModelType"/> to a URI route (generated registrations use this).</summary>
    public InMemoryNavigator Map(Type viewModelType, string route)
    {
        _routes.Map(viewModelType, route);
        return this;
    }

    /// <summary>Resolves <paramref name="route"/> to a ViewModel type.</summary>
    public bool TryResolve(string route, out Type viewModelType) => _routes.TryResolve(route, out viewModelType);

    /// <inheritdoc />
    public Task<Result> NavigateToAsync<TViewModel>(CancellationToken cancellationToken = default)
        where TViewModel : class, IViewModel
        => NavigateCore(typeof(TViewModel), null, null, null, null, cancellationToken);

    /// <inheritdoc />
    public Task<Result> NavigateToAsync<TViewModel, TArgs>(TArgs args, CancellationToken cancellationToken = default)
        where TViewModel : class, IViewModel
        where TArgs : notnull
        => NavigateCore(typeof(TViewModel), args, null, null, null, cancellationToken);

    /// <inheritdoc />
    public Task<Result> NavigateToAsync(
        string route,
        IReadOnlyDictionary<string, object>? query = null,
        NavOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        var split = NavigationRouteTable.Split(route);
        var merged = NavigationRouteTable.MergeQuery(split.Query, query);
        if (!_routes.TryResolve(split.Path, out var viewModelType))
        {
            return Task.FromResult(Result.Failure("E_ROUTE", $"No route mapped for '{split.Path}'."));
        }

        _routes.TryGetRoute(viewModelType, out var mapped);
        return NavigateCore(viewModelType, merged, mapped ?? split.Path, merged, options, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result> NavigateToAsync(Type viewModelType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);
        return NavigateCore(viewModelType, null, null, null, null, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result> GoBackAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!CanLeave())
        {
            return Task.FromResult(Result.Failure("E_GUARD", "Navigation blocked"));
        }

        _stack.Pop();
        return Task.FromResult(Result.Success());
    }

    /// <inheritdoc />
    public Task<Result> PopToRootAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!CanLeave())
        {
            return Task.FromResult(Result.Failure("E_GUARD", "Navigation blocked"));
        }

        _stack.PopToRoot();
        return Task.FromResult(Result.Success());
    }

    /// <inheritdoc />
    public Task<Result> ReplaceAsync<TViewModel>(CancellationToken cancellationToken = default)
        where TViewModel : class, IViewModel
        => NavigateCore(typeof(TViewModel), null, null, null, new NavOptions { Replace = true }, cancellationToken);

    /// <inheritdoc />
    public Task<Result> ResetAsync<TViewModel>(CancellationToken cancellationToken = default)
        where TViewModel : class, IViewModel
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!CanLeave())
        {
            return Task.FromResult(Result.Failure("E_GUARD", "Navigation blocked"));
        }

        _routes.TryGetRoute(typeof(TViewModel), out var route);
        _stack.Reset(new NavigationRequest(typeof(TViewModel), null, route));
        return Task.FromResult(Result.Success());
    }

    /// <inheritdoc />
    public Task<Result> ReplaceRootAsync<TViewModel>(CancellationToken cancellationToken = default)
        where TViewModel : class, IViewModel
        => ResetAsync<TViewModel>(cancellationToken);

    /// <inheritdoc />
    public Task<Result> PushModalAsync<TViewModel>(CancellationToken cancellationToken = default)
        where TViewModel : class, IViewModel
        => NavigateCore(typeof(TViewModel), null, null, null, new NavOptions { Modal = true }, cancellationToken);

    /// <inheritdoc />
    public Task<Result> PopModalAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (ModalStack.Count == 0)
        {
            return Task.FromResult(Result.Failure("E_MODAL", "Modal stack is empty."));
        }

        return GoBackAsync(cancellationToken);
    }

    private Task<Result> NavigateCore(
        Type viewModelType,
        object? args,
        string? route,
        IReadOnlyDictionary<string, object>? query,
        NavOptions? options,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!CanLeave())
        {
            return Task.FromResult(Result.Failure("E_GUARD", "Navigation blocked"));
        }

        var frame = new NavigationRequest(viewModelType, args, route, query, options?.Modal == true);
        if (options?.Replace == true)
        {
            _stack.Replace(frame);
        }
        else
        {
            _stack.Push(frame, options);
        }

        return Task.FromResult(Result.Success());
    }

    private bool CanLeave() => Current is null || _canLeave?.Invoke(Current) != false;
}
