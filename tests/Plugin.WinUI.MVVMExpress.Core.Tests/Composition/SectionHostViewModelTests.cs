using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Composition;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Composition;

public sealed class SectionHostViewModelTests
{
    [Fact]
    public async Task Select_SwitchesWithoutNavigator_AndForwardsLifecycle()
    {
        var host = new SectionHostViewModel();
        var chats = host.Add("chats", new ProbeSection());
        var calls = host.Add("calls", new ProbeSection());
        await host.InitializeAsync();
        Assert.Equal("chats", host.CurrentKey);
        Assert.Same(chats, host.Current);
        Assert.Equal(1, chats.Appears);
        Assert.Equal(0, calls.Appears);

        await host.SelectAsync("calls");
        Assert.Equal("calls", host.CurrentKey);
        Assert.True(host.IsCurrent("calls"));
        Assert.Equal(1, chats.Disappears);
        Assert.Equal(1, calls.Appears);

        await host.SelectAsync("calls");
        Assert.Equal(1, calls.Appears);
    }

    [Fact]
    public void DuplicateKey_Throws()
    {
        var host = new SectionHostViewModel();
        host.Add("chats", new ProbeSection());
        Assert.Throws<ArgumentException>(() => host.Add("chats", new ProbeSection()));
    }

    private sealed class ProbeSection : ViewModel
    {
        public int Appears { get; private set; }

        public int Disappears { get; private set; }

        public override Task OnAppearingAsync(CancellationToken cancellationToken = default)
        {
            Appears++;
            return Task.CompletedTask;
        }

        public override Task OnDisappearingAsync(CancellationToken cancellationToken = default)
        {
            Disappears++;
            return Task.CompletedTask;
        }
    }
}
