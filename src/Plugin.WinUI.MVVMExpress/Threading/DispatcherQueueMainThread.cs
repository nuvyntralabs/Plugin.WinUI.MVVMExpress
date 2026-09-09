using Microsoft.UI.Dispatching;
using Plugin.WinUI.MVVMExpress.Threading;

namespace Plugin.WinUI.MVVMExpress.Threading;

/// <summary>WinUI <see cref="DispatcherQueue"/> adapter for <see cref="IMainThread"/>.</summary>
public sealed class DispatcherQueueMainThread : IMainThread
{
    private readonly DispatcherQueue _queue;

    public DispatcherQueueMainThread()
        : this(DispatcherQueue.GetForCurrentThread() ?? DispatcherQueue.GetForCurrentThread())
    {
    }

    public DispatcherQueueMainThread(DispatcherQueue queue)
    {
        ArgumentNullException.ThrowIfNull(queue);
        _queue = queue;
    }

    public bool IsMainThread => _queue.HasThreadAccess;

    public void BeginInvoke(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (_queue.HasThreadAccess)
        {
            action();
            return;
        }

        _ = _queue.TryEnqueue(() => action());
    }

    public Task InvokeAsync(Action action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();
        if (_queue.HasThreadAccess)
        {
            action();
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource();
        if (!_queue.TryEnqueue(() =>
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    action();
                    tcs.TrySetResult();
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }))
        {
            tcs.TrySetException(new InvalidOperationException("DispatcherQueue rejected the work item."));
        }

        return tcs.Task;
    }

    public async Task InvokeAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();
        if (_queue.HasThreadAccess)
        {
            await action().ConfigureAwait(true);
            return;
        }

        var tcs = new TaskCompletionSource();
        if (!_queue.TryEnqueue(async () =>
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await action().ConfigureAwait(true);
                    tcs.TrySetResult();
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }))
        {
            tcs.TrySetException(new InvalidOperationException("DispatcherQueue rejected the work item."));
        }

        await tcs.Task.ConfigureAwait(false);
    }
}
