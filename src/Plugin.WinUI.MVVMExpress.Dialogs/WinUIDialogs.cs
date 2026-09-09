using Microsoft.UI.Xaml.Controls;
using Plugin.WinUI.MVVMExpress.Hosting;
using Plugin.WinUI.MVVMExpress.Navigation;
using Plugin.WinUI.MVVMExpress.Outcome;
using Plugin.WinUI.MVVMExpress.Threading;

namespace Plugin.WinUI.MVVMExpress.Dialogs;

/// <summary>WinUI <see cref="ContentDialog"/> adapter. Sets <c>XamlRoot</c> from the owner window.</summary>
public sealed class WinUIDialogs : IDialogs
{
    private readonly IWindowContext _window;
    private readonly IMainThread _main;

    public WinUIDialogs(IWindowContext? window = null, IMainThread? mainThread = null)
    {
        _window = window ?? WindowContext.Default;
        _main = mainThread ?? NotificationMarshaller.Current ?? ImmediateMainThread.Instance;
    }

    public Task AlertAsync(string title, string message, string cancel = "OK", CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _main.InvokeAsync(() => ShowAsync(title, message, cancel, accept: null), cancellationToken);
    }

    public async Task<bool> ConfirmAsync(string title, string message, string accept = "OK", string cancel = "Cancel", CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var accepted = false;
        await _main.InvokeAsync(
            async () => accepted = await ShowAsync(title, message, cancel, accept).ConfigureAwait(true),
            cancellationToken).ConfigureAwait(false);
        return accepted;
    }

    public Task ErrorAsync(ErrorInfo error, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(error);
        return AlertAsync("Error", error.Message, cancellationToken: cancellationToken);
    }

    private async Task<bool> ShowAsync(string title, string message, string cancel, string? accept)
    {
        var owner = WinUIWindowContext.TryGetWindow(_window);
        var xamlRoot = (owner?.Content as Microsoft.UI.Xaml.FrameworkElement)?.XamlRoot;
        if (xamlRoot is null)
        {
            return false;
        }

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = cancel,
            PrimaryButtonText = accept,
            XamlRoot = xamlRoot
        };
        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }
}
