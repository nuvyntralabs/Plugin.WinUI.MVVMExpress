using Plugin.WinUI.MVVMExpress.Threading;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Threading;

public sealed class ImmediateMainThreadTests
{
    [Fact]
    public void BeginInvoke_RunsInline()
    {
        var ran = false;
        ImmediateMainThread.Instance.BeginInvoke(() => ran = true);
        Assert.True(ran);
        Assert.True(ImmediateMainThread.Instance.IsMainThread);
    }

    [Fact]
    public async Task InvokeAsync_RunsAction()
    {
        var ran = false;
        await ImmediateMainThread.Instance.InvokeAsync(() => ran = true);
        Assert.True(ran);
    }

    [Fact]
    public async Task InvokeAsync_HonorsCancellation()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => ImmediateMainThread.Instance.InvokeAsync(() => { }, cts.Token));
    }
}
