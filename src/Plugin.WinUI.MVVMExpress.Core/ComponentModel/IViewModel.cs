using Plugin.WinUI.MVVMExpress.State;

namespace Plugin.WinUI.MVVMExpress.ComponentModel;

/// <summary>Lifecycle-aware ViewModel surface used by the host and tests.</summary>
public interface IViewModel : IAsyncDisposable, IDisposable
{
    /// <summary>Current UI status.</summary>
    ViewModelStatus Status { get; }

    /// <summary>Gets a value indicating whether the ViewModel is doing work.</summary>
    bool IsBusy { get; }

    /// <summary>Token cancelled when the ViewModel is disposed. Remains readable after dispose.</summary>
    CancellationToken ViewModelCancellationToken { get; }

    /// <summary>Called once after construction.</summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>Called when the page appears.</summary>
    Task OnAppearingAsync(CancellationToken cancellationToken = default);

    /// <summary>Called when the page disappears.</summary>
    Task OnDisappearingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels in-flight work when the host has <c>CancelOperationsOnDisappear</c>.
    /// Default is a no-op; override to cancel commands without disposing the ViewModel.
    /// </summary>
    void CancelPendingOperations()
    {
    }
}
