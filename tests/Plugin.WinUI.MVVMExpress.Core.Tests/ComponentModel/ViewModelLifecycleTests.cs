using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Core.Tests.Support;
using Plugin.WinUI.MVVMExpress.State;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.ComponentModel;

public sealed class ViewModelLifecycleTests
{
    [Fact]
    public async Task Lifecycle_InitializeAppearDisappear_CanBeCalled()
    {
        var vm = new ProbeViewModel();
        await vm.InitializeAsync();
        await vm.OnAppearingAsync();
        await vm.OnDisappearingAsync();
        Assert.Equal(1, vm.AppearCount);
        Assert.False(vm.ViewModelCancellationToken.IsCancellationRequested);
    }

    [Fact]
    public void ViewModel_ImplementsIViewModel()
    {
        IViewModel vm = new ProbeViewModel();
        Assert.Equal(ViewModelStatus.Idle, vm.Status);
        Assert.False(vm.IsBusy);
    }

    [Fact]
    public void StatusLoading_IsBusy()
    {
        var vm = new DependentViewModel();
        vm.MarkLoading();
        Assert.True(vm.IsBusy);
        Assert.Equal(ViewModelStatus.Loading, vm.Status);
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var vm = new ProbeViewModel();
        vm.Dispose();
        vm.Dispose();
        Assert.True(vm.IsDisposed);
        Assert.True(vm.ViewModelCancellationToken.IsCancellationRequested);
    }

    [Fact]
    public async Task DisposeAsync_CancelsToken()
    {
        var vm = new ProbeViewModel();
        await vm.DisposeAsync();
        Assert.True(vm.IsDisposed);
        Assert.True(vm.ViewModelCancellationToken.IsCancellationRequested);
    }

    [Theory]
    [InlineData(ViewModelStatus.Refreshing)]
    [InlineData(ViewModelStatus.Saving)]
    public void WorkingStatuses_AreBusy(ViewModelStatus status)
    {
        var vm = new DependentViewModel();
        if (status == ViewModelStatus.Refreshing)
        {
            vm.MarkRefreshing();
        }
        else
        {
            vm.MarkSaving();
        }

        Assert.True(vm.IsBusy);
        vm.MarkIdle();
        Assert.False(vm.IsBusy);
    }
}
