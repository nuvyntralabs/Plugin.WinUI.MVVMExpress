using Plugin.WinUI.MVVMExpress.Navigation;

namespace Plugin.WinUI.MVVMExpress.Dialogs;

/// <summary><see cref="INotifier"/> that shows an overlay toast.</summary>
public sealed class WinUINotifier : INotifier
{
    private readonly IToastPresenter _presenter;

    public WinUINotifier(IToastPresenter? presenter = null, IWindowContext? window = null)
        => _presenter = presenter ?? new WinUIToastPresenter(window);

    public Task ToastAsync(string message, TimeSpan? duration = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        cancellationToken.ThrowIfCancellationRequested();
        return _presenter.ShowAsync(message, duration ?? TimeSpan.FromSeconds(2), cancellationToken);
    }
}
