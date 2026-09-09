using Plugin.WinUI.MVVMExpress.Outcome;

namespace Plugin.WinUI.MVVMExpress.Dialogs;

/// <summary>No-op dialogs for <c>net10.0</c> hosts and tests that do not assert UI.</summary>
public sealed class NullDialogs : IDialogs, INotifier
{
    /// <summary>Shared instance.</summary>
    public static NullDialogs Instance { get; } = new();

    /// <inheritdoc />
    public Task AlertAsync(string title, string message, string cancel = "OK", CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> ConfirmAsync(string title, string message, string accept = "OK", string cancel = "Cancel", CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task ErrorAsync(ErrorInfo error, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(error);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ToastAsync(string message, TimeSpan? duration = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}
