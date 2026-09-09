using Plugin.WinUI.MVVMExpress.Core.Tests.Support;
using Plugin.WinUI.MVVMExpress.Messaging;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Messaging;

public sealed class MessageHubTests
{
    [Fact]
    public void Publish_InvokesAllSubscribers()
    {
        var hub = new MessageHub();
        var a = new ProbeViewModel();
        var b = new ProbeViewModel();
        hub.Subscribe<ProbeViewModel, string>(a, static (r, m) => r.Name = m);
        hub.Subscribe<ProbeViewModel, string>(b, static (r, m) => r.Name = m);
        hub.Publish("hello");
        Assert.Equal("hello", a.Name);
        Assert.Equal("hello", b.Name);
    }

    [Fact]
    public async Task PublishAsync_InvokesSubscriber()
    {
        var hub = new MessageHub();
        var vm = new ProbeViewModel();
        hub.Subscribe<ProbeViewModel, string>(vm, static (r, m) => r.Name = m);
        await hub.PublishAsync("async");
        Assert.Equal("async", vm.Name);
    }

    [Fact]
    public void DisposeSubscription_StopsDelivery()
    {
        var hub = new MessageHub();
        var vm = new ProbeViewModel();
        var sub = hub.Subscribe<ProbeViewModel, string>(vm, static (r, m) => r.Name = m);
        sub.Dispose();
        hub.Publish("later");
        Assert.Null(vm.Name);
    }

    [Fact]
    public void Publish_NoSubscribers_DoesNotThrow()
        => new MessageHub().Publish(42);

    [Fact]
    public async Task PublishAsync_Cancelled_Throws()
    {
        var hub = new MessageHub();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => hub.PublishAsync("x", cts.Token));
    }

    [Fact]
    public void Unsubscribe_StopsAllDeliveryForSubscriber()
    {
        var hub = new MessageHub();
        var vm = new ProbeViewModel();
        hub.Subscribe<ProbeViewModel, string>(vm, static (r, m) => r.Name = m);
        hub.Unsubscribe(vm);
        hub.Publish("later");
        Assert.Null(vm.Name);
    }

    [Fact]
    public void Subscribe_NullArguments_Throw()
    {
        var hub = new MessageHub();
        var vm = new ProbeViewModel();
        Assert.Throws<ArgumentNullException>(() =>
            hub.Subscribe<ProbeViewModel, string>(null!, static (_, _) => { }));
        Assert.Throws<ArgumentNullException>(() =>
            hub.Subscribe<ProbeViewModel, string>(vm, null!));
        Assert.Throws<ArgumentNullException>(() => hub.Unsubscribe(null!));
    }

    [Fact]
    public void Publish_AfterDisposedSubscription_DoesNotInvoke()
    {
        var hub = new MessageHub();
        var vm = new ProbeViewModel();
        var sub = hub.Subscribe<ProbeViewModel, string>(vm, static (r, m) => r.Name = m);
        sub.Dispose();
        sub.Dispose();
        hub.Publish("x");
        Assert.Null(vm.Name);
    }
}
