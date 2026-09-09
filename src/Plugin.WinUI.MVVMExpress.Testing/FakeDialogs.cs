using Plugin.WinUI.MVVMExpress.Dialogs;
using Plugin.WinUI.MVVMExpress.Outcome;

namespace Plugin.WinUI.MVVMExpress.Testing;

/// <summary>Records dialog calls.</summary>
public sealed class FakeDialogs : IDialogs, INotifier
{
    /// <summary>Recorded titles and messages.</summary>
    public List<string> Alerts { get; } = [];

    /// <summary>Value returned from <see cref="ConfirmAsync"/>.</summary>
    public bool ConfirmResult { get; set; } = true;

    /// <inheritdoc />
    public Task AlertAsync(string title, string message, string cancel = "OK", CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Alerts.Add($"{title}:{message}");
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> ConfirmAsync(string title, string message, string accept = "OK", string cancel = "Cancel", CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Alerts.Add($"confirm:{title}");
        return Task.FromResult(ConfirmResult);
    }

    /// <inheritdoc />
    public Task ErrorAsync(ErrorInfo error, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(error);
        return AlertAsync("Error", error.Message, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task ToastAsync(string message, TimeSpan? duration = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Alerts.Add($"toast:{message}");
        return Task.CompletedTask;
    }
}
