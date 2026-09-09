using Microsoft.UI.Xaml.Controls;
using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Navigation;
using Plugin.WinUI.MVVMExpress.Threading;

namespace Plugin.WinUI.MVVMExpress.Navigation.Tests;

public sealed class FrameNavigatorTests
{
    [Fact]
    public async Task ResetAsync_WithoutFrame_FailsWithE_PAGE()
    {
        var navigator = new WinUIFrameNavigator(frame: () => null, mainThread: ImmediateMainThread.Instance)
            .Map<HomeViewModel, HomeView>("home");

        var result = await navigator.ResetAsync<HomeViewModel>();

        Assert.False(result.IsSuccess);
        Assert.Equal("E_PAGE", result.Error?.Code);
    }

    [Fact]
    public async Task Navigate_ConstructsView_OnMainThread()
    {
        var thread = new RecordingMainThread();
        var navigator = new WinUIFrameNavigator(frame: () => null, mainThread: thread)
            .Map<HomeViewModel, HomeView>("home");

        await navigator.ResetAsync<HomeViewModel>();

        Assert.True(thread.Invoked);
    }

    private sealed class HomeViewModel : ViewModel
    {
    }

    private sealed class HomeView : UserControl
    {
    }

    private sealed class RecordingMainThread : IMainThread
    {
        public bool Invoked { get; private set; }

        public bool IsMainThread => true;

        public void BeginInvoke(Action action)
        {
            Invoked = true;
            action();
        }

        public Task InvokeAsync(Action action, CancellationToken cancellationToken = default)
        {
            Invoked = true;
            action();
            return Task.CompletedTask;
        }

        public Task InvokeAsync(Func<Task> action, CancellationToken cancellationToken = default)
        {
            Invoked = true;
            return action();
        }
    }
}
