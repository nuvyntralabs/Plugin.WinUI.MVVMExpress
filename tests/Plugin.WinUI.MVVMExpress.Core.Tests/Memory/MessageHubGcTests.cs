using Plugin.WinUI.MVVMExpress.Core.Tests.Support;
using Plugin.WinUI.MVVMExpress.Messaging;
using Plugin.WinUI.MVVMExpress.Testing;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Memory;

public sealed class MessageHubGcTests
{
    [Fact]
    public void WeakSubscriber_IsCollectable_WhileHubLives()
    {
        var hub = new MessageHub();
        var weak = SubscribeAndDrop(hub);
        Assert.True(LeakProbe.IsCollected(weak), "Weak MessageHub subscriber was not collected.");
    }

    [Fact]
    public void StrongSubscriber_StaysAlive_UntilUnsubscribe()
    {
        var hub = new MessageHub();
        var (pinned, afterUnsubscribe) = SubscribeStrongThenUnsubscribe(hub);
        Assert.True(pinned, "Strong subscription should pin the subscriber.");
        Assert.True(LeakProbe.IsCollected(afterUnsubscribe), "Subscriber should collect after Unsubscribe.");
    }

    [Fact]
    public void Publish_DoesNotInvokeCollectedWeakSubscriber()
    {
        var hub = new MessageHub();
        var seen = 0;
        SubscribeCounter(hub, () => seen++);
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        hub.Publish("x");
        Assert.Equal(0, seen);
    }

    private static (bool Pinned, WeakReference AfterUnsubscribe) SubscribeStrongThenUnsubscribe(MessageHub hub)
    {
        var vm = new ProbeViewModel();
        hub.Subscribe<ProbeViewModel, string>(vm, static (r, m) => r.Name = m, weak: false);
        var weak = LeakProbe.Track(vm);
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        var pinned = weak.IsAlive;
        hub.Unsubscribe(vm);
        return (pinned, weak);
    }

    private static WeakReference SubscribeAndDrop(MessageHub hub)
    {
        var vm = new ProbeViewModel();
        hub.Subscribe<ProbeViewModel, string>(vm, static (r, m) => r.Name = m, weak: true);
        var weak = LeakProbe.Track(vm);
        return weak;
    }

    private static void SubscribeCounter(MessageHub hub, Action onMessage)
    {
        var vm = new ProbeViewModel();
        hub.Subscribe<ProbeViewModel, string>(vm, (_, _) => onMessage(), weak: true);
    }
}
