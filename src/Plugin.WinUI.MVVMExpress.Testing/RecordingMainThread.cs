using Plugin.WinUI.MVVMExpress.Threading;

namespace Plugin.WinUI.MVVMExpress.Testing;

/// <summary>Records hops. Set <see cref="IsMainThread"/> to <see langword="false"/> to force a marshal.</summary>
public sealed class RecordingMainThread : IMainThread
{
    /// <summary>How many times work was posted or invoked.</summary>
    public int InvokeCount { get; private set; }

    /// <summary>Gets a value indicating whether a hop is currently running the action.</summary>
    public bool IsInvoking { get; private set; }

    /// <inheritdoc />
    public bool IsMainThread { get; set; }

    /// <inheritdoc />
    public void BeginInvoke(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        InvokeCount++;
        IsInvoking = true;
        try
        {
            action();
        }
        finally
        {
            IsInvoking = false;
        }
    }

    /// <inheritdoc />
    public Task InvokeAsync(Action action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();
        BeginInvoke(action);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task InvokeAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();
        InvokeCount++;
        IsInvoking = true;
        try
        {
            return action();
        }
        finally
        {
            IsInvoking = false;
        }
    }
}

/// <summary>Fails if a notification handler runs while the caller is off-thread and not inside a hop.</summary>
public sealed class StrictMainThread : IMainThread
{
    /// <inheritdoc />
    public bool IsMainThread => false;

    /// <summary>How many hops ran.</summary>
    public int InvokeCount { get; private set; }

    /// <summary>True while a hop is executing the action.</summary>
    public bool IsInvoking { get; private set; }

    /// <inheritdoc />
    public void BeginInvoke(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        InvokeCount++;
        IsInvoking = true;
        try
        {
            action();
        }
        finally
        {
            IsInvoking = false;
        }
    }

    /// <inheritdoc />
    public Task InvokeAsync(Action action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();
        BeginInvoke(action);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task InvokeAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();
        InvokeCount++;
        IsInvoking = true;
        try
        {
            return action();
        }
        finally
        {
            IsInvoking = false;
        }
    }

    /// <summary>Throws when <paramref name="handler"/> runs outside <see cref="BeginInvoke"/>.</summary>
    public void EnsureRaisedOnHop(Action handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        if (!IsInvoking)
        {
            throw new InvalidOperationException("Notification raised off the main thread.");
        }

        handler();
    }
}
