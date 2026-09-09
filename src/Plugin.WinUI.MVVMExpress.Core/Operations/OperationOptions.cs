using Plugin.WinUI.MVVMExpress.Input;

namespace Plugin.WinUI.MVVMExpress.Operations;

/// <summary>Shared busy / cancel / timeout / retry / concurrency options for <see cref="IOperationExecutor"/>.</summary>
public sealed class OperationOptions
{
    /// <summary>Cancels the operation if it runs longer than this.</summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>Extra attempts after a failure (not after cancel).</summary>
    public int RetryCount { get; init; }

    /// <summary>Wait between retries.</summary>
    public TimeSpan RetryDelay { get; init; } = TimeSpan.FromMilliseconds(50);

    /// <summary>Wait after the last call before running.</summary>
    public TimeSpan? Debounce { get; init; }

    /// <summary>Ignore calls that arrive within this window after a start.</summary>
    public TimeSpan? Throttle { get; init; }

    /// <summary>How overlapping runs are treated.</summary>
    public ConcurrencyMode Concurrency { get; init; } = ConcurrencyMode.Prevent;
}
