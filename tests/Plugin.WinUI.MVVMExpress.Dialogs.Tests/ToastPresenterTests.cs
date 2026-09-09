using Plugin.WinUI.MVVMExpress.Dialogs;
using Plugin.WinUI.MVVMExpress.Navigation;
using Plugin.WinUI.MVVMExpress.Threading;

namespace Plugin.WinUI.MVVMExpress.Dialogs.Tests;

public sealed class ToastPresenterTests
{
    [Fact]
    public async Task Toast_WithoutOwnerWindow_DoesNotThrow()
    {
        var presenter = new WinUIToastPresenter(WindowContext.Default, ImmediateMainThread.Instance);
        await presenter.ShowAsync("Saved", TimeSpan.FromMilliseconds(1));
    }
}
