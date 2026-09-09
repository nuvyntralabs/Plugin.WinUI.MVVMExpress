namespace Plugin.WinUI.MVVMExpress.Threading;

/// <summary>
/// UI-thread marshal. This is the only dispatcher ViewModels and hosts should use.
/// Do not call MAUI <c>MainThread</c> statics from ViewModels — after <c>ConfigureAwait(false)</c>
/// navigation and dialogs must hop here or they construct pages off-thread.
/// </summary>
public interface IMainThread
{
    /// <summary>Gets a value indicating whether the caller is on the main thread.</summary>
    bool IsMainThread { get; }

    /// <summary>Posts <paramref name="action"/> without waiting.</summary>
    /// <param name="action">Work to run.</param>
    void BeginInvoke(Action action);

    /// <summary>Runs <paramref name="action"/> on the main thread and waits.</summary>
    /// <param name="action">Work to run.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    Task InvokeAsync(Action action, CancellationToken cancellationToken = default);

    /// <summary>Runs <paramref name="action"/> on the main thread and waits.</summary>
    /// <param name="action">Work to run.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    Task InvokeAsync(Func<Task> action, CancellationToken cancellationToken = default);
}

/// <summary>Runs work inline. Used by unit tests and <c>net10.0</c> hosts without a window.</summary>
public sealed class ImmediateMainThread : IMainThread
{
    /// <summary>Shared instance.</summary>
    public static ImmediateMainThread Instance { get; } = new();

    /// <inheritdoc />
    public bool IsMainThread => true;

    /// <inheritdoc />
    public void BeginInvoke(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        action();
    }

    /// <inheritdoc />
    public Task InvokeAsync(Action action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();
        action();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task InvokeAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();
        return action();
    }
}
