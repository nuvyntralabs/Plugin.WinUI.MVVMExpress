using Plugin.WinUI.MVVMExpress.Dialogs;
using Plugin.WinUI.MVVMExpress.Errors;
using Plugin.WinUI.MVVMExpress.Threading;

namespace Plugin.WinUI.MVVMExpress.Input;

/// <summary>How an async command treats a second execution while one is running.</summary>
public enum ConcurrencyMode
{
    /// <summary>Ignore the second execute.</summary>
    Prevent = 0,

    /// <summary>Cancel the in-flight run, then start the new one.</summary>
    CancelPrevious = 1,

    /// <summary>Wait for the current run, then start the new one.</summary>
    Queue = 2,

    /// <summary>Allow overlapping runs.</summary>
    Allow = 3,

    /// <summary>Same as <see cref="CancelPrevious"/>: drop the previous run and start this one.</summary>
    Replace = 4
}

/// <summary>Optional execution policy for <see cref="AsyncModelCommand"/>.</summary>
public sealed class AsyncCommandOptions
{
    /// <summary>Default: prevent a second run.</summary>
    public ConcurrencyMode Concurrency { get; init; } = ConcurrencyMode.Prevent;

    /// <summary>Cancels the command if it runs longer than this.</summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>Extra attempts after a failure (not after cancel).</summary>
    public int RetryCount { get; init; }

    /// <summary>Wait between retries.</summary>
    public TimeSpan RetryDelay { get; init; } = TimeSpan.FromMilliseconds(50);

    /// <summary>Wait after the last <c>ExecuteAsync</c> before running.</summary>
    public TimeSpan? Debounce { get; init; }

    /// <summary>Ignore executes that arrive within this window after a start.</summary>
    public TimeSpan? Throttle { get; init; }

    /// <summary>
    /// When <see langword="true"/> (default), <c>CanExecuteChanged</c> / <c>IsRunning</c> / <c>State</c>
    /// hop to <see cref="IMainThread"/>. Tests using <see cref="ImmediateMainThread"/> stay inline.
    /// </summary>
    public bool MarshalToMainThread { get; init; } = true;

    /// <summary>Optional dispatcher override for this command.</summary>
    public IMainThread? MainThread { get; init; }

    /// <summary>Receives failures from <c>ICommand.Execute</c> (async void). <c>ExecuteAsync</c> still rethrows.</summary>
    public IErrorSink? ErrorSink { get; init; }

    /// <summary>Optional dialogs for <c>ICommand.Execute</c> failures.</summary>
    public IDialogs? Dialogs { get; init; }
}
