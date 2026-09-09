using Plugin.WinUI.MVVMExpress.Threading;

namespace Plugin.WinUI.MVVMExpress.Testing;

/// <summary>Records <see cref="IMainThread"/> calls and runs work inline.</summary>
public sealed class FakeMainThread : IMainThread
{
    /// <summary>How many times work was posted or invoked.</summary>
    public int InvokeCount { get; private set; }

    /// <inheritdoc />
    public bool IsMainThread => true;

    /// <inheritdoc />
    public void BeginInvoke(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        InvokeCount++;
        action();
    }

    /// <inheritdoc />
    public Task InvokeAsync(Action action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();
        InvokeCount++;
        action();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task InvokeAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();
        InvokeCount++;
        return action();
    }
}
