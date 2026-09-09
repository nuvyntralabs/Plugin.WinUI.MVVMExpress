using Plugin.WinUI.MVVMExpress.Auth;
using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Dialogs;
using Plugin.WinUI.MVVMExpress.Errors;
using Result = Plugin.WinUI.MVVMExpress.Outcome.Outcome;

namespace Plugin.WinUI.MVVMExpress.Navigation;

/// <summary>Optional challenge and failure forwarding for <see cref="GuardedNavigator"/>.</summary>
public sealed class GuardedNavigatorOptions
{
    /// <summary>Login ViewModel to open when a guarded route is blocked.</summary>
    public Type? ChallengeViewModel { get; init; }

    /// <summary>Optional sink for failed navigation outcomes.</summary>
    public IErrorSink? Errors { get; init; }

    /// <summary>Optional dialogs for failed navigation outcomes.</summary>
    public IDialogs? Dialogs { get; init; }

    /// <summary>When <see langword="true"/>, failed outcomes are forwarded to <see cref="Errors"/> / <see cref="Dialogs"/>.</summary>
    public bool ForwardFailures { get; init; }
}

/// <summary>Blocks navigation to selected ViewModel types until <see cref="IAuthState.IsAuthenticated"/>.</summary>
public sealed class GuardedNavigator : IPageNavigator
{
    private readonly INavigator _inner;
    private readonly IAuthState _auth;
    private readonly HashSet<Type> _protectedTypes;
    private readonly INavigationAuthPolicy? _policy;
    private readonly GuardedNavigatorOptions _options;
    private Func<Task<Result>>? _pending;

    /// <summary>Creates a guard around <paramref name="inner"/>.</summary>
    public GuardedNavigator(INavigator inner, IAuthState auth, params Type[] protectedTypes)
        : this(inner, auth, policy: null, options: null, protectedTypes)
    {
    }

    /// <summary>Creates a guard that also applies <paramref name="policy"/> (generated <see cref="RequiresAuthAttribute"/> maps).</summary>
    public GuardedNavigator(INavigator inner, IAuthState auth, INavigationAuthPolicy? policy, params Type[] protectedTypes)
        : this(inner, auth, policy, options: null, protectedTypes)
    {
    }

    /// <summary>Creates a guard with challenge / failure-forwarding options.</summary>
    public GuardedNavigator(
        INavigator inner,
        IAuthState auth,
        INavigationAuthPolicy? policy,
        GuardedNavigatorOptions? options,
        params Type[] protectedTypes)
    {
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(auth);
        _inner = inner;
        _auth = auth;
        _policy = policy;
        _options = options ?? new GuardedNavigatorOptions();
        _protectedTypes = [.. protectedTypes];
        _auth.Changed += OnAuthChanged;
    }

    /// <inheritdoc />
    public IWindowContext Window => _inner is IPageNavigator page ? page.Window : WindowContext.Default;

    /// <inheritdoc />
    public Type? Current => _inner.Current;

    /// <inheritdoc />
    public IReadOnlyList<Type> Stack => _inner.Stack;

    /// <inheritdoc />
    public IReadOnlyList<Type> ModalStack => _inner.ModalStack;

    /// <inheritdoc />
    public bool CanGoBack => _inner.CanGoBack;

    /// <inheritdoc />
    public IReadOnlyList<NavigationRequest> History => _inner.History;

    /// <inheritdoc />
    public Task<Result> NavigateToAsync<TViewModel>(CancellationToken cancellationToken = default)
        where TViewModel : class, IViewModel
        => Gate(typeof(TViewModel), () => _inner.NavigateToAsync<TViewModel>(cancellationToken), cancellationToken);

    /// <inheritdoc />
    public Task<Result> NavigateToAsync<TViewModel, TArgs>(TArgs args, CancellationToken cancellationToken = default)
        where TViewModel : class, IViewModel
        where TArgs : notnull
        => Gate(typeof(TViewModel), () => _inner.NavigateToAsync<TViewModel, TArgs>(args, cancellationToken), cancellationToken);

    /// <inheritdoc />
    public Task<Result> NavigateToAsync(
        string route,
        IReadOnlyDictionary<string, object>? query = null,
        NavOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (_inner is IRouteResolver resolver && resolver.TryResolve(route, out var type))
        {
            return Gate(type, () => _inner.NavigateToAsync(route, query, options, cancellationToken), cancellationToken);
        }

        return ForwardAsync(_inner.NavigateToAsync(route, query, options, cancellationToken), cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result> NavigateToAsync(Type viewModelType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);
        return Gate(viewModelType, () => _inner.NavigateToAsync(viewModelType, cancellationToken), cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result> GoBackAsync(CancellationToken cancellationToken = default)
        => ForwardAsync(_inner.GoBackAsync(cancellationToken), cancellationToken);

    /// <inheritdoc />
    public Task<Result> PopToRootAsync(CancellationToken cancellationToken = default)
        => ForwardAsync(_inner.PopToRootAsync(cancellationToken), cancellationToken);

    /// <inheritdoc />
    public Task<Result> ReplaceAsync<TViewModel>(CancellationToken cancellationToken = default)
        where TViewModel : class, IViewModel
        => Gate(typeof(TViewModel), () => _inner.ReplaceAsync<TViewModel>(cancellationToken), cancellationToken);

    /// <inheritdoc />
    public Task<Result> ResetAsync<TViewModel>(CancellationToken cancellationToken = default)
        where TViewModel : class, IViewModel
        => Gate(typeof(TViewModel), () => _inner.ResetAsync<TViewModel>(cancellationToken), cancellationToken);

    private async Task<Result> Gate(Type viewModelType, Func<Task<Result>> next, CancellationToken cancellationToken)
    {
        var requiresAuth = _protectedTypes.Contains(viewModelType)
            || _policy?.RequiresAuthentication(viewModelType) == true;
        if (requiresAuth && !_auth.IsAuthenticated)
        {
            if (_options.ChallengeViewModel is { } challenge && challenge != viewModelType)
            {
                _pending = next;
                return await ForwardAsync(_inner.NavigateToAsync(challenge, cancellationToken), cancellationToken).ConfigureAwait(false);
            }

            return await ForwardAsync(Task.FromResult(Result.Failure("E_AUTH", "Sign in required")), cancellationToken).ConfigureAwait(false);
        }

        if (_policy?.RequiresRole(viewModelType, out var role) == true
            && !string.IsNullOrEmpty(role)
            && (_auth is not IRoleState roles || !roles.HasRole(role)))
        {
            return await ForwardAsync(Task.FromResult(Result.Failure("E_ROLE", $"Role '{role}' required")), cancellationToken).ConfigureAwait(false);
        }

        return await ForwardAsync(next(), cancellationToken).ConfigureAwait(false);
    }

    private async Task<Result> ForwardAsync(Task<Result> navigation, CancellationToken cancellationToken)
    {
        var result = await navigation.ConfigureAwait(false);
        if (_options.ForwardFailures && !result.IsSuccess && result.Error is { } error)
        {
            if (_options.Errors is { } errors)
            {
                await errors.HandleAsync(error, cancellationToken).ConfigureAwait(false);
            }

            if (_options.Dialogs is { } dialogs)
            {
                await dialogs.ErrorAsync(error, cancellationToken).ConfigureAwait(false);
            }
        }

        return result;
    }

    private void OnAuthChanged(object? sender, EventArgs e)
    {
        if (!_auth.IsAuthenticated || _pending is null)
        {
            return;
        }

        var pending = _pending;
        _pending = null;
        _ = pending();
    }
}
