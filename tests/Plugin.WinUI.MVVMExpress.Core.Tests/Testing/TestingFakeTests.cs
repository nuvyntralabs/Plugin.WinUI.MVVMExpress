using Plugin.WinUI.MVVMExpress.Hosting;
using Plugin.WinUI.MVVMExpress.Testing;
using Plugin.WinUI.MVVMExpress.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Testing;

public sealed class TestingFakeTests
{
    [Fact]
    public async Task FakeMainThread_RunsInline_AndCounts()
    {
        var thread = new FakeMainThread();
        Assert.True(thread.IsMainThread);
        var n = 0;
        thread.BeginInvoke(() => n++);
        await thread.InvokeAsync(() => n++);
        await thread.InvokeAsync(() =>
        {
            n++;
            return Task.CompletedTask;
        });
        Assert.Equal(3, n);
        Assert.Equal(3, thread.InvokeCount);
    }

    [Fact]
    public void FakeConnectivity_IsMutable()
    {
        var probe = new FakeConnectivity();
        Assert.True(probe.IsOnline);
        probe.IsOnline = false;
        Assert.False(probe.IsOnline);
    }

    [Fact]
    public void FakeMessageHub_RecordsPublish_AndDelivers()
    {
        var hub = new FakeMessageHub();
        var seen = 0;
        var listener = new object();
        hub.Subscribe<object, string>(listener, (_, _) => seen++);
        hub.Publish("hello");
        Assert.Equal("hello", Assert.Single(hub.Published));
        Assert.Equal(1, seen);
    }

    [Fact]
    public async Task FakeMessageHub_PublishAsync_Records()
    {
        var hub = new FakeMessageHub();
        await hub.PublishAsync(3);
        Assert.Equal(3, Assert.Single(hub.Published));
    }

    [Fact]
    public void FakeMessageHub_Unsubscribe_StopsDelivery()
    {
        var hub = new FakeMessageHub();
        var seen = 0;
        var listener = new object();
        hub.Subscribe<object, int>(listener, (_, _) => seen++);
        hub.Unsubscribe(listener);
        hub.Publish(1);
        Assert.Equal(0, seen);
        Assert.Equal(1, Assert.Single(hub.Published));
    }

    [Fact]
    public void AddMvvmExpress_StillRegistersImmediateMainThread()
    {
        using var provider = new ServiceCollection().AddMvvmExpress().BuildServiceProvider();
        Assert.Same(ImmediateMainThread.Instance, provider.GetRequiredService<IMainThread>());
    }
}
