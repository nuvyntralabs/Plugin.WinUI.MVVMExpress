using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Testing;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Testing;

public sealed class ViewModelLifecycleDriverTests
{
    [Fact]
    public async Task AppearAsync_InitializesOnce_ThenAppears()
    {
        var vm = new Probe();
        await vm.AppearAsync();
        await vm.AppearAsync();
        Assert.Equal(1, vm.InitCount);
        Assert.Equal(2, vm.AppearCount);
    }

    [Fact]
    public async Task DisappearAsync_Forwards()
    {
        var vm = new Probe();
        await vm.AppearAsync();
        await vm.DisappearAsync();
        Assert.Equal(1, vm.DisappearCount);
    }

    [Fact]
    public async Task AppearAsync_Null_Throws()
    {
        IViewModel? vm = null;
        await Assert.ThrowsAsync<ArgumentNullException>(() => vm!.AppearAsync());
    }

    private sealed class Probe : ViewModel
    {
        public int InitCount { get; private set; }

        public int AppearCount { get; private set; }

        public int DisappearCount { get; private set; }

        public override Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            InitCount++;
            return Task.CompletedTask;
        }

        public override Task OnAppearingAsync(CancellationToken cancellationToken = default)
        {
            AppearCount++;
            return Task.CompletedTask;
        }

        public override Task OnDisappearingAsync(CancellationToken cancellationToken = default)
        {
            DisappearCount++;
            return Task.CompletedTask;
        }
    }
}
