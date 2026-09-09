using Plugin.WinUI.MVVMExpress.Busy;
using Plugin.WinUI.MVVMExpress.Errors;
using Plugin.WinUI.MVVMExpress.Input;
using Plugin.WinUI.MVVMExpress.Outcome;

namespace Plugin.WinUI.MVVMExpress.Operations;

/// <summary>Default <see cref="IOperationExecutor"/> with debounce, throttle, queue, timeout, and retry.</summary>
public sealed class OperationExecutor : IOperationExecutor
{
    private readonly IBusyGate? _busy;
    private readonly IErrorSink _errors;
    private readonly SemaphoreSlim _queue = new(1, 1);
    private readonly object _gate = new();
    private CancellationTokenSource? _debounce;
    private TaskCompletionSource? _debounceStarted;
    private int _debounceGeneration;
    private DateTimeOffset _lastStart;
    private int _running;

    /// <summary>Creates an executor.</summary>
    /// <param name="busy">Optional nested busy gate.</param>
    /// <param name="errors">Unexpected-error sink.</param>
    public OperationExecutor(IBusyGate? busy = null, IErrorSink? errors = null)
    {
        _busy = busy;
        _errors = errors ?? NullErrorSink.Instance;
    }

    /// <inheritdoc />
    public async Task<Outcome.Outcome> RunAsync(
        Func<CancellationToken, Task> operation,
        OperationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        var result = await RunAsync<object?>(
            async ct =>
            {
                await operation(ct).ConfigureAwait(false);
                return null;
            },
            options,
            cancellationToken).ConfigureAwait(false);
        return result.IsSuccess
            ? Outcome.Outcome.Success()
            : Outcome.Outcome.Failure(result.Error!);
    }

    /// <inheritdoc />
    public async Task<Outcome<T>> RunAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        OperationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        options ??= new OperationOptions();

        if (!await WaitPolicyAsync(options, cancellationToken).ConfigureAwait(false))
        {
            return Outcome<T>.Failure("E_THROTTLE", "Throttled");
        }

        var queued = options.Concurrency == ConcurrencyMode.Queue;
        if (queued)
        {
            await _queue.WaitAsync(cancellationToken).ConfigureAwait(false);
        }

        if (options.Concurrency == ConcurrencyMode.Prevent && Volatile.Read(ref _running) > 0)
        {
            if (queued)
            {
                _queue.Release();
            }

            return Outcome<T>.Failure("E_BUSY", "Already running");
        }

        Interlocked.Increment(ref _running);
        using var scope = _busy?.Enter();
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (options.Timeout is { } timeout)
        {
            linked.CancelAfter(timeout);
        }

        try
        {
            var attempts = 0;
            while (true)
            {
                try
                {
                    var value = await operation(linked.Token).ConfigureAwait(false);
                    return Outcome<T>.Success(value);
                }
                catch (OperationCanceledException)
                {
                    return Outcome<T>.Failure("E_CANCEL", "Cancelled");
                }
                catch (Exception ex) when (attempts < options.RetryCount)
                {
                    attempts++;
                    if (options.RetryDelay > TimeSpan.Zero)
                    {
                        await Task.Delay(options.RetryDelay, linked.Token).ConfigureAwait(false);
                    }
                    else
                    {
                        _ = ex;
                    }
                }
                catch (Exception ex)
                {
                    var error = new ErrorInfo("E_OP", ex.Message, ex);
                    await _errors.HandleAsync(error, linked.Token).ConfigureAwait(false);
                    return Outcome<T>.Failure(error);
                }
            }
        }
        finally
        {
            Interlocked.Decrement(ref _running);
            if (queued)
            {
                _queue.Release();
            }
        }
    }

    private async Task<bool> WaitPolicyAsync(OperationOptions options, CancellationToken cancellationToken)
    {
        if (options.Throttle is { } throttle && throttle > TimeSpan.Zero)
        {
            lock (_gate)
            {
                var now = DateTimeOffset.UtcNow;
                if (now - _lastStart < throttle)
                {
                    return false;
                }

                _lastStart = now;
            }
        }

        if (options.Debounce is not { } debounce || debounce <= TimeSpan.Zero)
        {
            return true;
        }

        CancellationTokenSource debounceCts;
        int generation;
        TaskCompletionSource entered;
        lock (_gate)
        {
            _debounce?.Cancel();
            _debounce?.Dispose();
            generation = ++_debounceGeneration;
            debounceCts = new CancellationTokenSource();
            _debounce = debounceCts;
            _debounceStarted ??= new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            entered = _debounceStarted;
        }

        entered.TrySetResult();

        try
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, debounceCts.Token);
            await Task.Delay(debounce, linked.Token).ConfigureAwait(false);
            lock (_gate)
            {
                return generation == _debounceGeneration;
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }

    /// <summary>Completes when a debounce wait has started. Tests only.</summary>
    internal Task WaitForDebounceAsync()
    {
        lock (_gate)
        {
            _debounceStarted ??= new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            return _debounceStarted.Task;
        }
    }
}