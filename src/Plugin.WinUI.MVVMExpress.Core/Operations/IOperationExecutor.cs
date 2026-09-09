using Plugin.WinUI.MVVMExpress.Outcome;

namespace Plugin.WinUI.MVVMExpress.Operations;

/// <summary>
/// Shared operation pipeline: busy, cancellation, timeout, retry, debounce / throttle, and <see cref="Outcome"/>.
/// </summary>
public interface IOperationExecutor
{
    /// <summary>Runs <paramref name="operation"/> through the pipeline.</summary>
    Task<Outcome.Outcome> RunAsync(
        Func<CancellationToken, Task> operation,
        OperationOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Runs <paramref name="operation"/> through the pipeline and returns a value.</summary>
    Task<Outcome<T>> RunAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        OperationOptions? options = null,
        CancellationToken cancellationToken = default);
}
