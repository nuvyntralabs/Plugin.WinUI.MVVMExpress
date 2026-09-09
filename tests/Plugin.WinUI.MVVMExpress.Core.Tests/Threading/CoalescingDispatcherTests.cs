using Plugin.WinUI.MVVMExpress.Threading;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Threading;

public sealed class CoalescingDispatcherTests
{
    [Fact]
    public void Post_Coalesces_UntilQueuedWorkRuns()
    {
        var runs = 0;
        Action? pending = null;
        var gate = new CoalescingDispatcher(() => runs++, new QueueingMainThread(action => pending = action));
        gate.Post();
        gate.Post();
        gate.Post();
        Assert.Equal(0, runs);
        Assert.NotNull(pending);
        pending();
        Assert.Equal(1, runs);

        gate.Post();
        pending!();
        Assert.Equal(2, runs);
    }

    [Fact]
    public void Post_RunsInline_OnImmediateMainThread()
    {
        var runs = 0;
        var gate = new CoalescingDispatcher(() => runs++);
        gate.Post();
        Assert.Equal(1, runs);
        gate.Post();
        Assert.Equal(2, runs);
    }

    private sealed class QueueingMainThread : IMainThread
    {
        private readonly Action<Action> _queue;

        public QueueingMainThread(Action<Action> queue) => _queue = queue;

        public bool IsMainThread => true;

        public void BeginInvoke(Action action) => _queue(action);

        public Task InvokeAsync(Action action, CancellationToken cancellationToken = default)
        {
            action();
            return Task.CompletedTask;
        }

        public Task InvokeAsync(Func<Task> action, CancellationToken cancellationToken = default) => action();
    }
}
