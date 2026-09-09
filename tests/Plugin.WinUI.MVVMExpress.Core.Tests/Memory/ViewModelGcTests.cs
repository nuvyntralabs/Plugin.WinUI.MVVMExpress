using Plugin.WinUI.MVVMExpress.Core.Tests.Support;
using Plugin.WinUI.MVVMExpress.Testing;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Memory;

public sealed class ViewModelGcTests
{
    [Fact]
    public void DisposedViewModel_IsCollectable()
    {
        var weak = CreateAndDispose();
        Assert.True(LeakProbe.IsCollected(weak), "ViewModel was not collected after Dispose.");
    }

    [Theory]
    [InlineData(ApplicationScale.Small)]
    [InlineData(ApplicationScale.Mid)]
    [InlineData(ApplicationScale.Large)]
    public void DisposedViewModelBatch_IsCollectable(ApplicationScale scale)
    {
        var weaks = CreateBatch(ScaleProfile.ViewModelBatch(scale));
        var survivors = weaks.Count(w => !LeakProbe.IsCollected(w, rounds: 4));
        Assert.Equal(0, survivors);
    }

    [Fact]
    public async Task Dispose_CancelsLifetimeToken()
    {
        var vm = new ProbeViewModel();
        var token = vm.ViewModelCancellationToken;
        Assert.False(token.IsCancellationRequested);
        await vm.OnAppearingAsync();
        vm.Dispose();
        Assert.True(token.IsCancellationRequested);
        Assert.Equal(1, vm.AppearCount);
    }

    private static WeakReference CreateAndDispose()
    {
        var vm = new ProbeViewModel();
        vm.Name = "x";
        vm.Dispose();
        var weak = LeakProbe.Track(vm);
        return weak;
    }

    private static WeakReference[] CreateBatch(int count)
    {
        var weaks = new WeakReference[count];
        for (var i = 0; i < count; i++)
        {
            var vm = new ProbeViewModel();
            vm.Name = i.ToString();
            vm.Dispose();
            weaks[i] = LeakProbe.Track(vm);
        }

        return weaks;
    }
}
