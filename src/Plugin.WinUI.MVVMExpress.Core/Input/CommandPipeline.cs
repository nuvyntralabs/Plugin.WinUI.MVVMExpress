namespace Plugin.WinUI.MVVMExpress.Input;

/// <summary>Shared debounce / throttle / queue gate used by async commands.</summary>
internal sealed class CommandPipeline
{
    private readonly AsyncCommandOptions _options;
    private readonly SemaphoreSlim _queue = new(1, 1);
    private readonly object _gate = new();
    private CancellationTokenSource? _debounce;
    private int _debounceGeneration;
    private DateTimeOffset _lastStart;

    public CommandPipeline(AsyncCommandOptions options)
    {
        _options = options;
    }

    public bool AllowsExecuteWhileRunning => _options.Concurrency is not ConcurrencyMode.Prevent;

    public bool InterruptsPrevious =>
        _options.Concurrency is ConcurrencyMode.CancelPrevious or ConcurrencyMode.Replace;

    public bool AllowsOverlap => _options.Concurrency == ConcurrencyMode.Allow;

    public bool Queues => _options.Concurrency == ConcurrencyMode.Queue;

    public async Task<bool> WaitPolicyAsync(CancellationToken cancellationToken)
    {
        if (_options.Throttle is { } throttle && throttle > TimeSpan.Zero)
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

        if (_options.Debounce is not { } debounce || debounce <= TimeSpan.Zero)
        {
            return true;
        }

        CancellationTokenSource debounceCts;
        CancellationTokenSource? previous;
        int generation;
        lock (_gate)
        {
            previous = _debounce;
            generation = ++_debounceGeneration;
            debounceCts = new CancellationTokenSource();
            _debounce = debounceCts;
        }

        try
        {
            previous?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // Previous waiter already finished.
        }

        try
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, debounceCts.Token);
            await Task.Delay(debounce, linked.Token).ConfigureAwait(false);
            lock (_gate)
            {
                if (generation != _debounceGeneration)
                {
                    debounceCts.Dispose();
                    return false;
                }

                return true;
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            debounceCts.Dispose();
            return false;
        }
    }

    public Task EnterQueueAsync(CancellationToken cancellationToken)
        => Queues ? _queue.WaitAsync(cancellationToken) : Task.CompletedTask;

    public void ExitQueue()
    {
        if (Queues)
        {
            _queue.Release();
        }
    }
}
