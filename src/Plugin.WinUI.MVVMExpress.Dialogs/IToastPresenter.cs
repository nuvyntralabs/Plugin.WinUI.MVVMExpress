namespace Plugin.WinUI.MVVMExpress.Dialogs;

/// <summary>Shows a short-lived in-app toast. Tests inject a recording implementation.</summary>
public interface IToastPresenter
{
    Task ShowAsync(string message, TimeSpan duration, CancellationToken cancellationToken = default);
}
