using Plugin.WinUI.MVVMExpress.ComponentModel;
using Result = Plugin.WinUI.MVVMExpress.Outcome.Outcome;

namespace Plugin.WinUI.MVVMExpress.Navigation;

/// <summary>ViewModel navigation. Implementations must not require a <c>Page</c> reference on the ViewModel.</summary>
public interface INavigator
{
    /// <summary>Last navigated ViewModel type, if any.</summary>
    Type? Current { get; }

    /// <summary>Non-modal stack (root first).</summary>
    IReadOnlyList<Type> Stack { get; }

    /// <summary>Modal stack (first modal first).</summary>
    IReadOnlyList<Type> ModalStack { get; }

    /// <summary>True when <see cref="GoBackAsync"/> would pop a frame.</summary>
    bool CanGoBack { get; }

    /// <summary>Recorded navigation requests (including back).</summary>
    IReadOnlyList<NavigationRequest> History { get; }

    /// <summary>Navigates to <typeparamref name="TViewModel"/>.</summary>
    Task<Result> NavigateToAsync<TViewModel>(CancellationToken cancellationToken = default)
        where TViewModel : class, IViewModel;

    /// <summary>Navigates to <typeparamref name="TViewModel"/> with typed <paramref name="args"/>.</summary>
    Task<Result> NavigateToAsync<TViewModel, TArgs>(TArgs args, CancellationToken cancellationToken = default)
        where TViewModel : class, IViewModel
        where TArgs : notnull;

    /// <summary>Navigates by URI route and optional dictionary query.</summary>
    Task<Result> NavigateToAsync(
        string route,
        IReadOnlyDictionary<string, object>? query = null,
        NavOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Pops one level (modal first, then the page stack).</summary>
    Task<Result> GoBackAsync(CancellationToken cancellationToken = default);

    /// <summary>Pops to the first stack frame and clears the modal stack.</summary>
    Task<Result> PopToRootAsync(CancellationToken cancellationToken = default);

    /// <summary>Replaces the current frame with <typeparamref name="TViewModel"/>.</summary>
    Task<Result> ReplaceAsync<TViewModel>(CancellationToken cancellationToken = default)
        where TViewModel : class, IViewModel;

    /// <summary>Clears the stack and makes <typeparamref name="TViewModel"/> the root.</summary>
    Task<Result> ResetAsync<TViewModel>(CancellationToken cancellationToken = default)
        where TViewModel : class, IViewModel;

    /// <summary>Navigates to a ViewModel type resolved at runtime (auth challenge, generated routes).</summary>
    Task<Result> NavigateToAsync(Type viewModelType, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure("E_ROUTE", $"No typed navigation for {viewModelType.Name}."));
}

/// <summary>One recorded navigation.</summary>
/// <param name="ViewModelType">Destination ViewModel type.</param>
/// <param name="Args">Typed args, a query dictionary, or a sentinel such as <c>back</c>.</param>
/// <param name="Route">URI path when the request used a string route.</param>
/// <param name="Query">Dictionary / URI query values.</param>
/// <param name="Modal">Whether the frame was pushed onto the modal stack.</param>
public sealed record NavigationRequest(
    Type ViewModelType,
    object? Args,
    string? Route = null,
    IReadOnlyDictionary<string, object>? Query = null,
    bool Modal = false);

/// <summary>Destination that accepts typed navigation arguments.</summary>
/// <typeparam name="TArgs">Argument type.</typeparam>
public interface IAcceptNavArgs<TArgs>
    where TArgs : notnull
{
    /// <summary>Applies <paramref name="args"/> before initialize/load.</summary>
    void Accept(TArgs args);
}

/// <summary>Destination that accepts dictionary / URI query arguments.</summary>
public interface IAcceptNavQuery
{
    /// <summary>Applies <paramref name="query"/> before initialize/load.</summary>
    void Accept(IReadOnlyDictionary<string, object> query);
}

/// <summary>Navigation lifecycle used by <see cref="PageViewModel"/>.</summary>
public interface INavigable
{
    /// <summary>Called after the host has navigated to this ViewModel.</summary>
    Task OnNavigatedToAsync(CancellationToken cancellationToken = default);

    /// <summary>Called before the host leaves this ViewModel.</summary>
    Task OnNavigatedFromAsync(CancellationToken cancellationToken = default);

    /// <summary>Return <see langword="false"/> to block navigation away (dirty form, etc.).</summary>
    Task<bool> CanNavigateAwayAsync(CancellationToken cancellationToken = default);
}
