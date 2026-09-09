using Plugin.WinUI.MVVMExpress.Busy;
using Plugin.WinUI.MVVMExpress.Dialogs;
using Plugin.WinUI.MVVMExpress.Errors;
using Plugin.WinUI.MVVMExpress.Navigation;
using Plugin.WinUI.MVVMExpress.Threading;

namespace Plugin.WinUI.MVVMExpress.ComponentModel;

/// <summary>Page-scoped ViewModel with navigator/dialogs and navigation guards.</summary>
public abstract class PageViewModel : ViewModel, INavigable
{
    private Outcome.Outcome? _lastNavigation;

    /// <summary>Creates a page ViewModel.</summary>
    /// <param name="navigator">Optional navigator.</param>
    /// <param name="dialogs">Optional dialogs.</param>
    /// <param name="errors">Optional unexpected-error sink.</param>
    /// <param name="busy">Optional nested busy gate.</param>
    /// <param name="mainThread">Optional UI dispatcher.</param>
    protected PageViewModel(
        INavigator? navigator = null,
        IDialogs? dialogs = null,
        IErrorSink? errors = null,
        IBusyGate? busy = null,
        IMainThread? mainThread = null)
        : base(errors, busy, mainThread)
    {
        Navigator = navigator;
        Dialogs = dialogs;
        MainThread = mainThread;
    }

    /// <summary>Typed navigation service.</summary>
    protected INavigator? Navigator { get; }

    /// <summary>ViewModel-facing dialogs.</summary>
    protected IDialogs? Dialogs { get; }

    /// <summary>UI dispatcher when injected (optional, same style as <see cref="Dialogs"/>).</summary>
    protected IMainThread? MainThread { get; }

    /// <summary>Last <see cref="INavigator"/> outcome recorded by <see cref="TrackNavigation"/>.</summary>
    public Outcome.Outcome? LastNavigation
    {
        get => _lastNavigation;
        private set => SetProperty(ref _lastNavigation, value);
    }

    /// <summary>Stores <paramref name="outcome"/> for binding and optional dialog / error-sink forwarding.</summary>
    protected async Task<Outcome.Outcome> TrackNavigation(
        Outcome.Outcome outcome,
        bool forwardFailures = true,
        CancellationToken cancellationToken = default)
    {
        LastNavigation = outcome;
        if (forwardFailures && !outcome.IsSuccess && outcome.Error is { } error)
        {
            await Errors.HandleAsync(error, cancellationToken).ConfigureAwait(false);
            if (Dialogs is { } dialogs)
            {
                await dialogs.ErrorAsync(error, cancellationToken).ConfigureAwait(false);
            }
        }

        return outcome;
    }

    /// <inheritdoc />
    public virtual Task OnNavigatedToAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task OnNavigatedFromAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task<bool> CanNavigateAwayAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(true);
}
