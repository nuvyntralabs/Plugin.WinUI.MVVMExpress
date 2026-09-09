using Plugin.WinUI.MVVMExpress.ComponentModel;
using Result = Plugin.WinUI.MVVMExpress.Outcome.Outcome;

namespace Plugin.WinUI.MVVMExpress.Navigation;

/// <summary>
/// Page / <c>INavigation</c> host. Distinct from Shell so both can be registered in the same app.
/// </summary>
public interface IPageNavigator : INavigator
{
    /// <summary>Window this stack belongs to.</summary>
    IWindowContext Window { get; }

    /// <summary>
    /// Replaces the window root with <typeparamref name="TViewModel"/> (login → host).
    /// Same contract as <see cref="INavigator.ResetAsync{TViewModel}"/>.
    /// </summary>
    Task<Result> ReplaceRootAsync<TViewModel>(CancellationToken cancellationToken = default)
        where TViewModel : class, IViewModel
        => ResetAsync<TViewModel>(cancellationToken);

    /// <summary>Pushes <typeparamref name="TViewModel"/> onto the modal stack.</summary>
    Task<Result> PushModalAsync<TViewModel>(CancellationToken cancellationToken = default)
        where TViewModel : class, IViewModel
        => Task.FromResult(Result.Failure("E_MODAL", $"{GetType().Name} does not implement PushModalAsync."));

    /// <summary>Pops the current modal. Default implementation uses <see cref="INavigator.GoBackAsync"/>.</summary>
    Task<Result> PopModalAsync(CancellationToken cancellationToken = default)
        => ModalStack.Count == 0
            ? Task.FromResult(Result.Failure("E_MODAL", "Modal stack is empty."))
            : GoBackAsync(cancellationToken);
}
