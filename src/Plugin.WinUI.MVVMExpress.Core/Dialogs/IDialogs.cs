using Plugin.WinUI.MVVMExpress.Outcome;

namespace Plugin.WinUI.MVVMExpress.Dialogs;

/// <summary>ViewModel-facing dialogs. Never call <c>Page.DisplayAlert</c> from a ViewModel.</summary>
public interface IDialogs
{
    /// <summary>Shows an alert.</summary>
    Task AlertAsync(string title, string message, string cancel = "OK", CancellationToken cancellationToken = default);

    /// <summary>Shows a confirm dialog.</summary>
    Task<bool> ConfirmAsync(string title, string message, string accept = "OK", string cancel = "Cancel", CancellationToken cancellationToken = default);

    /// <summary>Shows an error.</summary>
    Task ErrorAsync(ErrorInfo error, CancellationToken cancellationToken = default);
}

/// <summary>In-app notifications (toast / snackbar).</summary>
public interface INotifier
{
    /// <summary>Shows a short message.</summary>
    Task ToastAsync(string message, TimeSpan? duration = null, CancellationToken cancellationToken = default);
}
