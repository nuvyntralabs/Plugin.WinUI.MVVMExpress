namespace Plugin.WinUI.MVVMExpress.Threading;

/// <summary>
/// Posts work to <see cref="IMainThread"/> and drops duplicate posts until the queued run finishes.
/// Use this for inbox / hub handlers so a live list is not reset on every message.
/// </summary>
public sealed class CoalescingDispatcher
{
    private readonly IMainThread _main;
    private readonly Action _work;
    private int _queued;

    /// <summary>Creates a coalescing dispatcher.</summary>
    /// <param name="work">Work to run on the UI thread.</param>
    /// <param name="mainThread">Optional dispatcher. Falls back to <see cref="NotificationMarshaller"/>.</param>
    public CoalescingDispatcher(Action work, IMainThread? mainThread = null)
    {
        ArgumentNullException.ThrowIfNull(work);
        _work = work;
        _main = NavigationThread.Resolve(mainThread);
    }

    /// <summary>Queues work once. Further calls before it runs are ignored.</summary>
    public void Post()
    {
        if (Interlocked.Exchange(ref _queued, 1) == 1)
        {
            return;
        }

        _main.BeginInvoke(Run);
    }

    private void Run()
    {
        Interlocked.Exchange(ref _queued, 0);
        _work();
    }
}
