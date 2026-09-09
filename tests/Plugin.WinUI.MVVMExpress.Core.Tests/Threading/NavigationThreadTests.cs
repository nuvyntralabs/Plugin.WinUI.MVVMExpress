using Plugin.WinUI.MVVMExpress.Diagnostics;
using Plugin.WinUI.MVVMExpress.Threading;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Threading;

public sealed class NavigationThreadTests
{
    [Fact]
    public void EnsurePageFactoryOnMainThread_Throws_WhenOffThread()
    {
        var off = new OffThread();
        var ex = Assert.Throws<InvalidOperationException>(() => NavigationThread.EnsurePageFactoryOnMainThread(off));
        Assert.Contains("main thread", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EnsurePageFactoryOnMainThread_AllowsImmediate()
        => NavigationThread.EnsurePageFactoryOnMainThread(ImmediateMainThread.Instance);

    [Fact]
    public void TraceOffThread_Writes_WhenEnabled()
    {
        var lines = new List<string>();
        var diagnostics = new CallbackDiagnostics((area, message) => lines.Add($"{area}:{message}"));
        NavigationThread.TraceOffThread(new OffThread(), diagnostics, "Hopping before page construction.");
        Assert.Contains(lines, item => item.StartsWith("nav:", StringComparison.Ordinal));
    }

    [Fact]
    public void Resolve_PrefersInjected()
    {
        var injected = new OffThread();
        Assert.Same(injected, NavigationThread.Resolve(injected));
        Assert.Same(ImmediateMainThread.Instance, NavigationThread.Resolve(null));
    }

    private sealed class OffThread : IMainThread
    {
        public bool IsMainThread => false;

        public void BeginInvoke(Action action) => action();

        public Task InvokeAsync(Action action, CancellationToken cancellationToken = default)
        {
            action();
            return Task.CompletedTask;
        }

        public Task InvokeAsync(Func<Task> action, CancellationToken cancellationToken = default) => action();
    }
}
